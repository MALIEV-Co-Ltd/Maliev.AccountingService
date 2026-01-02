using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Api.DTOs.Responses;
using Maliev.AccountingService.Api.Extensions;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service implementation for chart of accounts management
/// </summary>
public sealed class ChartOfAccountsService : IChartOfAccountsService
{
    private readonly AccountingDbContext _dbContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<ChartOfAccountsService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChartOfAccountsService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="auditService">The audit service.</param>
    /// <param name="logger">The logger.</param>
    public ChartOfAccountsService(
        AccountingDbContext dbContext,
        IAuditService auditService,
        ILogger<ChartOfAccountsService> logger)
    {
        _dbContext = dbContext;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all accounts with optional filtering.
    /// </summary>
    /// <param name="accountType">The optional account type filter.</param>
    /// <param name="includeInactive">If true, includes inactive accounts.</param>
    /// <returns>A list of chart of account responses.</returns>
    public async Task<List<ChartOfAccountResponse>> GetAllAccountsAsync(string? accountType = null, bool includeInactive = false)
    {
        var query = _dbContext.ChartOfAccounts
            .Include(a => a.ParentAccount)
            .AsQueryable();

        if (!string.IsNullOrEmpty(accountType))
        {
            if (Enum.TryParse<AccountType>(accountType, out var parsedType))
            {
                query = query.Where(a => a.Type == parsedType);
            }
        }

        if (!includeInactive)
        {
            query = query.Where(a => a.IsActive);
        }

        var accounts = await query
            .OrderBy(a => a.AccountNumber)
            .ToListAsync();

        return accounts.ToResponse();
    }

    /// <summary>
    /// Retrieves the account hierarchy with optional filtering.
    /// </summary>
    /// <param name="accountType">The optional account type filter.</param>
    /// <returns>A hierarchical list of chart of account responses.</returns>
    public async Task<List<ChartOfAccountResponse>> GetAccountHierarchyAsync(string? accountType = null)
    {
        // Load all active accounts with their children
        var query = _dbContext.ChartOfAccounts
            .Include(a => a.ChildAccounts)
            .Where(a => a.IsActive);

        if (!string.IsNullOrEmpty(accountType))
        {
            if (Enum.TryParse<AccountType>(accountType, out var parsedType))
            {
                query = query.Where(a => a.Type == parsedType);
            }
        }

        var allAccounts = await query
            .OrderBy(a => a.AccountNumber)
            .ToListAsync();

        // Build hierarchical structure - start with root accounts (no parent)
        var accountsByParent = allAccounts.ToLookup(a => a.ParentAccountId);

        var rootAccounts = accountsByParent[null]
            .Select(a => BuildAccountHierarchy(a, accountsByParent))
            .ToList();

        return rootAccounts;
    }

    private ChartOfAccountResponse BuildAccountHierarchy(ChartOfAccount account, ILookup<Guid?, ChartOfAccount> accountsByParent)
    {
        var response = account.ToResponse();

        // Find and build children recursively using lookup
        var children = accountsByParent[account.Id]
            .Select(child => BuildAccountHierarchy(child, accountsByParent))
            .ToList();

        if (children.Any())
        {
            response = response with { Children = children };
        }

        return response;
    }

    /// <summary>
    /// Retrieves an account by its ID.
    /// </summary>
    /// <param name="id">The account ID.</param>
    /// <returns>The chart of account response if found; otherwise, null.</returns>
    public async Task<ChartOfAccountResponse?> GetAccountByIdAsync(Guid id)
    {
        var account = await _dbContext.ChartOfAccounts
            .Include(a => a.ParentAccount)
            .Include(a => a.ChildAccounts)
            .FirstOrDefaultAsync(a => a.Id == id);

        return account?.ToResponse();
    }

    /// <summary>
    /// Retrieves an account by its account number.
    /// </summary>
    /// <param name="accountNumber">The account number.</param>
    /// <returns>The chart of account response if found; otherwise, null.</returns>
    public async Task<ChartOfAccountResponse?> GetAccountByNumberAsync(string accountNumber)
    {
        var account = await _dbContext.ChartOfAccounts
            .Include(a => a.ParentAccount)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        return account?.ToResponse();
    }

    /// <summary>
    /// Creates a new account.
    /// </summary>
    /// <param name="request">The create account request.</param>
    /// <returns>The created chart of account response.</returns>
    public async Task<ChartOfAccountResponse> CreateAccountAsync(CreateChartOfAccountRequest request)
    {
        // Validate account number uniqueness
        var exists = await _dbContext.ChartOfAccounts
            .AnyAsync(a => a.AccountNumber == request.AccountNumber);

        if (exists)
        {
            throw new InvalidOperationException($"Account number '{request.AccountNumber}' already exists");
        }

        // Validate parent account if specified
        if (request.ParentAccountId.HasValue)
        {
            var parentAccount = await _dbContext.ChartOfAccounts
                .FindAsync(request.ParentAccountId.Value);

            if (parentAccount == null)
            {
                throw new InvalidOperationException($"Parent account with ID '{request.ParentAccountId}' not found");
            }

            // Validate parent account type compatibility
            if (!Enum.TryParse<AccountType>(request.Type, out var accountType))
            {
                throw new ArgumentException($"Invalid account type: {request.Type}");
            }

            if (parentAccount.Type != accountType)
            {
                throw new InvalidOperationException(
                    $"Parent account type ({parentAccount.Type}) must match child account type ({accountType})");
            }
        }

        var account = request.ToEntity();

        _dbContext.ChartOfAccounts.Add(account);
        await _dbContext.SaveChangesAsync();

        await _auditService.RecordAuditAsync(
            entityType: nameof(ChartOfAccount),
            entityId: account.Id.ToString(),
            action: "Created",
            beforeState: null,
            afterState: account,
            userId: null,
            correlationId: null,
            ipAddress: null);

        _logger.LogInformation(
            "Created chart of account: {AccountNumber} - {Name}",
            account.AccountNumber,
            account.Name);

        return account.ToResponse();
    }

    /// <summary>
    /// Updates an existing account.
    /// </summary>
    /// <param name="id">The account ID.</param>
    /// <param name="request">The update account request.</param>
    /// <returns>The updated chart of account response.</returns>
    public async Task<ChartOfAccountResponse> UpdateAccountAsync(Guid id, UpdateChartOfAccountRequest request)
    {
        var account = await _dbContext.ChartOfAccounts
            .Include(a => a.ParentAccount)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (account == null)
        {
            throw new InvalidOperationException($"Account with ID '{id}' not found");
        }

        var beforeState = new
        {
            account.Name,
            Type = account.Type.ToString(),
            account.Category,
            account.ParentAccountId,
            account.IsActive
        };

        // Validate parent account if specified
        if (request.ParentAccountId.HasValue)
        {
            var parentAccount = await _dbContext.ChartOfAccounts
                .FindAsync(request.ParentAccountId.Value);

            if (parentAccount == null)
            {
                throw new InvalidOperationException($"Parent account with ID '{request.ParentAccountId}' not found");
            }

            if (parentAccount.Type != account.Type)
            {
                throw new InvalidOperationException(
                    $"Parent account type ({parentAccount.Type}) must match child account type ({account.Type})");
            }

            // Prevent circular references
            if (await WouldCreateCircularReferenceAsync(id, request.ParentAccountId.Value))
            {
                throw new InvalidOperationException("Cannot set parent account: would create circular reference");
            }
        }

        account.ApplyUpdate(request);
        await _dbContext.SaveChangesAsync();

        await _auditService.RecordAuditAsync(
            entityType: nameof(ChartOfAccount),
            entityId: account.Id.ToString(),
            action: "Updated",
            beforeState: beforeState,
            afterState: new
            {
                account.Name,
                Type = account.Type.ToString(),
                account.Category,
                account.ParentAccountId,
                account.IsActive
            },
            userId: null,
            correlationId: null,
            ipAddress: null);

        _logger.LogInformation(
            "Updated chart of account: {AccountNumber} - {Name}",
            account.AccountNumber,
            account.Name);

        return account.ToResponse();
    }

    /// <summary>
    /// Deactivates an account.
    /// </summary>
    /// <param name="id">The account ID.</param>
    /// <returns>True if the account was deactivated; otherwise, false.</returns>
    public async Task<bool> DeactivateAccountAsync(Guid id)
    {
        var (isValid, errorMessage) = await ValidateDeactivationAsync(id);

        if (!isValid)
        {
            throw new InvalidOperationException(errorMessage ?? "Cannot deactivate account");
        }

        var account = await _dbContext.ChartOfAccounts.FindAsync(id);

        if (account == null)
        {
            return false;
        }

        var beforeState = new { account.IsActive };

        account.IsActive = false;
        account.ModifiedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        await _auditService.RecordAuditAsync(
            entityType: nameof(ChartOfAccount),
            entityId: account.Id.ToString(),
            action: "Deactivated",
            beforeState: beforeState,
            afterState: new { account.IsActive },
            userId: null,
            correlationId: null,
            ipAddress: null);

        _logger.LogInformation(
            "Deactivated chart of account: {AccountNumber} - {Name}",
            account.AccountNumber,
            account.Name);

        return true;
    }

    /// <summary>
    /// Validates if an account can be deactivated.
    /// </summary>
    /// <param name="id">The account ID.</param>
    /// <returns>A tuple indicating if validation passed and an optional error message.</returns>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateDeactivationAsync(Guid id)
    {
        var account = await _dbContext.ChartOfAccounts.FindAsync(id);

        if (account == null)
        {
            return (false, "Account not found");
        }

        // Check if account has transactions in open periods
        var hasOpenPeriodTransactions = await _dbContext.JournalEntryLines
            .Include(l => l.JournalEntry)
            .ThenInclude(e => e.Period)
            .AnyAsync(l => l.AccountId == id &&
                          l.JournalEntry.Period.Status == PeriodStatus.Open);

        if (hasOpenPeriodTransactions)
        {
            return (false, "Cannot deactivate account with transactions in open periods");
        }

        // Check if account has active child accounts
        var hasActiveChildren = await _dbContext.ChartOfAccounts
            .AnyAsync(a => a.ParentAccountId == id && a.IsActive);

        if (hasActiveChildren)
        {
            return (false, "Cannot deactivate account with active child accounts");
        }

        return (true, null);
    }

    private async Task<bool> WouldCreateCircularReferenceAsync(Guid accountId, Guid newParentId)
    {
        var currentId = newParentId;

        while (currentId != Guid.Empty)
        {
            if (currentId == accountId)
            {
                return true; // Circular reference detected
            }

            var parent = await _dbContext.ChartOfAccounts
                .Where(a => a.Id == currentId)
                .Select(a => a.ParentAccountId)
                .FirstOrDefaultAsync();

            if (!parent.HasValue)
            {
                break;
            }

            currentId = parent.Value;
        }

        return false;
    }

}
