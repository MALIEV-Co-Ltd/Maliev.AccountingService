using System.Net;
using System.Net.Http.Json;
using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Data.Models;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

public class ReconciliationTests : BaseIntegrationTest
{
    public ReconciliationTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task RunReconciliation_Balanced_ShouldReturnSuccess()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var periodId = Guid.NewGuid();
        var fiscalYearId = Guid.NewGuid();
        var sourceSystem = "Sales";

        // Create a fiscal year
        dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = fiscalYearId,
            Name = "FY2026",
            StartDate = DateTime.UtcNow.Date.AddDays(-365),
            EndDate = DateTime.UtcNow.Date.AddDays(365),
            PeriodStructure = PeriodStructure.Monthly
        });

        // Create a period
        dbContext.FinancialPeriods.Add(new FinancialPeriod
        {
            Id = periodId,
            FiscalYearId = fiscalYearId,
            Name = "2026-01",
            StartDate = DateTime.UtcNow.Date.AddDays(-30),
            EndDate = DateTime.UtcNow.Date.AddDays(30),
            Status = PeriodStatus.Open
        });

        // Create accounts
        var assetAccount = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "1000", Name = "Cash", Type = AccountType.Asset, IsActive = true };
        var liabilityAccount = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "2000", Name = "Payables", Type = AccountType.Liability, IsActive = true };
        dbContext.ChartOfAccounts.AddRange(assetAccount, liabilityAccount);

        // Create a journal entry
        var journalEntryId = Guid.NewGuid();
        var entry = new JournalEntry
        {
            Id = journalEntryId,
            PeriodId = periodId,
            EntryNumber = "JE-REC-001",
            EntryDate = DateTime.UtcNow.Date,
            Description = "Sales reconciliation test",
            SourceSystem = sourceSystem,
            Status = EntryStatus.Posted,
            TotalDebit = 1000m,
            TotalCredit = 1000m
        };
        dbContext.JournalEntries.Add(entry);

        // Add lines (Total GL = 1000)
        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = journalEntryId, AccountId = assetAccount.Id, DebitAmount = 1000m, CreditAmount = 0m, LineSequence = 1 });
        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = journalEntryId, AccountId = liabilityAccount.Id, DebitAmount = 0m, CreditAmount = 1000m, LineSequence = 2 });

        // Add subledger transaction (Total Subledger = 1000)
        dbContext.SubledgerTransactions.Add(new SubledgerTransaction
        {
            Id = Guid.NewGuid(),
            JournalEntryId = journalEntryId,
            SourceSystem = sourceSystem,
            Amount = 1000m,
            TransactionDate = DateTime.UtcNow,
            Description = "Sale 1"
        });

        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/accounting/v1/reconciliation/run?sourceSystem={sourceSystem}&periodId={periodId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReconciliationResult>();
        Assert.NotNull(result);
        Assert.True(result.IsBalanced);
        Assert.Equal(1000m, result.SubledgerTotal);
        Assert.Equal(1000m, result.GeneralLedgerTotal);
    }

    [Fact]
    public async Task RunReconciliation_Unbalanced_ShouldReportVariance()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var periodId = Guid.NewGuid();
        var fiscalYearId = Guid.NewGuid();
        var sourceSystem = "Procurement";

        // Create a fiscal year
        dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = fiscalYearId,
            Name = "FY2026-P",
            StartDate = DateTime.UtcNow.Date.AddDays(-365),
            EndDate = DateTime.UtcNow.Date.AddDays(365),
            PeriodStructure = PeriodStructure.Monthly
        });

        // Create a period
        dbContext.FinancialPeriods.Add(new FinancialPeriod
        {
            Id = periodId,
            FiscalYearId = fiscalYearId,
            Name = "2026-02",
            StartDate = DateTime.UtcNow.Date.AddDays(-30),
            EndDate = DateTime.UtcNow.Date.AddDays(30),
            Status = PeriodStatus.Open
        });

        // Create accounts
        var assetAccount = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "1000-P", Name = "Cash", Type = AccountType.Asset, IsActive = true };
        var liabilityAccount = new ChartOfAccount { Id = Guid.NewGuid(), AccountNumber = "2000-P", Name = "Payables", Type = AccountType.Liability, IsActive = true };
        dbContext.ChartOfAccounts.AddRange(assetAccount, liabilityAccount);

        // Create a journal entry
        var journalEntryId = Guid.NewGuid();
        var entry = new JournalEntry
        {
            Id = journalEntryId,
            PeriodId = periodId,
            EntryNumber = "JE-REC-002",
            EntryDate = DateTime.UtcNow.Date,
            Description = "Procurement reconciliation test",
            SourceSystem = sourceSystem,
            Status = EntryStatus.Posted,
            TotalDebit = 1000m,
            TotalCredit = 1000m
        };
        dbContext.JournalEntries.Add(entry);

        // Add lines (Total GL = 1000)
        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = journalEntryId, AccountId = assetAccount.Id, DebitAmount = 1000m, CreditAmount = 0m, LineSequence = 1 });
        dbContext.JournalEntryLines.Add(new JournalEntryLine { Id = Guid.NewGuid(), JournalEntryId = journalEntryId, AccountId = liabilityAccount.Id, DebitAmount = 0m, CreditAmount = 1000m, LineSequence = 2 });

        // Add subledger transaction (Total Subledger = 900)
        dbContext.SubledgerTransactions.Add(new SubledgerTransaction
        {
            Id = Guid.NewGuid(),
            JournalEntryId = journalEntryId,
            SourceSystem = sourceSystem,
            Amount = 900m, // Variance!
            TransactionDate = DateTime.UtcNow,
            Description = "Purchase 1"
        });

        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync($"/accounting/v1/reconciliation/run?sourceSystem={sourceSystem}&periodId={periodId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ReconciliationResult>();
        Assert.NotNull(result);
        Assert.False(result.IsBalanced);
        Assert.Equal(-100m, result.Variance);
        Assert.NotEmpty(result.Discrepancies);
    }
}
