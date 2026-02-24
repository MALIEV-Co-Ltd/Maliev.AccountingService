# Event Contracts: Accounting Service

**Feature**: `001-accounting-service-core`
**Date**: 2025-12-05
**Purpose**: Define RabbitMQ event schemas for consumed and published events

## Routing Key Pattern

`maliev.{service}.{version}.{entity}.{action}`

---

## Consumed Events (Subscribed)

### 1. InvoiceCreated Event

**Routing Key**: `maliev.sales.v1.invoice.created`
**Publisher**: Sales Service
**Purpose**: Customer invoice creation triggers AR journal entry

```json
{
  "eventId": "uuid-v4",  // Idempotency key
  "invoiceId": "uuid-v4",
  "customerId": "uuid-v4",
  "invoiceNumber": "INV-2025-001",
  "invoiceDate": "2025-01-15",
  "dueDate": "2025-02-14",
  "amount": 1000.00,
  "taxAmount": 150.00,
  "taxRate": 0.15,
  "currency": "USD",
  "lineItems": [
    {
      "description": "Professional Services",
      "quantity": 10,
      "unitPrice": 100.00,
      "amount": 1000.00,
      "taxAmount": 150.00
    }
  ],
  "timestamp": "2025-01-15T10:30:00Z"
}
```

**Accounting Action**: Create journal entry:
- DR Accounts Receivable (1200) $1,150
- CR Sales Revenue (4000) $1,000
- CR VAT Payable (2300) $150

---

### 2. PaymentReceived Event

**Routing Key**: `maliev.sales.v1.payment.received`
**Publisher**: Sales Service
**Purpose**: Customer payment received triggers cash journal entry

```json
{
  "eventId": "uuid-v4",
  "paymentId": "uuid-v4",
  "customerId": "uuid-v4",
  "invoiceId": "uuid-v4",  // null if payment on account
  "paymentDate": "2025-01-20",
  "amount": 1150.00,
  "paymentMethod": "Bank Transfer",
  "bankAccount": "ACC-001",
  "referenceNumber": "TXN-12345",
  "timestamp": "2025-01-20T14:15:00Z"
}
```

**Accounting Action**: Create journal entry:
- DR Cash/Bank (1100) $1,150
- CR Accounts Receivable (1200) $1,150

---

### 3. SupplierInvoiceReceived Event

**Routing Key**: `maliev.procurement.v1.supplier-invoice.received`
**Publisher**: Procurement Service
**Purpose**: Supplier invoice triggers AP and expense journal entries

```json
{
  "eventId": "uuid-v4",
  "supplierInvoiceId": "uuid-v4",
  "supplierId": "uuid-v4",
  "invoiceNumber": "SUPP-INV-456",
  "invoiceDate": "2025-01-10",
  "dueDate": "2025-02-09",
  "amount": 500.00,
  "taxAmount": 75.00,
  "taxRate": 0.15,
  "expenseCategory": "Office Supplies",
  "accountCode": "5100",  // Suggested GL account
  "lineItems": [
    {
      "description": "Office Supplies",
      "quantity": 50,
      "unitPrice": 10.00,
      "amount": 500.00,
      "taxAmount": 75.00
    }
  ],
  "timestamp": "2025-01-10T09:00:00Z"
}
```

**Accounting Action**: Create journal entry:
- DR Office Supplies Expense (5100) $500
- DR VAT Receivable (1300) $75
- CR Accounts Payable (2100) $575

---

### 4. InventoryMovementRecorded Event

**Routing Key**: `maliev.inventory.v1.stock-movement.recorded`
**Publisher**: Inventory Service
**Purpose**: Stock movements trigger inventory asset adjustments

```json
{
  "eventId": "uuid-v4",
  "movementId": "uuid-v4",
  "movementType": "Purchase" | "Sale" | "Adjustment" | "Transfer",
  "movementDate": "2025-01-12",
  "productId": "uuid-v4",
  "quantity": 100,
  "unitCost": 20.00,
  "totalValue": 2000.00,
  "fromLocation": null,
  "toLocation": "Warehouse-A",
  "referenceType": "PurchaseOrder",
  "referenceId": "uuid-v4",
  "timestamp": "2025-01-12T11:00:00Z"
}
```

**Accounting Action** (for Purchase):
- DR Inventory Asset (1400) $2,000
- CR Accounts Payable (2100) $2,000 or Cash (1100) if paid

**Accounting Action** (for Sale):
- DR Cost of Goods Sold (5000) $2,000
- CR Inventory Asset (1400) $2,000

---

### 5. PayrollProcessed Event

**Routing Key**: `maliev.payroll.v1.payroll.processed`
**Publisher**: Payroll Service
**Purpose**: Payroll processing triggers wage expense and liability entries

```json
{
  "eventId": "uuid-v4",
  "payrollId": "uuid-v4",
  "payPeriodStart": "2025-01-01",
  "payPeriodEnd": "2025-01-15",
  "paymentDate": "2025-01-20",
  "totalGrossWages": 50000.00,
  "totalTaxWithholdings": 10000.00,
  "totalBenefitDeductions": 5000.00,
  "totalNetPay": 35000.00,
  "employeeCount": 25,
  "breakdown": {
    "federalTax": 7000.00,
    "stateTax": 2000.00,
    "socialSecurity": 1000.00,
    "healthInsurance": 3000.00,
    "retirement401k": 2000.00
  },
  "timestamp": "2025-01-20T08:00:00Z"
}
```

**Accounting Action**: Create journal entry:
- DR Payroll Expense (6000) $50,000
- CR Wages Payable (2200) $35,000
- CR Tax Withholdings Payable (2210) $10,000
- CR Benefit Deductions Payable (2220) $5,000

---

## Published Events (Emitted)

### 1. TransactionPosted Event

**Routing Key**: `maliev.accounting.v1.transaction.posted`
**Subscriber**: Analytics Service, Reporting Service
**Purpose**: Notify downstream services of posted journal entries

```json
{
  "eventId": "uuid-v4",
  "journalEntryId": "uuid-v4",
  "entryNumber": "JE-2025-001234",
  "entryDate": "2025-01-15",
  "periodId": "uuid-v4",
  "periodName": "January 2025",
  "sourceSystem": "Sales",
  "sourceEventId": "uuid-v4",
  "totalDebit": 1150.00,
  "totalCredit": 1150.00,
  "lineCount": 3,
  "postedAt": "2025-01-15T10:35:00Z",
  "postedBy": "uuid-v4",
  "accountsAffected": [
    {
      "accountId": "uuid-v4",
      "accountNumber": "1200",
      "accountName": "Accounts Receivable",
      "debitAmount": 1150.00,
      "creditAmount": 0.00
    },
    {
      "accountId": "uuid-v4",
      "accountNumber": "4000",
      "accountName": "Sales Revenue",
      "debitAmount": 0.00,
      "creditAmount": 1000.00
    },
    {
      "accountId": "uuid-v4",
      "accountNumber": "2300",
      "accountName": "VAT Payable",
      "debitAmount": 0.00,
      "creditAmount": 150.00
    }
  ],
  "timestamp": "2025-01-15T10:35:05Z"
}
```

---

### 2. PeriodClosed Event

**Routing Key**: `maliev.accounting.v1.period.closed`
**Subscriber**: Reporting Service, Analytics Service
**Purpose**: Notify that period is closed for transactions

```json
{
  "eventId": "uuid-v4",
  "periodId": "uuid-v4",
  "periodName": "January 2025",
  "fiscalYearName": "FY 2025",
  "startDate": "2025-01-01",
  "endDate": "2025-01-31",
  "closedAt": "2025-02-05T17:00:00Z",
  "closedBy": "uuid-v4",
  "transactionCount": 1234,
  "totalDebitAmount": 1500000.00,
  "totalCreditAmount": 1500000.00,
  "timestamp": "2025-02-05T17:00:05Z"
}
```

---

### 3. ReconciliationCompleted Event

**Routing Key**: `maliev.accounting.v1.reconciliation.completed`
**Subscriber**: Analytics Service, Alert Service
**Purpose**: Notify of reconciliation results and discrepancies

```json
{
  "eventId": "uuid-v4",
  "reconciliationId": "uuid-v4",
  "reconciliationType": "Accounts_Receivable",
  "periodId": "uuid-v4",
  "periodName": "January 2025",
  "runAt": "2025-02-01T10:00:00Z",
  "runBy": "uuid-v4",
  "subledgerTotal": 50000.00,
  "generalLedgerTotal": 48500.00,
  "varianceAmount": 1500.00,
  "status": "Discrepancy",
  "discrepancyCount": 3,
  "requiresInvestigation": true,
  "timestamp": "2025-02-01T10:05:00Z"
}
```

---

## Event Validation Rules

### All Events Must Include

1. **eventId** (string, UUID): Unique identifier for idempotency
2. **timestamp** (string, ISO 8601): Event creation time
3. **sourceSystem** or equivalent context

### Idempotency

- Publishers MUST generate unique `eventId` for each event
- Consumers MUST check `eventId` against processed event registry before processing
- Duplicate `eventId` should be acknowledged without reprocessing

### Retry Behavior

- Consumers will retry transient failures with exponential backoff (2s, 4s, 8s)
- After 3 retries, message moved to dead-letter queue
- Permanent errors (validation failures) immediately routed to DLQ

### Schema Evolution

- Breaking changes require new routing key version (v2)
- Additive changes (new optional fields) allowed in v1
- Removing fields requires v2 migration period with dual consumption

## Message Properties

### Headers (All Messages)

```json
{
  "content-type": "application/json",
  "content-encoding": "utf-8",
  "message-id": "uuid-v4",
  "timestamp": "unix-epoch-ms",
  "app-id": "service-name",
  "correlation-id": "distributed-trace-id",  // W3C Trace Context
  "traceparent": "00-{trace-id}-{span-id}-01"  // OpenTelemetry
}
```

### RabbitMQ Exchange Configuration

- **Type**: Topic
- **Durability**: Durable
- **Auto-delete**: False

### Queue Configuration (Per Consumer)

- **Durability**: Durable
- **Auto-acknowledge**: False (manual ack after processing)
- **Prefetch count**: 10
- **Message TTL**: 24 hours
- **Dead-letter exchange**: `maliev.accounting.dlx`

## Testing Event Contracts

```csharp
[Fact]
public async Task InvoiceCreatedConsumer_ValidEvent_CreatesJournalEntry()
{
    // Arrange
    var evt = new InvoiceCreatedEvent
    {
        EventId = Guid.NewGuid().ToString(),
        InvoiceId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        Amount = 1000.00m,
        TaxAmount = 150.00m,
        // ...
    };

    // Act
    await _consumer.Consume(evt);

    // Assert
    var journalEntry = await _dbContext.JournalEntries
        .Include(e => e.Lines)
        .FirstOrDefaultAsync(e => e.SourceEventId == evt.EventId);

    Assert.NotNull(journalEntry);
    Assert.Equal(1150.00m, journalEntry.TotalDebit);
    Assert.Equal(1150.00m, journalEntry.TotalCredit);
    Assert.Equal(3, journalEntry.Lines.Count);
}
```

## Event Schema Versioning Strategy

| Change Type | Action Required |
|-------------|------------------|
| Add optional field | Update v1, no migration |
| Add required field | Create v2, support both for 6 months |
| Remove field | Create v2, deprecate v1 after 6 months |
| Rename field | Create v2, map old→new in consumers |
| Change field type | Create v2, support both for 6 months |

## Monitoring & Alerting

Emit metrics for:
- Event consumption rate (events/sec)
- Event processing latency (ms)
- Event processing success rate (%)
- DLQ message count (count)
- Idempotency cache hit rate (%)

Alert on:
- DLQ message count > 10 (investigate within 1 hour)
- Event processing latency > 5 minutes (investigate immediately)
- Idempotency cache miss rate > 5% (potential Redis issues)
