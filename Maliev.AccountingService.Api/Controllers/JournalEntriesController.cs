using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Api.DTOs.Responses;
using Maliev.AccountingService.Api.Extensions;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Maliev.AccountingService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maliev.AccountingService.Api.Controllers;

/// <summary>
/// Controller for managing journal entries
/// </summary>
[ApiController]
[Route("accounting/v{version:apiVersion}/journal-entries")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly AccountingDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<JournalEntriesController> _logger;

    public JournalEntriesController(
        AccountingDbContext context,
        IAuditService auditService,
        ILogger<JournalEntriesController> logger)
    {
        _context = context;
        _auditService = auditService;
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
            .Include(j => j.Period)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Include(j => j.Lines)
                .ThenInclude(l => l.TaxComponents)
            .AsQueryable();

        // Apply filters
        if (startDate.HasValue)
        {
            query = query.Where(j => j.EntryDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(j => j.EntryDate <= endDate.Value);
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

        // Pagination
        var entries = await query
            .OrderByDescending(j => j.EntryDate)
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
    [Authorize(Roles = "accountant,financial_controller")]
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
        var period = await GetOrCreatePeriodAsync(request.EntryDate);

        if (period.Status != PeriodStatus.Open)
        {
            return BadRequest(new { message = $"Cannot post to closed period {period.Name}" });
        }

        // Get current user
        var userId = Guid.Empty; // TODO: Get from claims

        var journalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            PeriodId = period.Id,
            EntryNumber = await GenerateEntryNumberAsync(period.Id),
            EntryDate = request.EntryDate,
            Description = request.Description,
            Status = EntryStatus.Draft,
            CreatedBy = userId,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit
        };

        int lineSequence = 1;
        foreach (var lineRequest in request.Lines)
        {
            // Validate account exists and is active
            var account = await _context.ChartOfAccounts
                .FirstOrDefaultAsync(a => a.Id == lineRequest.AccountId && a.IsActive);

            if (account == null)
            {
                return BadRequest(new { message = $"Account {lineRequest.AccountId} not found or is inactive" });
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

    /// <summary>
    /// Post a draft journal entry to the ledger
    /// </summary>
    [HttpPost("{id}/post")]
    [Authorize(Roles = "accountant,financial_controller")]
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
        if (entry.Period.Status != PeriodStatus.Open)
        {
            return BadRequest(new { message = $"Cannot post to closed period {entry.Period.Name}" });
        }

        // Verify balanced
        if (entry.TotalDebit != entry.TotalCredit)
        {
            return BadRequest(new { message = "Journal entry is not balanced" });
        }

        var userId = Guid.Empty; // TODO: Get from claims
        var beforeState = entry.Status;

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

        _logger.LogInformation(
            "Posted journal entry {JournalEntryId} by user {UserId}",
            entry.Id,
            userId);

        return Ok(entry.ToResponse());
    }

    private async Task<FinancialPeriod> GetOrCreatePeriodAsync(DateTime transactionDate)
    {
        var year = transactionDate.Year;
        var month = transactionDate.Month;
        var periodName = $"{year}-{month:D2}";

        var period = await _context.FinancialPeriods
            .FirstOrDefaultAsync(p => p.Name == periodName);

        if (period == null)
        {
            // Get or create fiscal year
            var fiscalYearName = year.ToString();
            var fiscalYear = await _context.FiscalYears
                .FirstOrDefaultAsync(fy => fy.Name == fiscalYearName);

            if (fiscalYear == null)
            {
                fiscalYear = new FiscalYear
                {
                    Id = Guid.NewGuid(),
                    Name = fiscalYearName,
                    StartDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    EndDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc),
                    PeriodStructure = PeriodStructure.Monthly,
                    IsActive = true
                };
                _context.FiscalYears.Add(fiscalYear);
                await _context.SaveChangesAsync();
            }

            period = new FinancialPeriod
            {
                Id = Guid.NewGuid(),
                FiscalYearId = fiscalYear.Id,
                Name = periodName,
                StartDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59, DateTimeKind.Utc),
                Status = PeriodStatus.Open
            };

            _context.FinancialPeriods.Add(period);
            await _context.SaveChangesAsync();
        }

        return period;
    }

    private async Task<string> GenerateEntryNumberAsync(Guid periodId)
    {
        var period = await _context.FinancialPeriods.FindAsync(periodId);
        if (period == null)
        {
            throw new InvalidOperationException($"Period {periodId} not found");
        }

        var count = await _context.JournalEntries
            .CountAsync(j => j.PeriodId == periodId);

        return $"JE-{period.Name}-{(count + 1):D5}";
    }
}
