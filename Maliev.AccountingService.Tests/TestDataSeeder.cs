using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Data.Models;

namespace Maliev.AccountingService.Tests;

/// <summary>
/// Seeds test data for integration tests
/// </summary>
public static class TestDataSeeder
{
    // Fixed GUIDs to prevent cache consistency issues across tests
    public static readonly Guid CashAccountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ArAccountId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid VatInputAccountId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid InventoryAccountId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid ApAccountId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid TaxPayableAccountId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid InsurancePayableAccountId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid PensionPayableAccountId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid VatOutputAccountId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    public static readonly Guid SalesRevenueAccountId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid OperatingExpensesAccountId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid PayrollExpenseAccountId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

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
                Id = CashAccountId,
                AccountNumber = "1100",
                Name = "Cash/Bank",
                Type = AccountType.Asset,
                Category = "Current Assets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = ArAccountId,
                AccountNumber = "1200",
                Name = "Accounts Receivable",
                Type = AccountType.Asset,
                Category = "Current Assets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = VatInputAccountId,
                AccountNumber = "1300",
                Name = "VAT Input Tax Recoverable",
                Type = AccountType.Asset,
                Category = "Current Assets",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = InventoryAccountId,
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
                Id = ApAccountId,
                AccountNumber = "2100",
                Name = "Accounts Payable",
                Type = AccountType.Liability,
                Category = "Current Liabilities",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = TaxPayableAccountId,
                AccountNumber = "2200",
                Name = "Tax Payable",
                Type = AccountType.Liability,
                Category = "Current Liabilities",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = InsurancePayableAccountId,
                AccountNumber = "2210",
                Name = "Insurance Payable",
                Type = AccountType.Liability,
                Category = "Current Liabilities",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = PensionPayableAccountId,
                AccountNumber = "2220",
                Name = "Pension Payable",
                Type = AccountType.Liability,
                Category = "Current Liabilities",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = VatOutputAccountId,
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
                Id = SalesRevenueAccountId,
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
                Id = OperatingExpensesAccountId,
                AccountNumber = "5000",
                Name = "Operating Expenses",
                Type = AccountType.Expense,
                Category = "Operating Expenses",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ChartOfAccount
            {
                Id = PayrollExpenseAccountId,
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
