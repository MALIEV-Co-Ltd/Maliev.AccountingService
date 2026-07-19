using Maliev.AccountingService.Api.DTOs.Responses;
using Maliev.AccountingService.Infrastructure.Data;
using Maliev.AccountingService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Implementation of IReportingService for generating financial reports
/// </summary>
public class ReportingService : IReportingService
{
    private readonly AccountingDbContext _context;
    private readonly ILogger<ReportingService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportingService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    public ReportingService(AccountingDbContext context, ILogger<ReportingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TrialBalanceResponse> GetTrialBalanceAsync(Guid? periodId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Trial Balance report");

        var query = _context.JournalEntryLines
            .Where(l => l.JournalEntry.Status == EntryStatus.Posted);

        if (periodId.HasValue)
        {
            query = query.Where(l => l.JournalEntry.PeriodId == periodId.Value);
        }
        else
        {
            if (startDate.HasValue)
                query = query.Where(l => l.JournalEntry.EntryDate >= DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc));
            if (endDate.HasValue)
                query = query.Where(l => l.JournalEntry.EntryDate < DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc));
        }

        var accountBalances = await query
            .GroupBy(l => new { l.Account.AccountNumber, l.Account.Name })
            .Select(g => new TrialBalanceLine
            {
                AccountNumber = g.Key.AccountNumber,
                AccountName = g.Key.Name,
                DebitBalance = g.Sum(l => l.DebitAmount - l.CreditAmount) > 0 ? g.Sum(l => l.DebitAmount - l.CreditAmount) : 0,
                CreditBalance = g.Sum(l => l.CreditAmount - l.DebitAmount) > 0 ? g.Sum(l => l.CreditAmount - l.DebitAmount) : 0
            })
            .OrderBy(l => l.AccountNumber)
            .ToListAsync(cancellationToken);

        return new TrialBalanceResponse
        {
            PeriodName = periodId?.ToString() ?? "Custom Range",
            GeneratedAt = DateTime.UtcNow,
            Items = accountBalances,
            TotalDebit = accountBalances.Sum(l => l.DebitBalance),
            TotalCredit = accountBalances.Sum(l => l.CreditBalance)
        };
    }

    /// <inheritdoc />
    public async Task<BalanceSheetResponse> GetBalanceSheetAsync(DateTime asOfDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Balance Sheet as of {AsOfDate}", asOfDate);

        var utcAsOf = asOfDate.ToUniversalTime();

        var balances = await _context.JournalEntryLines
            .Where(l => l.JournalEntry.Status == EntryStatus.Posted && l.JournalEntry.EntryDate <= utcAsOf)
            .GroupBy(l => new { l.Account.AccountNumber, l.Account.Name, l.Account.Type, l.Account.Category })
            .Select(g => new
            {
                g.Key.AccountNumber,
                g.Key.Name,
                g.Key.Type,
                g.Key.Category,
                Balance = g.Sum(l => l.DebitAmount - l.CreditAmount)
            })
            .ToListAsync(cancellationToken);

        var response = new BalanceSheetResponse
        {
            AsOfDate = asOfDate,
            GeneratedAt = DateTime.UtcNow
        };

        // Group by Type and Category
        var grouped = balances.GroupBy(b => b.Type);

        foreach (var typeGroup in grouped)
        {
            var sections = typeGroup.GroupBy(b => b.Category ?? "Other")
                .Select(cg => new BalanceSheetSection
                {
                    Category = cg.Key,
                    Items = cg.Select(i => new BalanceSheetItem
                    {
                        AccountNumber = i.AccountNumber,
                        AccountName = i.Name,
                        Balance = i.Type == AccountType.Asset ? i.Balance : -i.Balance
                    }).ToList(),
                    Subtotal = cg.Sum(i => i.Type == AccountType.Asset ? i.Balance : -i.Balance)
                }).ToList();

            var total = sections.Sum(s => s.Subtotal);

            switch (typeGroup.Key)
            {
                case AccountType.Asset:
                    response.Assets = sections;
                    response.TotalAssets = total;
                    break;
                case AccountType.Liability:
                    response.Liabilities = sections;
                    response.TotalLiabilities = total;
                    break;
                case AccountType.Equity:
                    response.Equity = sections;
                    response.TotalEquity = total;
                    break;
            }
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<IncomeStatementResponse> GetIncomeStatementAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Income Statement from {StartDate} to {EndDate}", startDate, endDate);

        var utcStart = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var utcEnd = DateTime.SpecifyKind(endDate.Date.AddDays(1), DateTimeKind.Utc);

        var movements = await _context.JournalEntryLines
            .Where(l => l.JournalEntry.Status == EntryStatus.Posted
                && l.JournalEntry.EntryDate >= utcStart
                && l.JournalEntry.EntryDate < utcEnd)
            .Where(l => l.Account.Type == AccountType.Revenue || l.Account.Type == AccountType.Expense)
            .GroupBy(l => new { l.Account.AccountNumber, l.Account.Name, l.Account.Type, l.Account.Category })
            .Select(g => new
            {
                g.Key.AccountNumber,
                g.Key.Name,
                g.Key.Type,
                g.Key.Category,
                Amount = g.Sum(l => l.CreditAmount - l.DebitAmount) // Revenue is Credit - Debit
            })
            .ToListAsync(cancellationToken);

        var response = new IncomeStatementResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAt = DateTime.UtcNow
        };

        var revenueItems = movements.Where(m => m.Type == AccountType.Revenue);
        var expenseItems = movements.Where(m => m.Type == AccountType.Expense);

        response.Revenues = revenueItems.GroupBy(i => i.Category ?? "Revenue")
            .Select(g => new IncomeStatementSection
            {
                Category = g.Key,
                Items = g.Select(x => new IncomeStatementItem { AccountNumber = x.AccountNumber, AccountName = x.Name, Amount = x.Amount }).ToList(),
                Subtotal = g.Sum(x => x.Amount)
            }).ToList();

        response.Expenses = expenseItems.GroupBy(i => i.Category ?? "Expense")
            .Select(g => new IncomeStatementSection
            {
                Category = g.Key,
                Items = g.Select(x => new IncomeStatementItem { AccountNumber = x.AccountNumber, AccountName = x.Name, Amount = -x.Amount }).ToList(),
                Subtotal = g.Sum(x => -x.Amount)
            }).ToList();

        response.TotalRevenue = response.Revenues.Sum(r => r.Subtotal);
        response.TotalExpense = response.Expenses.Sum(e => e.Subtotal);

        return response;
    }
}

// Add GeneratedAt to responses if needed, or just use the DTOs created earlier.
// I will update the DTOs to include GeneratedAt.
