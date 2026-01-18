using System.Net;
using System.Net.Http.Json;
using Maliev.AccountingService.Api.Events;
using Maliev.AccountingService.Data.Models;
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

        // Verify chart of accounts exists
        var accounts = await dbContext.ChartOfAccounts.ToListAsync();
        Assert.NotEmpty(accounts); // Ensure accounts were seeded

        var invoiceEvent = new InvoiceCreatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = "INV-2025-001",
            InvoiceDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Test Customer",
            SubtotalAmount = 1000.00m,
            TaxAmount = 150.00m,
            TotalAmount = 1150.00m,
            TaxType = "VAT_Output",
            TaxRate = 0.15m
        };

        // Act
        await Factory.PublishEventAsync(invoiceEvent);
        await Task.Delay(3000); // Allow time for async processing

        // Assert - just verify no exception was thrown

        // Verify journal entry was created - get fresh context
        var dbContext2 = Factory.GetDbContext();
        var journalEntry = await dbContext2.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .Include(je => je.Lines!)
                .ThenInclude(l => l.TaxComponents)
            .FirstOrDefaultAsync(je => je.SourceEventId == invoiceEvent.EventId);

        // If null, check what's in the database
        if (journalEntry == null)
        {
            var allEntries = await dbContext2.JournalEntries.ToListAsync();
            var allEvents = await dbContext2.ProcessedEventRegistry.ToListAsync();
            Assert.Fail($"Journal entry not found. Total entries: {allEntries.Count}, Processed events: {allEvents.Count}");
        }

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);

        // Verify balanced entry (debits = credits)
        Assert.Equal(journalEntry.TotalDebit, journalEntry.TotalCredit);
        Assert.Equal(1150.00m, journalEntry.TotalDebit);
        Assert.Equal(1150.00m, journalEntry.TotalCredit);

        // Verify lines: AR debit, Revenue credit, VAT Output credit
        var lines = journalEntry.Lines.ToList();
        Assert.Equal(3, lines.Count);

        // AR Debit line
        var arLine = lines.FirstOrDefault(l => l.DebitAmount > 0 && l.Account.Type == AccountType.Asset);
        Assert.NotNull(arLine);
        Assert.Equal(1150.00m, arLine.DebitAmount);
        Assert.Equal(0m, arLine.CreditAmount);

        // Revenue Credit line
        var revenueLine = lines.FirstOrDefault(l => l.CreditAmount == 1000.00m);
        Assert.NotNull(revenueLine);
        Assert.Equal(0m, revenueLine.DebitAmount);
        Assert.Equal(1000.00m, revenueLine.CreditAmount);

        // VAT Output Credit line
        var vatLine = lines.FirstOrDefault(l => l.CreditAmount == 150.00m);
        Assert.NotNull(vatLine);
        Assert.Equal(0m, vatLine.DebitAmount);
        Assert.Equal(150.00m, vatLine.CreditAmount);

        // Verify tax component
        var taxComponent = vatLine.TaxComponents.FirstOrDefault();
        Assert.NotNull(taxComponent);
        Assert.Equal("VAT_Output", taxComponent.TaxType);
        Assert.Equal(0.15m, taxComponent.TaxRate);
        Assert.Equal(1000.00m, taxComponent.TaxableAmount);
        Assert.Equal(150.00m, taxComponent.TaxAmount);
    }

    [Fact]
    public async Task PaymentReceivedEvent_ShouldCreateJournalEntry_WithCashDebitAndARCredit()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var paymentEvent = new PaymentReceivedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            PaymentId = Guid.NewGuid(),
            PaymentDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            InvoiceId = Guid.NewGuid(),
            Amount = 1150.00m,
            PaymentMethod = "Bank Transfer"
        };

        // Act
        await Factory.PublishEventAsync(paymentEvent);
        await Task.Delay(3000);

        // Assert

        var dbContext = Factory.GetDbContext();
        var journalEntry = await dbContext.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(je => je.SourceEventId == paymentEvent.EventId);

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);

        // Verify balanced entry
        Assert.Equal(journalEntry.TotalDebit, journalEntry.TotalCredit);
        Assert.Equal(1150.00m, journalEntry.TotalDebit);

        var lines = journalEntry.Lines.ToList();
        Assert.Equal(2, lines.Count);

        // Cash/Bank Debit line
        var cashLine = lines.FirstOrDefault(l => l.DebitAmount > 0);
        Assert.NotNull(cashLine);
        Assert.Equal(1150.00m, cashLine.DebitAmount);
        Assert.Equal(0m, cashLine.CreditAmount);

        // AR Credit line
        var arLine = lines.FirstOrDefault(l => l.CreditAmount > 0);
        Assert.NotNull(arLine);
        Assert.Equal(0m, arLine.DebitAmount);
        Assert.Equal(1150.00m, arLine.CreditAmount);
    }

    [Fact]
    public async Task SupplierInvoiceEvent_ShouldCreateJournalEntry_WithExpenseDebitVATDebitAndAPCredit()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var supplierEvent = new SupplierInvoiceEvent
        {
            EventId = Guid.NewGuid().ToString(),
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = "SUPP-2025-001",
            InvoiceDate = DateTime.UtcNow,
            SupplierId = Guid.NewGuid(),
            SupplierName = "Test Supplier",
            SubtotalAmount = 500.00m,
            TaxAmount = 75.00m,
            TotalAmount = 575.00m,
            TaxRate = 0.15m,
            TaxType = "VAT_Input",
            ExpenseCategory = "Office Supplies"
        };

        // Act
        await Factory.PublishEventAsync(supplierEvent);
        await Task.Delay(3000);

        // Assert

        var dbContext = Factory.GetDbContext();
        var journalEntry = await dbContext.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .Include(je => je.Lines!)
                .ThenInclude(l => l.TaxComponents)
            .FirstOrDefaultAsync(je => je.SourceEventId == supplierEvent.EventId);

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);

        // Verify balanced entry
        Assert.Equal(journalEntry.TotalDebit, journalEntry.TotalCredit);
        Assert.Equal(575.00m, journalEntry.TotalDebit);
        Assert.Equal(575.00m, journalEntry.TotalCredit);

        var lines = journalEntry.Lines.ToList();
        Assert.Equal(3, lines.Count);

        // Expense Debit line
        var expenseLine = lines.FirstOrDefault(l => l.DebitAmount == 500.00m);
        Assert.NotNull(expenseLine);
        Assert.Equal(500.00m, expenseLine.DebitAmount);
        Assert.Equal(0m, expenseLine.CreditAmount);

        // VAT Input Debit line
        var vatLine = lines.FirstOrDefault(l => l.DebitAmount == 75.00m);
        Assert.NotNull(vatLine);
        Assert.Equal(75.00m, vatLine.DebitAmount);
        Assert.Equal(0m, vatLine.CreditAmount);

        // Verify VAT input tax component
        var taxComponent = vatLine.TaxComponents.FirstOrDefault();
        Assert.NotNull(taxComponent);
        Assert.Equal("VAT_Input", taxComponent.TaxType);
        Assert.Equal(0.15m, taxComponent.TaxRate);

        // AP Credit line
        var apLine = lines.FirstOrDefault(l => l.CreditAmount == 575.00m);
        Assert.NotNull(apLine);
        Assert.Equal(0m, apLine.DebitAmount);
        Assert.Equal(575.00m, apLine.CreditAmount);
    }

    [Fact]
    public async Task InventoryMovementEvent_ShouldCreateJournalEntry_WithInventoryDebit()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var inventoryEvent = new InventoryMovementEvent
        {
            EventId = Guid.NewGuid().ToString(),
            MovementId = Guid.NewGuid(),
            MovementDate = DateTime.UtcNow,
            MovementType = "Purchase",
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            Quantity = 100,
            UnitCost = 10.00m,
            TotalCost = 1000.00m,
            SupplierId = Guid.NewGuid()
        };

        // Act
        await Factory.PublishEventAsync(inventoryEvent);
        await Task.Delay(3000);

        // Assert

        var dbContext = Factory.GetDbContext();
        var journalEntry = await dbContext.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(je => je.SourceEventId == inventoryEvent.EventId);

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);

        // Verify balanced entry
        Assert.Equal(journalEntry.TotalDebit, journalEntry.TotalCredit);
        Assert.Equal(1000.00m, journalEntry.TotalDebit);

        var lines = journalEntry.Lines.ToList();
        Assert.Equal(2, lines.Count);

        // Inventory Debit line
        var inventoryLine = lines.FirstOrDefault(l => l.DebitAmount == 1000.00m);
        Assert.NotNull(inventoryLine);
        Assert.Equal(1000.00m, inventoryLine.DebitAmount);

        // AP/Cash Credit line
        var creditLine = lines.FirstOrDefault(l => l.CreditAmount == 1000.00m);
        Assert.NotNull(creditLine);
        Assert.Equal(1000.00m, creditLine.CreditAmount);
    }

    [Fact]
    public async Task PayrollProcessedEvent_ShouldCreateJournalEntry_WithPayrollExpenseDebitAndLiabilityCredits()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var payrollEvent = new PayrollProcessedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            PayrollId = Guid.NewGuid(),
            PayrollNumber = "PAY-2025-001",
            PayrollPeriod = "2025-01",
            ProcessedDate = DateTime.UtcNow,
            PayPeriodStart = DateTime.UtcNow.AddDays(-14),
            PayPeriodEnd = DateTime.UtcNow,
            PaymentDate = DateTime.UtcNow,
            GrossPay = 10000.00m,
            EmployeeTax = 1500.00m,
            SocialSecurity = 750.00m,
            TotalDeductions = 2250.00m,
            NetPay = 7750.00m,
            Deductions = new List<PayrollDeduction>
            {
                new PayrollDeduction { DeductionType = "Tax", Amount = 1500.00m },
                new PayrollDeduction { DeductionType = "Pension", Amount = 750.00m }
            }
        };

        // Act
        await Factory.PublishEventAsync(payrollEvent);
        await Task.Delay(3000);

        // Assert

        var dbContext = Factory.GetDbContext();
        var journalEntry = await dbContext.JournalEntries
            .Include(je => je.Lines!)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(je => je.SourceEventId == payrollEvent.EventId);

        Assert.NotNull(journalEntry);
        Assert.Equal(EntryStatus.Posted, journalEntry.Status);

        // Verify balanced entry
        Assert.Equal(journalEntry.TotalDebit, journalEntry.TotalCredit);
        Assert.Equal(10000.00m, journalEntry.TotalDebit);
        Assert.Equal(10000.00m, journalEntry.TotalCredit);

        var lines = journalEntry.Lines.ToList();
        Assert.True(lines.Count >= 3, "Should have payroll expense debit and multiple liability credits");

        // Payroll Expense Debit line
        var expenseLine = lines.FirstOrDefault(l => l.DebitAmount == 10000.00m);
        Assert.NotNull(expenseLine);

        // Verify total credits equal debits
        var totalCredits = lines.Sum(l => l.CreditAmount);
        Assert.Equal(10000.00m, totalCredits);
    }

    [Fact]
    public async Task EventProcessing_ShouldCreateAuditTrailEntry_ForEachTransaction()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var invoiceEvent = new InvoiceCreatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = "INV-AUDIT-001",
            InvoiceDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Audit Test Customer",
            SubtotalAmount = 100.00m,
            TaxAmount = 15.00m,
            TotalAmount = 115.00m,
            TaxType = "VAT_Output",
            TaxRate = 0.15m
        };

        // Act
        await Factory.PublishEventAsync(invoiceEvent);
        await Task.Delay(3000);

        // Assert

        var dbContext = Factory.GetDbContext();
        var journalEntry = await dbContext.JournalEntries
            .FirstOrDefaultAsync(je => je.SourceEventId == invoiceEvent.EventId);

        Assert.NotNull(journalEntry);

        // Verify audit trail entry exists
        var auditEntries = await dbContext.AuditTrailEntries
            .Where(a => a.EntityType == nameof(JournalEntry) && a.EntityId == journalEntry.Id.ToString())
            .ToListAsync();

        Assert.NotEmpty(auditEntries);

        // Should have at least one audit entry for posting
        var postedEntry = auditEntries.FirstOrDefault(a => a.Action == "Posted");
        Assert.NotNull(postedEntry);
        Assert.NotNull(postedEntry.AfterSnapshot);
        // CorrelationId is optional (depends on Activity tracing being set up)
    }

    [Fact]
    public async Task DuplicateEventId_ShouldBeRejected_WithIdempotencyCheck()
    {
        await CleanDatabaseAsync();

        // Arrange
        var setupContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(setupContext);

        var eventId = Guid.NewGuid().ToString();
        var invoiceEvent1 = new InvoiceCreatedEvent
        {
            EventId = eventId,
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = "INV-DUP-001",
            InvoiceDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Duplicate Test",
            SubtotalAmount = 100.00m,
            TaxAmount = 15.00m,
            TotalAmount = 115.00m,
            TaxRate = 0.15m
        };

        var invoiceEvent2 = new InvoiceCreatedEvent
        {
            EventId = eventId, // Same event ID
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = "INV-DUP-002",
            InvoiceDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Duplicate Test 2",
            SubtotalAmount = 200.00m,
            TaxAmount = 30.00m,
            TotalAmount = 230.00m,
            TaxRate = 0.15m
        };

        // Act
        await Factory.PublishEventAsync(invoiceEvent1);
        await Task.Delay(3000); // Allow time for first event to process
        await Factory.PublishEventAsync(invoiceEvent2);
        await Task.Delay(3000); // Allow time for second event (should be idempotent)

        // Assert - verify idempotency worked

        // Verify only ONE journal entry was created
        var dbContext = Factory.GetDbContext();
        var journalEntries = await dbContext.JournalEntries
            .Where(je => je.SourceEventId == eventId)
            .ToListAsync();

        Assert.Single(journalEntries);
        Assert.Equal($"Sales Invoice {invoiceEvent1.InvoiceNumber} - Customer {invoiceEvent1.CustomerId}", journalEntries[0].Description); // First event's description

        // Verify event registry shows processed
        var processedEvent = await dbContext.ProcessedEventRegistry
            .FirstOrDefaultAsync(pe => pe.EventId == eventId);

        Assert.NotNull(processedEvent);
        Assert.Equal(journalEntries[0].Id, processedEvent.JournalEntryId);
    }

    [Fact]
    public async Task EventsWithLineItems_ShouldCoverLineItemClasses()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(dbContext);

        var invoiceEvent = new InvoiceCreatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = "INV-LINES-001",
            InvoiceDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            SubtotalAmount = 100m,
            TaxAmount = 10m,
            TotalAmount = 110m,
            LineItems = new List<InvoiceLineItem>
            {
                new() { ProductId = Guid.NewGuid(), Description = "Product 1", Quantity = 1, UnitPrice = 100m, Amount = 100m }
            }
        };

        // Act
        await Factory.PublishEventAsync(invoiceEvent);
        await Task.Delay(3000);

        // Assert
        var dbContext2 = Factory.GetDbContext();
        var entry1 = await dbContext2.JournalEntries.AnyAsync(e => e.SourceEventId == invoiceEvent.EventId);
        Assert.True(entry1, $"Invoice event failed: {invoiceEvent.EventId}");

        var supplierEvent = new SupplierInvoiceEvent
        {
            EventId = Guid.NewGuid().ToString(),
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = "SUPP-LINES-001",
            InvoiceDate = DateTime.UtcNow,
            SupplierId = Guid.NewGuid(),
            SubtotalAmount = 200m,
            TaxAmount = 20m,
            TotalAmount = 220m,
            LineItems = new List<SupplierInvoiceLineItem>
            {
                new() { Description = "Service 1", Amount = 200m, ExpenseCategory = "Consulting" }
            }
        };

        await Factory.PublishEventAsync(supplierEvent);
        await Task.Delay(3000);

        var dbContext3 = Factory.GetDbContext();
        var entry2 = await dbContext3.JournalEntries.AnyAsync(e => e.SourceEventId == supplierEvent.EventId);
        Assert.True(entry2, $"Supplier event failed: {supplierEvent.EventId}");
    }

    [Fact]
    public async Task InvoiceCreatedEvent_WithNoTax_ShouldCreateBalancedEntry()
    {
        await CleanDatabaseAsync();

        // Arrange
        var dbContext = Factory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(dbContext);

        var invoiceEvent = new InvoiceCreatedEvent
        {
            EventId = Guid.NewGuid().ToString(),
            InvoiceId = Guid.NewGuid(),
            InvoiceNumber = "INV-NOTAX-001",
            InvoiceDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            SubtotalAmount = 500m,
            TaxAmount = 0m,
            TotalAmount = 500m
        };

        // Act
        await Factory.PublishEventAsync(invoiceEvent);
        await Task.Delay(3000);

        // Assert
        var dbContext2 = Factory.GetDbContext();
        var entry = await dbContext2.JournalEntries.Include(e => e.Lines).FirstOrDefaultAsync(e => e.SourceEventId == invoiceEvent.EventId);
        Assert.NotNull(entry);
        Assert.Equal(2, entry.Lines.Count); // AR and Revenue
    }
}
