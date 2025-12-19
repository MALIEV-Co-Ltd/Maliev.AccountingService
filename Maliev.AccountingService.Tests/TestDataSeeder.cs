using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;

namespace Maliev.AccountingService.Tests;

/// <summary>
/// Seeds test data for integration tests
/// </summary>
public static class TestDataSeeder
{
    /// <summary>
    /// Seeds the chart of accounts required for event processing
    /// </summary>
    public static async Task SeedChartOfAccountsAsync(AccountingDbContext context)
    {
        // Check if already seeded
        if (context.ChartOfAccounts.Any())
        {
            return;
        }

        var accounts = new List<ChartOfAccount>
        {
            // Assets
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1100",
                Name = "Cash/Bank",
                Type = AccountType.Asset,
                Category = "Current Assets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1200",
                Name = "Accounts Receivable",
                Type = AccountType.Asset,
                Category = "Current Assets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1300",
                Name = "VAT Input Tax Recoverable",
                Type = AccountType.Asset,
                Category = "Current Assets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1400",
                Name = "Inventory",
                Type = AccountType.Asset,
                Category = "Current Assets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // Liabilities
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "2100",
                Name = "Accounts Payable",
                Type = AccountType.Liability,
                Category = "Current Liabilities",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "2200",
                Name = "Tax Payable",
                Type = AccountType.Liability,
                Category = "Current Liabilities",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "2210",
                Name = "Insurance Payable",
                Type = AccountType.Liability,
                Category = "Current Liabilities",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "2220",
                Name = "Pension Payable",
                Type = AccountType.Liability,
                Category = "Current Liabilities",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "2300",
                Name = "VAT Output Tax Payable",
                Type = AccountType.Liability,
                Category = "Current Liabilities",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // Revenue
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "4000",
                Name = "Sales Revenue",
                Type = AccountType.Revenue,
                Category = "Operating Revenue",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },

            // Expenses
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "5000",
                Name = "Operating Expenses",
                Type = AccountType.Expense,
                Category = "Operating Expenses",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = Guid.NewGuid(),
                AccountNumber = "5100",
                Name = "Payroll Expense",
                Type = AccountType.Expense,
                Category = "Operating Expenses",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        context.ChartOfAccounts.AddRange(accounts);
        await context.SaveChangesAsync();
    }
}
