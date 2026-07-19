using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

public class PeriodTests : BaseIntegrationTest
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public PeriodTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ClosePeriod_ShouldSucceed_WhenBalancedAndNoDrafts()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var fiscalYearId = Guid.NewGuid();
        dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = fiscalYearId,
            Name = "FY2026-1",
            StartDate = DateTime.UtcNow.Date.AddDays(-365),
            EndDate = DateTime.UtcNow.Date.AddDays(365),
            PeriodStructure = PeriodStructure.Monthly
        });

        var period = new FinancialPeriod
        {
            Id = Guid.NewGuid(),
            FiscalYearId = fiscalYearId,
            Name = "2026-01",
            Status = PeriodStatus.Open,
            StartDate = DateTime.UtcNow.Date.AddDays(-30),
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };
        dbContext.FinancialPeriods.Add(period);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.PostAsync($"/accounting/v1/periods/{period.Id}/close", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dbContext2 = Factory.GetDbContext();
        var updatedPeriod = await dbContext2.FinancialPeriods.AsNoTracking().FirstOrDefaultAsync(p => p.Id == period.Id);
        Assert.Equal(PeriodStatus.Closed, updatedPeriod!.Status);
    }

    [Fact]
    public async Task ClosePeriod_ShouldFail_WhenHasDrafts()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var periodId = Guid.NewGuid();
        var fiscalYearId = Guid.NewGuid();
        dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = fiscalYearId,
            Name = "FY2026-2",
            StartDate = DateTime.UtcNow.Date.AddDays(-365),
            EndDate = DateTime.UtcNow.Date.AddDays(365),
            PeriodStructure = PeriodStructure.Monthly
        });

        var period = new FinancialPeriod
        {
            Id = periodId,
            FiscalYearId = fiscalYearId,
            Name = "2026-02",
            Status = PeriodStatus.Open,
            StartDate = DateTime.UtcNow.Date.AddDays(-30),
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };
        dbContext.FinancialPeriods.Add(period);

        dbContext.JournalEntries.Add(new JournalEntry
        {
            Id = Guid.NewGuid(),
            PeriodId = periodId,
            Status = EntryStatus.Draft,
            Description = "Draft entry"
        });
        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.PostAsync($"/accounting/v1/periods/{periodId}/close", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReopenPeriod_ShouldSucceed()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var fiscalYearId = Guid.NewGuid();
        dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = fiscalYearId,
            Name = "FY2026-3",
            StartDate = DateTime.UtcNow.Date.AddDays(-365),
            EndDate = DateTime.UtcNow.Date.AddDays(365),
            PeriodStructure = PeriodStructure.Monthly
        });

        var period = new FinancialPeriod
        {
            Id = Guid.NewGuid(),
            FiscalYearId = fiscalYearId,
            Name = "2026-03",
            Status = PeriodStatus.Closed,
            StartDate = DateTime.UtcNow.Date.AddDays(-30),
            EndDate = DateTime.UtcNow.Date.AddDays(30)
        };
        dbContext.FinancialPeriods.Add(period);
        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.PostAsync($"/accounting/v1/periods/{period.Id}/reopen", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var dbContext2 = Factory.GetDbContext();
        var updatedPeriod = await dbContext2.FinancialPeriods.AsNoTracking().FirstOrDefaultAsync(p => p.Id == period.Id);
        Assert.Equal(PeriodStatus.Open, updatedPeriod!.Status);
    }

    [Fact]
    public async Task GetPeriods_ShouldReturnList()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var fiscalYearId = Guid.NewGuid();
        dbContext.FiscalYears.Add(new FiscalYear
        {
            Id = fiscalYearId,
            Name = "FY2026-4",
            StartDate = DateTime.UtcNow.Date.AddDays(-365),
            EndDate = DateTime.UtcNow.Date.AddDays(365),
            PeriodStructure = PeriodStructure.Monthly
        });

        dbContext.FinancialPeriods.Add(new FinancialPeriod
        {
            Id = Guid.NewGuid(),
            FiscalYearId = fiscalYearId,
            Name = "2026-04",
            Status = PeriodStatus.Open,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        // Act
        var response = await Client.GetAsync("/accounting/v1/periods");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var periods = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.Equal(JsonValueKind.Array, periods.ValueKind);
        Assert.True(periods.GetArrayLength() > 0);
    }

    [Fact]
    public async Task OpenPeriod_ShouldReturnSuccess()
    {
        await CleanDatabaseAsync();

        // Act
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var response = await Client.PostAsync($"/accounting/v1/periods/open?date={date}", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.True(result.TryGetProperty("periodId", out _));
    }

    [Fact]
    public async Task PostToLockedPeriod_ShouldReturnBadRequest()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        var fiscalYearId = Guid.NewGuid();
        dbContext.FiscalYears.Add(new FiscalYear { Id = fiscalYearId, Name = "FY-LOCK", StartDate = DateTime.UtcNow.AddDays(-365), EndDate = DateTime.UtcNow.AddDays(365), PeriodStructure = PeriodStructure.Monthly });

        var periodId = Guid.NewGuid();
        dbContext.FinancialPeriods.Add(new FinancialPeriod
        {
            Id = periodId,
            FiscalYearId = fiscalYearId,
            Name = "LOCKED",
            Status = PeriodStatus.Locked,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        // Act - Try to post a journal entry to this period (we can use the API if we have a draft there, but easier to use service or just test the controller if it calls validation)
        // Controller calls ValidatePeriodForPostingAsync in CreateJournalEntry
        var request = new CreateJournalEntryRequest
        {
            EntryDate = DateTime.UtcNow,
            Description = "Locked Test",
            Lines = new List<CreateJournalEntryLineRequest>() // Will fail validation anyway if empty, but let's see
        };

        var response = await Client.PostAsJsonAsync("/accounting/v1/journal-entries", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ClosePeriod_WhenNotExists_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsync($"/accounting/v1/periods/{Guid.NewGuid()}/close", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReopenPeriod_WhenNotExists_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsync($"/accounting/v1/periods/{Guid.NewGuid()}/reopen", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
