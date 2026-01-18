using System.Net;
using System.Net.Http.Json;
using Maliev.AccountingService.Api.DTOs.Responses;
using Maliev.AccountingService.Data.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

public class FinancialReportsTests : BaseIntegrationTest
{
    public FinancialReportsTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetBalanceSheet_ReturnsOk()
    {
        // Arrange
        await CleanDatabaseAsync();
        var dbContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(dbContext);

        // Act
        var response = await Client.GetAsync("/accounting/v1/reports/balance-sheet");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BalanceSheetResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetIncomeStatement_ReturnsOk()
    {
        // Arrange
        await CleanDatabaseAsync();
        var dbContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(dbContext);
        var startDate = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await Client.GetAsync($"/accounting/v1/reports/income-statement?startDate={startDate}&endDate={endDate}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IncomeStatementResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTrialBalance_ReturnsOk()
    {
        // Arrange
        await CleanDatabaseAsync();
        var dbContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(dbContext);

        // Act
        var response = await Client.GetAsync("/accounting/v1/reports/trial-balance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TrialBalanceResponse>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetBalanceSheet_WithData_ShouldCalculateTotalsCorrectly()
    {
        // Arrange
        await CleanDatabaseAsync();
        var dbContext = Factory.GetDbContext();

        var asset = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "1000", Name = "Cash", Type = AccountType.Asset, Category = "Current Assets", IsActive = true };
        var liability = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "2000", Name = "Loans", Type = AccountType.Liability, Category = "Long-term Liabilities", IsActive = true };
        var equity = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "3000", Name = "Capital", Type = AccountType.Equity, Category = "Equity", IsActive = true };
        dbContext.ChartOfAccounts.AddRange(asset, liability, equity);

        var fiscalYearId = Guid.NewGuid();
        dbContext.FiscalYears.Add(new FiscalYear { Id = fiscalYearId, Name = "FY2026-A", StartDate = DateTime.UtcNow.AddDays(-365), EndDate = DateTime.UtcNow.AddDays(365), PeriodStructure = PeriodStructure.Monthly });

        var period = new FinancialPeriod { Id = Guid.NewGuid(), FiscalYearId = fiscalYearId, Name = "2026-01", StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(10) };
        dbContext.FinancialPeriods.Add(period);

        var entry = new JournalEntry { Id = Guid.NewGuid(), PeriodId = period.Id, EntryDate = DateTime.UtcNow, Status = EntryStatus.Posted, Description = "Test Entry", TotalDebit = 1000m, TotalCredit = 1000m, EntryNumber = "JE-001" };
        dbContext.JournalEntries.Add(entry);

        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = entry.Id, AccountId = asset.Id, DebitAmount = 1000m, CreditAmount = 0m, LineSequence = 1 });
        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = entry.Id, AccountId = liability.Id, DebitAmount = 0m, CreditAmount = 600m, LineSequence = 2 });
        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = entry.Id, AccountId = equity.Id, DebitAmount = 0m, CreditAmount = 400m, LineSequence = 3 });

        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/accounting/v1/reports/balance-sheet");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BalanceSheetResponse>();
        Assert.NotNull(result);
        Assert.Equal(1000m, result.TotalAssets);
        Assert.Equal(600m, result.TotalLiabilities);
        Assert.Equal(400m, result.TotalEquity);
    }

    [Fact]
    public async Task GetIncomeStatement_WithData_ShouldCalculateNetIncome()
    {
        // Arrange
        await CleanDatabaseAsync();
        var dbContext = Factory.GetDbContext();

        var revenue = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "4000", Name = "Sales", Type = AccountType.Revenue, Category = "Operating Revenue", IsActive = true };
        var expense = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "5000", Name = "Rent", Type = AccountType.Expense, Category = "Operating Expenses", IsActive = true };
        dbContext.ChartOfAccounts.AddRange(revenue, expense);

        var fiscalYearId = Guid.NewGuid();
        dbContext.FiscalYears.Add(new FiscalYear { Id = fiscalYearId, Name = "FY2026-B", StartDate = DateTime.UtcNow.AddDays(-365), EndDate = DateTime.UtcNow.AddDays(365), PeriodStructure = PeriodStructure.Monthly });

        var period = new FinancialPeriod { Id = Guid.NewGuid(), FiscalYearId = fiscalYearId, Name = "2026-02", StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(10) };
        dbContext.FinancialPeriods.Add(period);

        var entry = new JournalEntry { Id = Guid.NewGuid(), PeriodId = period.Id, EntryDate = DateTime.UtcNow, Status = EntryStatus.Posted, Description = "Revenue Entry", TotalDebit = 1000m, TotalCredit = 1000m, EntryNumber = "JE-002" };
        dbContext.JournalEntries.Add(entry);

        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = entry.Id, AccountId = revenue.Id, DebitAmount = 0m, CreditAmount = 1000m, LineSequence = 1 });
        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = entry.Id, AccountId = expense.Id, DebitAmount = 300m, CreditAmount = 0m, LineSequence = 2 });

        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/accounting/v1/reports/income-statement?startDate={DateTime.UtcNow.AddDays(-1):yyyy-MM-dd}&endDate={DateTime.UtcNow.AddDays(1):yyyy-MM-dd}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IncomeStatementResponse>();
        Assert.NotNull(result);
        Assert.Equal(1000m, result.TotalRevenue);
        Assert.Equal(300m, result.TotalExpense);
        Assert.Equal(700m, result.NetIncome);
    }

    [Fact]
    public async Task GetTrialBalance_WithPeriodFilter_ShouldReturnCorrectResults()
    {
        // Arrange
        await CleanDatabaseAsync();
        var dbContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(dbContext);
        var account = await dbContext.ChartOfAccounts.FirstAsync();

        var fiscalYearId = Guid.NewGuid();
        dbContext.FiscalYears.Add(new FiscalYear { Id = fiscalYearId, Name = "FY2026-C", StartDate = DateTime.UtcNow.AddDays(-365), EndDate = DateTime.UtcNow.AddDays(365), PeriodStructure = PeriodStructure.Monthly });

        var period = new FinancialPeriod { Id = Guid.NewGuid(), FiscalYearId = fiscalYearId, Name = "2026-03", StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(10) };
        dbContext.FinancialPeriods.Add(period);

        var entry = new JournalEntry { Id = Guid.NewGuid(), PeriodId = period.Id, EntryDate = DateTime.UtcNow, Status = EntryStatus.Posted, Description = "Trial Balance Test", TotalDebit = 100m, TotalCredit = 100m, EntryNumber = "JE-003" };
        dbContext.JournalEntries.Add(entry);
        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = entry.Id, AccountId = account.Id, DebitAmount = 100m, CreditAmount = 0m, LineSequence = 1 });

        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/accounting/v1/reports/trial-balance?periodId={period.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TrialBalanceResponse>();
        Assert.NotNull(result);
        Assert.Contains(result.Items, i => i.AccountNumber == account.AccountNumber && i.DebitBalance == 100m);
    }
}
