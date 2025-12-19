using Maliev.AccountingService.Api.DTOs.Requests;
using Maliev.AccountingService.Api.Extensions;
using Maliev.AccountingService.Data.Models;
using Xunit;

namespace Maliev.AccountingService.Tests.Unit;

/// <summary>
/// Unit tests for JournalEntry extension methods
/// </summary>
public class JournalEntryExtensionsTests
{
    [Fact]
    public void ToResponse_ShouldMapJournalEntryCorrectly()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var journalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = "JE-2025-01-001",
            EntryDate = new DateTime(2025, 1, 15),
            Description = "Test Entry",
            Status = EntryStatus.Draft,
            PeriodId = periodId,
            Period = new FinancialPeriod { Id = periodId, Name = "2025-01" },
            TotalDebit = 1000m,
            TotalCredit = 1000m,
            SourceSystem = "TEST",
            CreatedAt = DateTime.UtcNow,
            Lines = new List<JournalEntryLine>()
        };

        // Act
        var response = journalEntry.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(journalEntry.Id, response.Id);
        Assert.Equal(journalEntry.EntryNumber, response.EntryNumber);
        Assert.Equal(journalEntry.EntryDate, response.EntryDate);
        Assert.Equal(journalEntry.Description, response.Description);
        Assert.Equal("Draft", response.Status);
        Assert.Equal(periodId, response.PeriodId);
        Assert.Equal("2025-01", response.PeriodName);
        Assert.Equal(1000m, response.TotalDebit);
        Assert.Equal(1000m, response.TotalCredit);
        Assert.NotNull(response.Lines);
    }

    [Fact]
    public void ToResponse_ShouldHandleNullPeriod()
    {
        // Arrange
        var journalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = "JE-001",
            EntryDate = DateTime.UtcNow,
            Description = "Test",
            Status = EntryStatus.Posted,
            PeriodId = Guid.NewGuid(),
            Period = null!, // No period loaded
            Lines = null! // No lines loaded
        };

        // Act
        var response = journalEntry.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.PeriodName);
        Assert.NotNull(response.Lines);
        Assert.Empty(response.Lines);
    }

    [Fact]
    public void ToResponse_ShouldMapJournalEntryLineCorrectly()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var line = new JournalEntryLine
        {
            Id = Guid.NewGuid(),
            LineSequence = 1,
            AccountId = accountId,
            Account = new ChartOfAccount { AccountNumber = "1000", Name = "Cash" },
            DebitAmount = 500m,
            CreditAmount = 0m,
            Description = "Payment received",
            CustomerId = customerId,
            ReferenceId = "INV-001",
            TaxComponents = new List<TaxComponent>()
        };

        // Act
        var response = line.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(line.Id, response.Id);
        Assert.Equal(1, response.LineNumber);
        Assert.Equal(accountId, response.AccountId);
        Assert.Equal("1000", response.AccountNumber);
        Assert.Equal("Cash", response.AccountName);
        Assert.Equal(500m, response.DebitAmount);
        Assert.Equal(0m, response.CreditAmount);
        Assert.Equal("Payment received", response.Description);
        Assert.Equal(customerId, response.CustomerId);
        Assert.Equal("INV-001", response.Reference);
    }

    [Fact]
    public void ToResponse_ShouldHandleNullAccount()
    {
        // Arrange
        var line = new JournalEntryLine
        {
            Id = Guid.NewGuid(),
            LineSequence = 2,
            AccountId = Guid.NewGuid(),
            Account = null!, // No account loaded
            DebitAmount = 100m,
            CreditAmount = 0m,
            TaxComponents = null!
        };

        // Act
        var response = line.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(string.Empty, response.AccountNumber);
        Assert.Equal(string.Empty, response.AccountName);
        Assert.Null(response.TaxComponents);
    }

    [Fact]
    public void ToResponse_ShouldMapTaxComponentCorrectly()
    {
        // Arrange
        var taxComponent = new TaxComponent
        {
            Id = Guid.NewGuid(),
            TaxType = "VAT",
            TaxRate = 15m,
            TaxableAmount = 100m,
            TaxAmount = 15m
        };

        // Act
        var response = taxComponent.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(taxComponent.Id, response.Id);
        Assert.Equal("VAT", response.TaxType);
        Assert.Equal(15m, response.TaxRate);
        Assert.Equal(100m, response.TaxableAmount);
        Assert.Equal(15m, response.TaxAmount);
    }

    [Fact]
    public void ToEntity_ShouldMapCreateJournalEntryRequest()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var accountId1 = Guid.NewGuid();
        var accountId2 = Guid.NewGuid();

        var request = new CreateJournalEntryRequest
        {
            EntryDate = new DateTime(2025, 1, 15),
            Description = "Test Entry",
            Reference = "REF-001",
            Lines = new List<CreateJournalEntryLineRequest>
            {
                new()
                {
                    AccountId = accountId1,
                    DebitAmount = 1000m,
                    CreditAmount = 0m,
                    Description = "Debit line"
                },
                new()
                {
                    AccountId = accountId2,
                    DebitAmount = 0m,
                    CreditAmount = 1000m,
                    Description = "Credit line"
                }
            }
        };

        // Act
        var entity = request.ToEntity(periodId, createdBy);

        // Assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(request.EntryDate, entity.EntryDate);
        Assert.Equal(request.Description, entity.Description);
        Assert.Equal(EntryStatus.Draft, entity.Status);
        Assert.Equal(periodId, entity.PeriodId);
        Assert.Equal("REF-001", entity.SourceSystem);
        Assert.Equal(createdBy, entity.CreatedBy);
        Assert.Equal(2, entity.Lines.Count);
        Assert.Equal(1000m, entity.TotalDebit);
        Assert.Equal(1000m, entity.TotalCredit);
    }

    [Fact]
    public void ToEntity_ShouldMapCreateJournalEntryLineRequest()
    {
        // Arrange
        var journalEntryId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();

        var request = new CreateJournalEntryLineRequest
        {
            AccountId = accountId,
            DebitAmount = 500m,
            CreditAmount = 0m,
            Description = "Test line",
            CustomerId = customerId,
            SupplierId = supplierId,
            Reference = "REF-LINE-001",
            TaxComponents = new List<CreateTaxComponentRequest>
            {
                new()
                {
                    TaxType = "VAT",
                    TaxRate = 10m,
                    TaxableAmount = 500m,
                    TaxAmount = 50m
                }
            }
        };

        // Act
        var entity = request.ToEntity(journalEntryId, 1);

        // Assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(journalEntryId, entity.JournalEntryId);
        Assert.Equal(1, entity.LineSequence);
        Assert.Equal(accountId, entity.AccountId);
        Assert.Equal(500m, entity.DebitAmount);
        Assert.Equal(0m, entity.CreditAmount);
        Assert.Equal("Test line", entity.Description);
        Assert.Equal(customerId, entity.CustomerId);
        Assert.Equal(supplierId, entity.SupplierId);
        Assert.Equal("REF-LINE-001", entity.ReferenceId);
        Assert.Single(entity.TaxComponents);
    }

    [Fact]
    public void ToEntity_ShouldHandleNullTaxComponents()
    {
        // Arrange
        var request = new CreateJournalEntryLineRequest
        {
            AccountId = Guid.NewGuid(),
            DebitAmount = 100m,
            CreditAmount = 0m,
            TaxComponents = null!
        };

        // Act
        var entity = request.ToEntity(Guid.NewGuid(), 1);

        // Assert
        Assert.NotNull(entity);
        Assert.NotNull(entity.TaxComponents);
        Assert.Empty(entity.TaxComponents);
    }

    [Fact]
    public void ToEntity_ShouldMapCreateTaxComponentRequest()
    {
        // Arrange
        var lineId = Guid.NewGuid();
        var request = new CreateTaxComponentRequest
        {
            TaxType = "GST",
            TaxRate = 5m,
            TaxableAmount = 200m,
            TaxAmount = 10m
        };

        // Act
        var entity = request.ToEntity(lineId);

        // Assert
        Assert.NotNull(entity);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(lineId, entity.JournalEntryLineId);
        Assert.Equal("GST", entity.TaxType);
        Assert.Equal(5m, entity.TaxRate);
        Assert.Equal(200m, entity.TaxableAmount);
        Assert.Equal(10m, entity.TaxAmount);
    }

    [Fact]
    public void ToResponse_WithSourceEventId_ShouldParseCorrectly()
    {
        // Arrange
        var sourceEventId = Guid.NewGuid();
        var journalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = "JE-001",
            EntryDate = DateTime.UtcNow,
            Description = "Test",
            Status = EntryStatus.Posted,
            PeriodId = Guid.NewGuid(),
            SourceEventId = sourceEventId.ToString(),
            Lines = new List<JournalEntryLine>()
        };

        // Act
        var response = journalEntry.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.SourceEventId);
        Assert.Equal(sourceEventId, response.SourceEventId);
    }

    [Fact]
    public void ToResponse_WithNullSourceEventId_ShouldReturnNull()
    {
        // Arrange
        var journalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = "JE-001",
            EntryDate = DateTime.UtcNow,
            Description = "Test",
            Status = EntryStatus.Posted,
            PeriodId = Guid.NewGuid(),
            SourceEventId = null,
            Lines = new List<JournalEntryLine>()
        };

        // Act
        var response = journalEntry.ToResponse();

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.SourceEventId);
    }
}
