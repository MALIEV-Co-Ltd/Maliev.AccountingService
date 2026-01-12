using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Api.DTOs.Responses;
using Asp.Versioning;
using Maliev.AccountingService.Api.Extensions;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Maliev.AccountingService.Api.Services;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// Controller for managing journal entries
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("accounting/v{version:apiVersion}/journal-entries")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly AccountingDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IPeriodService _periodService;
    private readonly ILogger<JournalEntriesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JournalEntriesController"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="periodService">The period service.</param>
    /// <param name="logger">The logger.</param>
    public JournalEntriesController(
        AccountingDbContext context,
        IAuditService auditService,
        IPeriodService periodService,
        ILogger<JournalEntriesController> logger)
    {
        _context = context;
        _auditService = auditService;
        _periodService = periodService;
        _logger = logger;
    }

    /// <summary>
    /// Get journal entries with optional filtering
    /// </summary>
    /// <param name="startDate">Filter by start date</param>
    /// <param name="endDate">Filter by end date</param>
    /// <param name="accountId">Filter by account</param>
    /// <param name="customerId">Filter by customer</param>
    /// <param name="supplierId">Filter by supplier</param>
    /// <param name="status">Filter by status (Draft, Posted)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 50)</param>
    [HttpGet]
    [RequirePermission(AccountingPermissions.JournalEntriesRead)]
    public async Task<ActionResult<IEnumerable<JournalEntryResponse>>> GetJournalEntries(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? accountId,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? supplierId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = _context.JournalEntries
            .AsNoTracking()
            .Include(j => j.Period)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Include(j => j.Lines)
                .ThenInclude(l => l.TaxComponents)
            .AsSplitQuery()
            .AsQueryable();

        // Apply filters
        if (startDate.HasValue)
        {
            var utcStart = startDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc)
                : startDate.Value.ToUniversalTime();
            query = query.Where(j => j.EntryDate >= utcStart);
        }

        if (endDate.HasValue)
        {
            var utcEnd = (endDate.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc)
                : endDate.Value.ToUniversalTime()).Date.AddDays(1).AddTicks(-1);
            query = query.Where(j => j.EntryDate <= utcEnd);
        }

        if (accountId.HasValue)
        {
            query = query.Where(j => j.Lines.Any(l => l.AccountId == accountId.Value));
        }

        if (customerId.HasValue)
        {
            query = query.Where(j => j.Lines.Any(l => l.CustomerId == customerId.Value));
        }

        if (supplierId.HasValue)
        {
            query = query.Where(j => j.Lines.Any(l => l.SupplierId == supplierId.Value));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EntryStatus>(status, true, out var entryStatus))
        {
            query = query.Where(j => j.Status == entryStatus);
        }

        // Pagination - OrderBy MUST be before Skip/Take
        var entries = await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var response = entries.Select(e => e.ToResponse()).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Get a specific journal entry by ID
    /// </summary>
    [HttpGet("{id}")]
    [RequirePermission(AccountingPermissions.JournalEntriesRead)]
    public async Task<ActionResult<JournalEntryResponse>> GetJournalEntry(Guid id)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Period)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Include(j => j.Lines)
                .ThenInclude(l => l.TaxComponents)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (entry == null)
        {
            return NotFound(new { message = $"Journal entry {id} not found" });
        }

        return Ok(entry.ToResponse());
    }

    /// <summary>
    /// Create a new draft journal entry
    /// </summary>
    [HttpPost]
    [RequirePermission(AccountingPermissions.JournalEntriesCreate)]
    public async Task<ActionResult<JournalEntryResponse>> CreateJournalEntry(
        [FromBody] CreateJournalEntryRequest request)
    {
        // Validate balanced entry
        var totalDebit = request.Lines.Sum(l => l.DebitAmount);
        var totalCredit = request.Lines.Sum(l => l.CreditAmount);

        if (totalDebit != totalCredit)
        {
            return BadRequest(new
            {
                message = "Journal entry is not balanced",
                totalDebit,
                totalCredit,
                difference = totalDebit - totalCredit
            });
        }

        // Get or create period
        var period = await _periodService.GetOrCreatePeriodAsync(request.EntryDate);

        // Validate period is open
        await _periodService.ValidatePeriodForPostingAsync(period.Id, isAdjustingEntry: false);

        // Get current user
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdClaim, out var guid) ? guid : Guid.Empty;

        // Use execution strategy for retries with user-initiated transactions
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var journalEntry = new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    PeriodId = period.Id,
                    EntryNumber = await _periodService.GenerateEntryNumberAsync(period.Id),
                    EntryDate = request.EntryDate.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(request.EntryDate, DateTimeKind.Utc)
                        : request.EntryDate.ToUniversalTime(),
                    Description = request.Description,
                    Status = EntryStatus.Draft,
                    CreatedBy = userId,
                    TotalDebit = totalDebit,
                    TotalCredit = totalCredit
                };

                // Pre-fetch all referenced account IDs to avoid N+1 queries
                var accountIds = request.Lines.Select(l => l.AccountId).Distinct().ToList();
                var accounts = await _context.ChartOfAccounts
                    .Where(a => accountIds.Contains(a.Id) && a.IsActive)
                    .ToDictionaryAsync(a => a.Id);

                int lineSequence = 1;
                foreach (var lineRequest in request.Lines)
                {
                    // Validate account exists and is active
                    if (!accounts.TryGetValue(lineRequest.AccountId, out var account))
                    {
                        return (ActionResult<JournalEntryResponse>)BadRequest(new { message = $"Account {lineRequest.AccountId} not found or is inactive" });
                    }

                    var line = new JournalEntryLine
                    {
                        Id = Guid.NewGuid(),
                        JournalEntryId = journalEntry.Id,
                        AccountId = lineRequest.AccountId,
                        LineSequence = lineSequence++,
                        Description = lineRequest.Description,
                        DebitAmount = lineRequest.DebitAmount,
                        CreditAmount = lineRequest.CreditAmount,
                        CustomerId = lineRequest.CustomerId,
                        SupplierId = lineRequest.SupplierId,
                        ReferenceId = lineRequest.Reference
                    };

                    // Add tax components if present
                    if (lineRequest.TaxComponents != null)
                    {
                        foreach (var taxRequest in lineRequest.TaxComponents)
                        {
                            line.TaxComponents.Add(new TaxComponent
                            {
                                Id = Guid.NewGuid(),
                                JournalEntryLineId = line.Id,
                                TaxType = taxRequest.TaxType,
                                TaxRate = taxRequest.TaxRate,
                                TaxableAmount = taxRequest.TaxableAmount,
                                TaxAmount = taxRequest.TaxAmount
                            });
                        }
                    }

                    journalEntry.Lines.Add(line);
                }

                _context.JournalEntries.Add(journalEntry);
                await _context.SaveChangesAsync();

                // Record audit trail
                await _auditService.RecordAuditAsync(
                    "JournalEntry",
                    journalEntry.Id.ToString(),
                    "Created",
                    null,
                    journalEntry,
                    userId.ToString(),
                    HttpContext.TraceIdentifier,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    HttpContext.RequestAborted);

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Created draft journal entry {JournalEntryId} by user {UserId}",
                    journalEntry.Id,
                    userId);

                // Reload with includes for response
                var createdEntry = await _context.JournalEntries
                    .Include(j => j.Period)
                    .Include(j => j.Lines)
                        .ThenInclude(l => l.Account)
                    .Include(j => j.Lines)
                        .ThenInclude(l => l.TaxComponents)
                    .FirstAsync(j => j.Id == journalEntry.Id);

                return CreatedAtAction(
                    nameof(GetJournalEntry),
                    new { id = journalEntry.Id },
                    createdEntry.ToResponse());
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    /// <summary>
    /// Post a draft journal entry to the ledger
    /// </summary>
    [HttpPost("{id}/post")]
    [RequirePermission(AccountingPermissions.JournalEntriesPost)]
    public async Task<ActionResult<JournalEntryResponse>> PostJournalEntry(Guid id)
    {
        var entry = await _context.JournalEntries
            .Include(j => j.Period)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Include(j => j.Lines)
                .ThenInclude(l => l.TaxComponents)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (entry == null)
        {
            return NotFound(new { message = $"Journal entry {id} not found" });
        }

        if (entry.Status == EntryStatus.Posted)
        {
            return BadRequest(new { message = "Journal entry is already posted" });
        }

        // Verify period is still open
        await _periodService.ValidatePeriodForPostingAsync(entry.PeriodId, isAdjustingEntry: false);

        // Verify balanced
        if (entry.TotalDebit != entry.TotalCredit)
        {
            return BadRequest(new { message = "Journal entry is not balanced" });
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = Guid.TryParse(userIdClaim, out var guid) ? guid : Guid.Empty;
        var beforeState = entry.Status;

        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                entry.Status = EntryStatus.Posted;
                entry.PostedAt = DateTime.UtcNow;
                entry.PostedBy = userId;

                await _context.SaveChangesAsync();

                // Record audit trail
                await _auditService.RecordAuditAsync(
                    "JournalEntry",
                    entry.Id.ToString(),
                    "Posted",
                    beforeState,
                    entry.Status,
                    userId.ToString(),
                    HttpContext.TraceIdentifier,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    HttpContext.RequestAborted);

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Posted journal entry {JournalEntryId} by user {UserId}",
                    entry.Id,
                    userId);

                return Ok(entry.ToResponse());
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
