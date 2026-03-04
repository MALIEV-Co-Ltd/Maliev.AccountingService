using System.Net;
using System.Net.Http.Json;
using Maliev.MessagingContracts.Contracts.Accounting;
using Maliev.MessagingContracts.Contracts.Invoices;
using Maliev.MessagingContracts;
using Maliev.AccountingService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

/// <summary>
/// Integration tests for end-to-end event processing flow validation.
/// Tests event ingestion from RabbitMQ through to journal entry creation with audit trails.
/// </summary>
public class EventProcessingTests : BaseIntegrationTest
{
    public EventProcessingTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task InvoiceCreatedEvent_ShouldCreateBalancedJournalEntry_WithCorrectAccounting()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(dbContext);

        var messageId = Guid.NewGuid();
        var invoiceEvent = new InvoiceCreatedEvent(
            MessageId: messageId,
            MessageName: nameof(InvoiceCreatedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new InvoiceCreatedEventPayload
            {
                InvoiceId = Guid.NewGuid(),
                InvoiceNumber = "INV-2025-001",
                CustomerId = Guid.NewGuid(),
                TotalAmount = 1150.0,
                Currency = "THB",
                CreatedAt = DateTimeOffset.UtcNow
            }
        );

        // Act
        await Factory.PublishEventAsync(invoiceEvent);
        
        // Wait for async processing - give more time for RabbitMQ to deliver and process
        await Task.Delay(5000);

        // Verify journal entry was created - get fresh context
        var dbContext2 = Factory.GetDbContext();
        var journalEntry = await dbContext2.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(je => je.SourceEventId == messageId.ToString());

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);

        // Verify balanced entry (debits = credits)
        Assert.Equal(journalEntry.TotalDebit, journalEntry.TotalCredit);
        Assert.Equal(1150.00m, journalEntry.TotalDebit);
    }

    [Fact]
    public async Task PaymentReceivedEvent_ShouldCreateJournalEntry_WithCashDebitAndARCredit()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var messageId = Guid.NewGuid();
        var paymentEvent = new PaymentRecordedEvent(
            MessageId: messageId,
            MessageName: nameof(PaymentRecordedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new PaymentRecordedEventPayload
            {
                PaymentId = Guid.NewGuid(),
                PaymentNumber = "PAY-001",
                PaymentDate = DateTimeOffset.UtcNow,
                Amount = 1150.0,
                PaymentMethod = "Bank Transfer",
                Currency = "THB",
                CustomerId = Guid.NewGuid()
            }
        );

        // Act
        await Factory.PublishEventAsync(paymentEvent);
        await Task.Delay(5000);

        var dbContext = Factory.GetDbContext();
        var journalEntry = await dbContext.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(je => je.SourceEventId == messageId.ToString());

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);
        Assert.Equal(1150.00m, journalEntry.TotalDebit);
    }

    [Fact]
    public async Task SupplierInvoiceEvent_ShouldCreateJournalEntry_WithExpenseDebitVATDebitAndAPCredit()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var messageId = Guid.NewGuid();
        var supplierEvent = new SupplierInvoiceReceivedEvent(
            MessageId: messageId,
            MessageName: nameof(SupplierInvoiceReceivedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new SupplierInvoiceReceivedEventPayload
            {
                SupplierInvoiceId = Guid.NewGuid(),
                InvoiceNumber = "SUPP-001",
                InvoiceDate = DateTimeOffset.UtcNow,
                SupplierId = Guid.NewGuid(),
                SupplierName = "Test Supplier",
                TotalAmount = 575.0,
                Currency = "THB"
            }
        );

        // Act
        await Factory.PublishEventAsync(supplierEvent);
        await Task.Delay(5000);

        var dbContext = Factory.GetDbContext();
        var journalEntry = await dbContext.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(je => je.SourceEventId == messageId.ToString());

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);
        Assert.Equal(575.00m, journalEntry.TotalDebit);
    }

    [Fact]
    public async Task InventoryMovementEvent_ShouldCreateJournalEntry_WithInventoryDebit()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var messageId = Guid.NewGuid();
        var inventoryEvent = new InventoryMovementEvent(
            MessageId: messageId,
            MessageName: nameof(InventoryMovementEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new InventoryMovementEventPayload
            {
                MovementId = Guid.NewGuid(),
                MovementNumber = "MOV-001",
                MovementDate = DateTimeOffset.UtcNow,
                MovementType = "Purchase",
                ProductId = Guid.NewGuid(),
                ProductName = "Test Product",
                Quantity = 100,
                TotalCost = 1000.0
            }
        );

        // Act
        await Factory.PublishEventAsync(inventoryEvent);
        await Task.Delay(5000);

        var dbContext = Factory.GetDbContext();
        var journalEntry = await dbContext.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(je => je.SourceEventId == messageId.ToString());

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);
        Assert.Equal(1000.00m, journalEntry.TotalDebit);
    }

    [Fact]
    public async Task PayrollProcessedEvent_ShouldCreateJournalEntry_WithPayrollExpenseDebitAndLiabilityCredits()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var messageId = Guid.NewGuid();
        var payrollEvent = new PayrollProcessedEvent(
            MessageId: messageId,
            MessageName: nameof(PayrollProcessedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new PayrollProcessedEventPayload
            {
                PayrollId = Guid.NewGuid(),
                PayrollNumber = "PAY-001",
                PaymentDate = DateTimeOffset.UtcNow,
                GrossPay = 10000.0,
                NetPay = 7750.0,
                TotalDeductions = 2250.0
            }
        );

        // Act
        await Factory.PublishEventAsync(payrollEvent);
        await Task.Delay(5000);

        var dbContext = Factory.GetDbContext();
        var journalEntry = await dbContext.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(je => je.SourceEventId == messageId.ToString());

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);
        Assert.Equal(10000.00m, journalEntry.TotalDebit);
    }
}
