using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Maliev.AccountingService.Api.Extensions;

/// <summary>
/// Extension methods for seeding initial data into the accounting database
/// </summary>
public static class SeedDataExtensions
{
    /// <summary>
    /// Seeds a standard chart of accounts structure with common account types
    /// Usage: Call from Program.cs when --seed-data argument is present
    /// </summary>
    public static async Task SeedStandardChartOfAccountsAsync(this AccountingDbContext context, ILogger logger)
    {
        // Check if chart of accounts already exists
        var existingCount = await context.ChartOfAccounts.CountAsync();
        if (existingCount > 0)
        {
            logger.LogInformation("Chart of accounts already seeded ({Count} accounts exist). Skipping seed.", existingCount);
            return;
        }

        logger.LogInformation("Seeding standard chart of accounts...");

        var accounts = GetStandardChartOfAccounts();

        await context.ChartOfAccounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();

        logger.LogInformation("Successfully seeded {Count} standard accounts", accounts.Count);
    }

    private static List<ChartOfAccount> GetStandardChartOfAccounts()
    {
        var accounts = new List<ChartOfAccount>();
        var now = DateTime.UtcNow;

        // ========== ASSETS (1000-1999) ==========

        // Current Assets (1000-1499)
        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1000",
            Name = "Current Assets",
            Description = "Parent account for all current assets",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1100",
            Name = "Cash and Cash Equivalents",
            Description = "Bank accounts, petty cash, and cash on hand",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1200",
            Name = "Accounts Receivable",
            Description = "Amounts owed by customers for goods/services sold on credit",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1210",
            Name = "Allowance for Doubtful Accounts",
            Description = "Estimated uncollectible receivables (contra-asset)",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1300",
            Name = "Inventory",
            Description = "Goods held for sale or raw materials for production",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1400",
            Name = "Prepaid Expenses",
            Description = "Expenses paid in advance (insurance, rent, etc.)",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            CreatedAt = now
        });

        // Fixed Assets (1500-1999)
        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1500",
            Name = "Fixed Assets",
            Description = "Parent account for all fixed assets",
            Type = AccountType.Asset,
            Category = "Fixed Assets",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1510",
            Name = "Property, Plant and Equipment",
            Description = "Land, buildings, machinery, and equipment",
            Type = AccountType.Asset,
            Category = "Fixed Assets",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "1520",
            Name = "Accumulated Depreciation",
            Description = "Cumulative depreciation of fixed assets (contra-asset)",
            Type = AccountType.Asset,
            Category = "Fixed Assets",
            IsActive = true,
            CreatedAt = now
        });

        // ========== LIABILITIES (2000-2999) ==========

        // Current Liabilities (2000-2499)
        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "2000",
            Name = "Current Liabilities",
            Description = "Parent account for all current liabilities",
            Type = AccountType.Liability,
            Category = "Current Liabilities",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "2100",
            Name = "Accounts Payable",
            Description = "Amounts owed to suppliers for goods/services purchased on credit",
            Type = AccountType.Liability,
            Category = "Current Liabilities",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "2200",
            Name = "Accrued Expenses",
            Description = "Expenses incurred but not yet paid",
            Type = AccountType.Liability,
            Category = "Current Liabilities",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "2300",
            Name = "Payroll Liabilities",
            Description = "Wages, salaries, and benefits payable to employees",
            Type = AccountType.Liability,
            Category = "Current Liabilities",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "2400",
            Name = "Tax Liabilities",
            Description = "Taxes payable (VAT, income tax, etc.)",
            Type = AccountType.Liability,
            Category = "Current Liabilities",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "2410",
            Name = "VAT Payable",
            Description = "Output VAT collected from customers",
            Type = AccountType.Liability,
            Category = "Current Liabilities",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "2420",
            Name = "VAT Receivable",
            Description = "Input VAT paid to suppliers",
            Type = AccountType.Asset,
            Category = "Current Assets",
            IsActive = true,
            CreatedAt = now
        });

        // Long-term Liabilities (2500-2999)
        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "2500",
            Name = "Long-term Liabilities",
            Description = "Parent account for long-term debt and obligations",
            Type = AccountType.Liability,
            Category = "Long-term Liabilities",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "2510",
            Name = "Long-term Debt",
            Description = "Loans and debt obligations due after one year",
            Type = AccountType.Liability,
            Category = "Long-term Liabilities",
            IsActive = true,
            CreatedAt = now
        });

        // ========== EQUITY (3000-3999) ==========

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "3000",
            Name = "Equity",
            Description = "Parent account for owner's equity",
            Type = AccountType.Equity,
            Category = "Equity",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "3100",
            Name = "Share Capital",
            Description = "Capital invested by shareholders",
            Type = AccountType.Equity,
            Category = "Equity",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "3200",
            Name = "Retained Earnings",
            Description = "Cumulative net income retained in the business",
            Type = AccountType.Equity,
            Category = "Equity",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "3300",
            Name = "Current Year Earnings",
            Description = "Net profit or loss for the current fiscal year",
            Type = AccountType.Equity,
            Category = "Equity",
            IsActive = true,
            CreatedAt = now
        });

        // ========== REVENUE (4000-4999) ==========

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "4000",
            Name = "Revenue",
            Description = "Parent account for all revenue",
            Type = AccountType.Revenue,
            Category = "Operating Revenue",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "4100",
            Name = "Sales Revenue",
            Description = "Revenue from sale of goods and services",
            Type = AccountType.Revenue,
            Category = "Operating Revenue",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "4200",
            Name = "Service Revenue",
            Description = "Revenue from services provided",
            Type = AccountType.Revenue,
            Category = "Operating Revenue",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "4900",
            Name = "Other Income",
            Description = "Non-operating income (interest, dividends, etc.)",
            Type = AccountType.Revenue,
            Category = "Other Revenue",
            IsActive = true,
            CreatedAt = now
        });

        // ========== EXPENSES (5000-9999) ==========

        // Cost of Goods Sold (5000-5999)
        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "5000",
            Name = "Cost of Goods Sold",
            Description = "Direct costs of producing goods sold",
            Type = AccountType.Expense,
            Category = "Cost of Sales",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "5100",
            Name = "Material Costs",
            Description = "Cost of raw materials used in production",
            Type = AccountType.Expense,
            Category = "Cost of Sales",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "5200",
            Name = "Direct Labor",
            Description = "Wages for production workers",
            Type = AccountType.Expense,
            Category = "Cost of Sales",
            IsActive = true,
            CreatedAt = now
        });

        // Operating Expenses (6000-8999)
        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6000",
            Name = "Operating Expenses",
            Description = "Parent account for operating expenses",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6100",
            Name = "Salaries and Wages",
            Description = "Employee compensation expense",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6200",
            Name = "Rent Expense",
            Description = "Office and facility rental costs",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6300",
            Name = "Utilities Expense",
            Description = "Electricity, water, internet, phone",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6400",
            Name = "Depreciation Expense",
            Description = "Allocation of fixed asset costs over useful life",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6500",
            Name = "Marketing and Advertising",
            Description = "Promotional and marketing expenses",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6600",
            Name = "Professional Fees",
            Description = "Legal, accounting, consulting fees",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6700",
            Name = "Insurance Expense",
            Description = "Business insurance premiums",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6800",
            Name = "Office Supplies",
            Description = "Stationery, printing, and office materials",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "6900",
            Name = "Travel and Entertainment",
            Description = "Business travel, meals, and entertainment",
            Type = AccountType.Expense,
            Category = "Operating Expenses",
            IsActive = true,
            CreatedAt = now
        });

        // Other Expenses (9000-9999)
        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "9000",
            Name = "Other Expenses",
            Description = "Non-operating and miscellaneous expenses",
            Type = AccountType.Expense,
            Category = "Other Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "9100",
            Name = "Interest Expense",
            Description = "Interest paid on loans and debt",
            Type = AccountType.Expense,
            Category = "Other Expenses",
            IsActive = true,
            CreatedAt = now
        });

        accounts.Add(new ChartOfAccount
        {
            Id = Guid.NewGuid(),
            AccountNumber = "9200",
            Name = "Bank Fees",
            Description = "Banking transaction and service fees",
            Type = AccountType.Expense,
            Category = "Other Expenses",
            IsActive = true,
            CreatedAt = now
        });

        return accounts;
    }
}
