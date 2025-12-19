# Data Model: Accounting Service Core

**Feature**: `001-accounting-service-core`
**Date**: 2025-12-05
**Purpose**: Define database schema, entities, relationships, and constraints

## Entity Relationship Diagram

```
[FiscalYear] 1──∞ [FinancialPeriod]
                    │
                    │ 1
                    │
                    ∞
[ChartOfAccount] ∞──∞ [JournalEntry] ∞──1 [User]
    │                     │
    │ (self-ref)          │ 1
    │                     │
    └─────────────────────∞
                    [JournalEntryLine] 1──∞ [TaxComponent]

[SubledgerTransaction] ∞──1 [JournalEntry]

[ReconciliationReport] ∞──1 [FinancialPeriod]

[AdjustingEntryApproval] 1──1 [JournalEntry]

[ProcessedEventRegistry] (standalone, no FK)

[TaxRate] (standalone, queried by tax_type + transaction date)

[AuditTrailEntry] (standalone, references any entity by type+id)
```

## Core Entities

### 1. ChartOfAccount

**Purpose**: Catalog of all accounts used for financial classification

**Table**: `chart_of_accounts`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| account_number | VARCHAR(20) | NOT NULL, UNIQUE | Account identifier (e.g., "1200") |
| name | VARCHAR(200) | NOT NULL | Account name (e.g., "Accounts Receivable") |
| type | VARCHAR(20) | NOT NULL | Asset, Liability, Equity, Revenue, Expense |
| category | VARCHAR(100) | NULL | Subcategory (e.g., "Current Assets") |
| parent_account_id | UUID | FK (chart_of_accounts.id), NULL | Parent for hierarchy |
| is_active | BOOLEAN | NOT NULL, DEFAULT true | Active/deactivated status |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |
| modified_at | TIMESTAMP | NULL | Last modification timestamp |

**Indexes**:
- `PK_chart_of_accounts` on `id`
- `UX_chart_of_accounts_account_number` UNIQUE on `account_number`
- `IX_chart_of_accounts_parent_account_id` on `parent_account_id`
- `IX_chart_of_accounts_type` on `type`
- `IX_chart_of_accounts_is_active` on `is_active` WHERE `is_active = true`

**Relationships**:
- Self-referencing: `parent_account_id` → `id` (adjacency list hierarchy)
- One-to-Many: ChartOfAccount → JournalEntryLine

**Validation Rules**:
- Account number must follow organizational format (configurable regex)
- Cannot deactivate account if it has transactions in open periods
- Cannot delete account (only deactivate)
- Parent account must be of same or higher-level type (e.g., Asset child under Asset parent)

**Constraints**:
```sql
ALTER TABLE chart_of_accounts
ADD CONSTRAINT chk_account_type CHECK (type IN ('Asset', 'Liability', 'Equity', 'Revenue', 'Expense'));

ALTER TABLE chart_of_accounts
ADD CONSTRAINT chk_account_number_format CHECK (account_number ~ '^[0-9]{4,10}$');
```

---

### 2. FiscalYear

**Purpose**: Define annual accounting cycles

**Table**: `fiscal_years`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| name | VARCHAR(50) | NOT NULL, UNIQUE | Year name (e.g., "FY 2025") |
| start_date | DATE | NOT NULL | Fiscal year start |
| end_date | DATE | NOT NULL | Fiscal year end |
| period_structure | VARCHAR(20) | NOT NULL | Monthly, Quarterly |
| is_active | BOOLEAN | NOT NULL, DEFAULT true | Current fiscal year flag |
| created_at | TIMESTAMP | NOT NULL | Creation timestamp |

**Indexes**:
- `PK_fiscal_years` on `id`
- `UX_fiscal_years_name` UNIQUE on `name`
- `IX_fiscal_years_dates` on `start_date, end_date`
- `IX_fiscal_years_is_active` on `is_active` WHERE `is_active = true`

**Relationships**:
- One-to-Many: FiscalYear → FinancialPeriod

**Validation Rules**:
- End date must be after start date
- Only one fiscal year can be active at a time
- Fiscal years cannot overlap

**Constraints**:
```sql
ALTER TABLE fiscal_years
ADD CONSTRAINT chk_fiscal_year_dates CHECK (end_date > start_date);

ALTER TABLE fiscal_years
ADD CONSTRAINT chk_period_structure CHECK (period_structure IN ('Monthly', 'Quarterly'));
```

---

### 3. FinancialPeriod

**Purpose**: Time intervals for organizing financial activity

**Table**: `financial_periods`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| fiscal_year_id | UUID | FK (fiscal_years.id), NOT NULL | Parent fiscal year |
| name | VARCHAR(50) | NOT NULL | Period name (e.g., "January 2025") |
| start_date | DATE | NOT NULL | Period start |
| end_date | DATE | NOT NULL | Period end |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'Open' | Open, Closed, Locked |
| closed_at | TIMESTAMP | NULL | When period was closed |
| closed_by | UUID | FK (users.id), NULL | User who closed period |
| row_version | BYTEA | NOT NULL | Concurrency token |

**Indexes**:
- `PK_financial_periods` on `id`
- `IX_financial_periods_fiscal_year_id` on `fiscal_year_id`
- `IX_financial_periods_dates` on `start_date, end_date`
- `IX_financial_periods_status` on `status`
- `UX_financial_periods_name_year` UNIQUE on `name, fiscal_year_id`

**Relationships**:
- Many-to-One: FinancialPeriod → FiscalYear
- One-to-Many: FinancialPeriod → JournalEntry
- One-to-Many: FinancialPeriod → ReconciliationReport

**Validation Rules**:
- Period must fall within fiscal year date range
- Cannot close period with draft journal entries
- Cannot reopen closed period (requires special adjustment workflow)
- Period dates cannot overlap within same fiscal year

**Constraints**:
```sql
ALTER TABLE financial_periods
ADD CONSTRAINT chk_period_status CHECK (status IN ('Open', 'Closed', 'Locked'));

ALTER TABLE financial_periods
ADD CONSTRAINT chk_period_dates CHECK (end_date > start_date);
```

---

### 4. JournalEntry

**Purpose**: Financial transaction header (double-entry bookkeeping)

**Table**: `journal_entries`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| period_id | UUID | FK (financial_periods.id), NOT NULL | Associated period |
| entry_number | VARCHAR(50) | NOT NULL, UNIQUE | Sequential entry number |
| entry_date | DATE | NOT NULL | Transaction date |
| description | VARCHAR(500) | NOT NULL | Entry description |
| status | VARCHAR(20) | NOT NULL, DEFAULT 'Draft' | Draft, Posted |
| source_system | VARCHAR(50) | NULL | Sales, Procurement, Inventory, Payroll |
| source_event_id | VARCHAR(100) | NULL | Original event ID |
| total_debit | DECIMAL(18,2) | NOT NULL, DEFAULT 0 | Sum of debit lines |
| total_credit | DECIMAL(18,2) | NOT NULL, DEFAULT 0 | Sum of credit lines |
| created_at | TIMESTAMP | NOT NULL | Creation timestamp |
| created_by | UUID | FK (users.id), NOT NULL | Creating user |
| posted_at | TIMESTAMP | NULL | Posting timestamp |
| posted_by | UUID | FK (users.id), NULL | Posting user |
| row_version | BYTEA | NOT NULL | Concurrency token |

**Indexes**:
- `PK_journal_entries` on `id`
- `UX_journal_entries_entry_number` UNIQUE on `entry_number`
- `IX_journal_entries_period_id_status` on `period_id, status`
- `IX_journal_entries_entry_date` on `entry_date`
- `IX_journal_entries_source` on `source_system, source_event_id`
- `IX_journal_entries_status` on `status` WHERE `status = 'Posted'` (partial index)

**Relationships**:
- Many-to-One: JournalEntry → FinancialPeriod
- One-to-Many: JournalEntry → JournalEntryLine
- One-to-One: JournalEntry → AdjustingEntryApproval (for closed period adjustments)
- Many-to-One: JournalEntry → User (created_by, posted_by)

**Validation Rules**:
- Total debits must equal total credits for posted entries (balanced)
- Entry date must fall within period date range
- Cannot modify or delete posted entries
- Cannot post to closed period without adjustment approval

**Constraints**:
```sql
ALTER TABLE journal_entries
ADD CONSTRAINT chk_journal_entry_status CHECK (status IN ('Draft', 'Posted'));

ALTER TABLE journal_entries
ADD CONSTRAINT chk_balanced_entry CHECK (
    status = 'Draft' OR total_debit = total_credit
);

-- Trigger to prevent modification of posted entries
CREATE OR REPLACE FUNCTION prevent_posted_entry_modification()
RETURNS TRIGGER AS $$
BEGIN
    IF OLD.status = 'Posted' AND (TG_OP = 'UPDATE' OR TG_OP = 'DELETE') THEN
        RAISE EXCEPTION 'Cannot modify or delete posted journal entry';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_prevent_posted_modification
BEFORE UPDATE OR DELETE ON journal_entries
FOR EACH ROW EXECUTE FUNCTION prevent_posted_entry_modification();
```

---

### 5. JournalEntryLine

**Purpose**: Individual debit/credit line within journal entry

**Table**: `journal_entry_lines`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| journal_entry_id | UUID | FK (journal_entries.id), NOT NULL | Parent entry |
| account_id | UUID | FK (chart_of_accounts.id), NOT NULL | Associated account |
| line_sequence | INT | NOT NULL | Line order |
| description | VARCHAR(500) | NULL | Line-specific description |
| debit_amount | DECIMAL(18,2) | NOT NULL, DEFAULT 0 | Debit amount |
| credit_amount | DECIMAL(18,2) | NOT NULL, DEFAULT 0 | Credit amount |
| reference_type | VARCHAR(50) | NULL | Invoice, Payment, Receipt, etc. |
| reference_id | VARCHAR(100) | NULL | External reference ID |
| customer_id | UUID | NULL | Customer reference |
| supplier_id | UUID | NULL | Supplier reference |

**Indexes**:
- `PK_journal_entry_lines` on `id`
- `IX_journal_entry_lines_journal_entry_id` on `journal_entry_id`
- `IX_journal_entry_lines_account_id` on `account_id`
- `IX_journal_entry_lines_reference` on `reference_type, reference_id`
- `IX_journal_entry_lines_period_status_account` on `(journal_entry.period_id), (journal_entry.status), account_id` (composite for report queries)

**Relationships**:
- Many-to-One: JournalEntryLine → JournalEntry
- Many-to-One: JournalEntryLine → ChartOfAccount
- One-to-Many: JournalEntryLine → TaxComponent

**Validation Rules**:
- Each line must have either debit OR credit (not both, not neither)
- Account must be active
- Amounts must be non-negative
- Line sequence must be unique within journal entry

**Constraints**:
```sql
ALTER TABLE journal_entry_lines
ADD CONSTRAINT chk_debit_or_credit CHECK (
    (debit_amount > 0 AND credit_amount = 0) OR
    (credit_amount > 0 AND debit_amount = 0)
);

ALTER TABLE journal_entry_lines
ADD CONSTRAINT chk_positive_amounts CHECK (
    debit_amount >= 0 AND credit_amount >= 0
);
```

---

### 6. TaxComponent

**Purpose**: Tax obligations/recoveries associated with transactions

**Table**: `tax_components`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| journal_entry_line_id | UUID | FK (journal_entry_lines.id), NOT NULL | Parent line |
| tax_type | VARCHAR(50) | NOT NULL | VAT_Output, VAT_Input, Sales_Tax, etc. |
| tax_rate | DECIMAL(5,4) | NOT NULL | Tax rate (e.g., 0.15 for 15%) |
| taxable_amount | DECIMAL(18,2) | NOT NULL | Base amount for tax calculation |
| tax_amount | DECIMAL(18,2) | NOT NULL | Calculated tax |
| reporting_period_id | UUID | FK (financial_periods.id), NULL | Tax reporting period |

**Indexes**:
- `PK_tax_components` on `id`
- `IX_tax_components_journal_entry_line_id` on `journal_entry_line_id`
- `IX_tax_components_reporting_period_type` on `reporting_period_id, tax_type`

**Relationships**:
- Many-to-One: TaxComponent → JournalEntryLine
- Many-to-One: TaxComponent → FinancialPeriod (reporting period)

**Validation Rules**:
- Tax amount must equal taxable_amount × tax_rate
- Tax rate must be between 0 and 1 (0% to 100%)
- Taxable amount must be positive

**Constraints**:
```sql
ALTER TABLE tax_components
ADD CONSTRAINT chk_tax_calculation CHECK (
    ABS(tax_amount - (taxable_amount * tax_rate)) < 0.01
);

ALTER TABLE tax_components
ADD CONSTRAINT chk_tax_rate_range CHECK (tax_rate >= 0 AND tax_rate <= 1);
```

---

### 7. SubledgerTransaction

**Purpose**: Track source system transactions linked to journal entries

**Table**: `subledger_transactions`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| source_system | VARCHAR(50) | NOT NULL | Sales, Procurement, Inventory, Payroll |
| transaction_type | VARCHAR(50) | NOT NULL | Invoice, Payment, Receipt, PO, etc. |
| transaction_id | VARCHAR(100) | NOT NULL | ID in source system |
| transaction_date | DATE | NOT NULL | Transaction date |
| amount | DECIMAL(18,2) | NOT NULL | Transaction amount |
| tax_amount | DECIMAL(18,2) | NULL | Tax component |
| customer_id | UUID | NULL | Customer reference |
| supplier_id | UUID | NULL | Supplier reference |
| journal_entry_id | UUID | FK (journal_entries.id), NULL | Linked journal entry |
| processed_at | TIMESTAMP | NULL | When processed |

**Indexes**:
- `PK_subledger_transactions` on `id`
- `UX_subledger_transactions_source` UNIQUE on `source_system, transaction_id`
- `IX_subledger_transactions_journal_entry_id` on `journal_entry_id`
- `IX_subledger_transactions_date` on `transaction_date`

**Relationships**:
- Many-to-One: SubledgerTransaction → JournalEntry

**Validation Rules**:
- Source system and transaction ID combination must be unique
- Transaction must be linked to journal entry after processing

---

### 8. ReconciliationReport

**Purpose**: Results of subledger-to-GL reconciliation

**Table**: `reconciliation_reports`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| period_id | UUID | FK (financial_periods.id), NOT NULL | Reporting period |
| reconciliation_type | VARCHAR(50) | NOT NULL | Accounts_Receivable, Accounts_Payable, Inventory |
| run_at | TIMESTAMP | NOT NULL | When reconciliation ran |
| run_by | UUID | FK (users.id), NOT NULL | User who ran reconciliation |
| subledger_total | DECIMAL(18,2) | NOT NULL | Total from subledger |
| general_ledger_total | DECIMAL(18,2) | NOT NULL | Total from GL |
| variance_amount | DECIMAL(18,2) | NOT NULL | Difference |
| status | VARCHAR(20) | NOT NULL | Matched, Discrepancy, Investigating |
| discrepancy_details | JSONB | NULL | Detailed variance breakdown |
| resolved_at | TIMESTAMP | NULL | When resolved |
| resolved_by | UUID | FK (users.id), NULL | Resolver |

**Indexes**:
- `PK_reconciliation_reports` on `id`
- `IX_reconciliation_reports_period_type` on `period_id, reconciliation_type`
- `IX_reconciliation_reports_status` on `status` WHERE `status = 'Discrepancy'`

**Relationships**:
- Many-to-One: ReconciliationReport → FinancialPeriod
- Many-to-One: ReconciliationReport → User (run_by, resolved_by)

**Validation Rules**:
- Variance amount must equal subledger_total - general_ledger_total
- Status must progress: Matched (no variance) or Discrepancy → Investigating → Resolved

**Constraints**:
```sql
ALTER TABLE reconciliation_reports
ADD CONSTRAINT chk_variance_calculation CHECK (
    variance_amount = subledger_total - general_ledger_total
);
```

---

### 9. AdjustingEntryApproval

**Purpose**: Authorization for posting to closed periods

**Table**: `adjusting_entry_approvals`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| journal_entry_id | UUID | FK (journal_entries.id), NOT NULL, UNIQUE | Adjusting entry |
| requesting_user_id | UUID | FK (users.id), NOT NULL | Requester |
| approving_user_id | UUID | FK (users.id), NULL | Approver |
| reason | TEXT | NOT NULL | Justification for adjustment |
| requested_at | TIMESTAMP | NOT NULL | Request timestamp |
| approved_at | TIMESTAMP | NULL | Approval timestamp |
| status | VARCHAR(20) | NOT NULL | Pending, Approved, Rejected |

**Indexes**:
- `PK_adjusting_entry_approvals` on `id`
- `UX_adjusting_entry_approvals_journal_entry_id` UNIQUE on `journal_entry_id`
- `IX_adjusting_entry_approvals_status` on `status`

**Relationships**:
- One-to-One: AdjustingEntryApproval → JournalEntry
- Many-to-One: AdjustingEntryApproval → User (requesting, approving)

**Validation Rules**:
- Journal entry must be for closed period
- Cannot approve without approver assignment
- Rejected approvals cannot be reused (create new approval)

---

### 10. ProcessedEventRegistry

**Purpose**: Idempotency tracking for event deduplication

**Table**: `processed_event_registry` (also stored in Redis with 24h TTL)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| event_id | VARCHAR(100) | NOT NULL, UNIQUE | Event ID from source system |
| source_system | VARCHAR(50) | NOT NULL | Originating service |
| event_type | VARCHAR(50) | NOT NULL | Event type |
| journal_entry_id | UUID | FK (journal_entries.id), NULL | Created journal entry |
| processed_at | TIMESTAMP | NOT NULL | Processing timestamp |
| expires_at | TIMESTAMP | NOT NULL | Expiration for cleanup |

**Indexes**:
- `PK_processed_event_registry` on `id`
- `UX_processed_event_registry_event_id` UNIQUE on `event_id`
- `IX_processed_event_registry_expires_at` on `expires_at` (for cleanup jobs)

**Relationships**:
- Many-to-One: ProcessedEventRegistry → JournalEntry

**Validation Rules**:
- Event ID must be unique
- Expires_at should be 24 hours after processed_at

**Maintenance**:
```sql
-- Cleanup job (run daily)
DELETE FROM processed_event_registry
WHERE expires_at < NOW();
```

---

### 11. AuditTrailEntry

**Purpose**: Immutable audit log of all data changes

**Table**: `audit_trail` (append-only, UPDATE/DELETE revoked)

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| entity_type | VARCHAR(100) | NOT NULL | Entity name |
| entity_id | UUID | NOT NULL | Entity primary key |
| action | VARCHAR(50) | NOT NULL | Created, Updated, Posted, Closed, etc. |
| user_id | UUID | NOT NULL | Acting user |
| timestamp | TIMESTAMP | NOT NULL | Action timestamp |
| before_values | JSONB | NULL | State before change |
| after_values | JSONB | NULL | State after change |
| ip_address | VARCHAR(45) | NULL | Client IP |
| correlation_id | VARCHAR(100) | NULL | Distributed trace ID |

**Indexes**:
- `PK_audit_trail` on `id`
- `IX_audit_trail_entity` on `entity_type, entity_id, timestamp`
- `IX_audit_trail_user_id` on `user_id`
- `IX_audit_trail_timestamp` on `timestamp`
- `IX_audit_trail_correlation_id` on `correlation_id`

**Relationships**:
- None (loosely coupled via entity_type + entity_id)

**Validation Rules**:
- All fields required except before_values (for Create actions)
- Cannot UPDATE or DELETE rows (enforced by DB permissions)

**Constraints**:
```sql
-- Revoke modification permissions
REVOKE UPDATE, DELETE ON audit_trail FROM accounting_app_user;
GRANT SELECT, INSERT ON audit_trail TO accounting_app_user;
```

---

### 12. TaxRate

**Purpose**: Store tax rate configurations with temporal validity for applying correct rates based on transaction dates

**Table**: `tax_rates`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| tax_type | VARCHAR(50) | NOT NULL | VAT, Sales_Tax, etc. |
| rate_percentage | DECIMAL(5,4) | NOT NULL | Tax rate (e.g., 0.15 for 15%) |
| effective_start_date | DATE | NOT NULL | Rate becomes effective |
| effective_end_date | DATE | NULL | Rate expires (null = current rate) |
| jurisdiction | VARCHAR(100) | NOT NULL DEFAULT 'TH' | TH, US, etc. |
| description | VARCHAR(200) | NULL | Rate description |
| created_at | TIMESTAMP | NOT NULL, DEFAULT NOW() | Creation timestamp |

**Indexes**:
- `PK_tax_rates` on `id`
- `UX_tax_rates_temporal` UNIQUE on `tax_type, jurisdiction, effective_start_date`
- `IX_tax_rates_effective_dates` on `effective_start_date, effective_end_date`

**Relationships**:
- None (referenced by TaxComponent via tax_type and transaction date lookup)

**Validation Rules**:
- Rate percentage must be between 0 and 1 (0% to 100%)
- Effective start date must be before end date (if specified)
- Only one active rate per tax_type + jurisdiction at any given time (no overlapping date ranges)

**Constraints**:
```sql
ALTER TABLE tax_rates
ADD CONSTRAINT chk_tax_rate_percentage CHECK (rate_percentage >= 0 AND rate_percentage <= 1);

ALTER TABLE tax_rates
ADD CONSTRAINT chk_tax_rate_dates CHECK (
    effective_end_date IS NULL OR effective_end_date > effective_start_date
);
```

**Query Pattern (for FR-035)**:
```sql
-- Get applicable tax rate for a transaction date
SELECT rate_percentage
FROM tax_rates
WHERE tax_type = 'VAT'
  AND jurisdiction = 'TH'
  AND effective_start_date <= '2025-04-15'
  AND (effective_end_date IS NULL OR effective_end_date > '2025-04-15')
ORDER BY effective_start_date DESC
LIMIT 1;
```

---

## Database Performance Optimizations

### Query Performance Indexes

```sql
-- Trial balance query optimization
CREATE INDEX IX_journal_entry_lines_report_query
ON journal_entry_lines (account_id, (debit_amount - credit_amount))
WHERE journal_entry_id IN (
    SELECT id FROM journal_entries WHERE status = 'Posted'
);

-- Period-based reporting
CREATE INDEX IX_journal_entries_period_status_date
ON journal_entries (period_id, status, entry_date)
WHERE status = 'Posted';

-- Tax reporting
CREATE INDEX IX_tax_components_period_type_rate
ON tax_components (reporting_period_id, tax_type, tax_rate);
```

### Table Partitioning (Future Optimization)

```sql
-- Partition audit_trail by timestamp (implement when > 10M rows)
CREATE TABLE audit_trail_2025 PARTITION OF audit_trail
FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');

-- Partition journal_entries by fiscal year (implement when > 5M entries)
CREATE TABLE journal_entries_2025 PARTITION OF journal_entries
FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');
```

## Data Integrity Summary

| Principle | Implementation |
|-----------|----------------|
| Balanced entries | DB check constraint + application validation |
| Immutable posted entries | DB trigger preventing modification/deletion |
| Period locking | Status checks + row-level locking (SELECT FOR UPDATE) |
| Idempotency | Unique event ID constraint + Redis registry |
| Audit completeness | Append-only audit table with revoked UPDATE/DELETE |
| Referential integrity | Foreign key constraints with appropriate ON DELETE rules |
| Concurrency safety | Row version (timestamp) tokens on critical tables |

## Entity Creation Order (for Migrations)

1. FiscalYear
2. FinancialPeriod
3. ChartOfAccount
4. TaxRate (standalone, no FK dependencies)
5. JournalEntry
6. JournalEntryLine
7. TaxComponent
8. SubledgerTransaction
9. ReconciliationReport
10. AdjustingEntryApproval
11. ProcessedEventRegistry
12. AuditTrailEntry

## Estimated Data Volumes (Year 1)

| Entity | Estimated Rows | Growth Rate |
|--------|----------------|-------------|
| ChartOfAccount | 200-300 | Low (5-10/year) |
| FiscalYear | 1-2 | 1/year |
| FinancialPeriod | 12-24 | 12/year |
| JournalEntry | 240,000 | ~1,000/day |
| JournalEntryLine | 960,000 | ~4 lines/entry |
| TaxComponent | 480,000 | ~50% of lines have tax |
| SubledgerTransaction | 240,000 | 1:1 with journal entries |
| ReconciliationReport | 144 | 12/month × 3 types |
| AdjustingEntryApproval | 50-100 | Low, exception cases |
| ProcessedEventRegistry | ~6,000 | 24h sliding window |
| AuditTrailEntry | 1,200,000 | ~5 audit entries/change |

**Total database size estimate (Year 1)**: ~5GB (excluding indexes)
**Index overhead**: ~30% additional (~1.5GB)
**Audit trail storage**: ~2GB/year (recommend separate volume)
