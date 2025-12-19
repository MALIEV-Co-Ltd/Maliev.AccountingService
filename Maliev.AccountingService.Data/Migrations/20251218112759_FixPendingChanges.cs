using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.AccountingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_adjusting_entry_approvals_journal_entries_JournalEntryId",
                table: "adjusting_entry_approvals");

            migrationBuilder.DropForeignKey(
                name: "FK_chart_of_accounts_chart_of_accounts_ParentAccountId",
                table: "chart_of_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_financial_periods_fiscal_years_FiscalYearId",
                table: "financial_periods");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entries_financial_periods_FinancialPeriodId",
                table: "journal_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entries_financial_periods_PeriodId",
                table: "journal_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entry_lines_chart_of_accounts_AccountId",
                table: "journal_entry_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_entry_lines_journal_entries_JournalEntryId",
                table: "journal_entry_lines");

            migrationBuilder.DropForeignKey(
                name: "FK_reconciliation_reports_financial_periods_FinancialPeriodId",
                table: "reconciliation_reports");

            migrationBuilder.DropForeignKey(
                name: "FK_reconciliation_reports_financial_periods_PeriodId",
                table: "reconciliation_reports");

            migrationBuilder.DropForeignKey(
                name: "FK_subledger_transactions_journal_entries_JournalEntryId",
                table: "subledger_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_tax_components_journal_entry_lines_JournalEntryLineId",
                table: "tax_components");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tax_components",
                table: "tax_components");

            migrationBuilder.DropPrimaryKey(
                name: "PK_subledger_transactions",
                table: "subledger_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reconciliation_reports",
                table: "reconciliation_reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_processed_event_registry",
                table: "processed_event_registry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journal_entry_lines",
                table: "journal_entry_lines");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journal_entries",
                table: "journal_entries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_fiscal_years",
                table: "fiscal_years");

            migrationBuilder.DropPrimaryKey(
                name: "PK_financial_periods",
                table: "financial_periods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_chart_of_accounts",
                table: "chart_of_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_trail_entries",
                table: "audit_trail_entries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_adjusting_entry_approvals",
                table: "adjusting_entry_approvals");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tax_components",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TaxableAmount",
                table: "tax_components",
                newName: "taxable_amount");

            migrationBuilder.RenameColumn(
                name: "TaxType",
                table: "tax_components",
                newName: "tax_type");

            migrationBuilder.RenameColumn(
                name: "TaxRate",
                table: "tax_components",
                newName: "tax_rate");

            migrationBuilder.RenameColumn(
                name: "TaxJurisdiction",
                table: "tax_components",
                newName: "tax_jurisdiction");

            migrationBuilder.RenameColumn(
                name: "TaxAmount",
                table: "tax_components",
                newName: "tax_amount");

            migrationBuilder.RenameColumn(
                name: "JournalEntryLineId",
                table: "tax_components",
                newName: "journal_entry_line_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tax_components",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_tax_components_TaxType",
                table: "tax_components",
                newName: "ix_tax_components_tax_type");

            migrationBuilder.RenameIndex(
                name: "IX_tax_components_JournalEntryLineId",
                table: "tax_components",
                newName: "ix_tax_components_journal_entry_line_id");

            migrationBuilder.RenameColumn(
                name: "Metadata",
                table: "subledger_transactions",
                newName: "metadata");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "subledger_transactions",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "subledger_transactions",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "subledger_transactions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "subledger_transactions",
                newName: "transaction_type");

            migrationBuilder.RenameColumn(
                name: "TransactionDate",
                table: "subledger_transactions",
                newName: "transaction_date");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "subledger_transactions",
                newName: "supplier_id");

            migrationBuilder.RenameColumn(
                name: "SourceTransactionId",
                table: "subledger_transactions",
                newName: "source_transaction_id");

            migrationBuilder.RenameColumn(
                name: "SourceSystem",
                table: "subledger_transactions",
                newName: "source_system");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "subledger_transactions",
                newName: "journal_entry_id");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "subledger_transactions",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "subledger_transactions",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_subledger_transactions_TransactionDate",
                table: "subledger_transactions",
                newName: "ix_subledger_transactions_transaction_date");

            migrationBuilder.RenameIndex(
                name: "IX_subledger_transactions_SourceSystem_SourceTransactionId",
                table: "subledger_transactions",
                newName: "ix_subledger_transactions_source_system_source_transaction_id");

            migrationBuilder.RenameIndex(
                name: "IX_subledger_transactions_JournalEntryId",
                table: "subledger_transactions",
                newName: "ix_subledger_transactions_journal_entry_id");

            migrationBuilder.RenameColumn(
                name: "Variance",
                table: "reconciliation_reports",
                newName: "variance");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "reconciliation_reports",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "reconciliation_reports",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SubledgerTotal",
                table: "reconciliation_reports",
                newName: "subledger_total");

            migrationBuilder.RenameColumn(
                name: "RunBy",
                table: "reconciliation_reports",
                newName: "run_by");

            migrationBuilder.RenameColumn(
                name: "RunAt",
                table: "reconciliation_reports",
                newName: "run_at");

            migrationBuilder.RenameColumn(
                name: "ResolvedBy",
                table: "reconciliation_reports",
                newName: "resolved_by");

            migrationBuilder.RenameColumn(
                name: "ResolvedAt",
                table: "reconciliation_reports",
                newName: "resolved_at");

            migrationBuilder.RenameColumn(
                name: "ResolutionNotes",
                table: "reconciliation_reports",
                newName: "resolution_notes");

            migrationBuilder.RenameColumn(
                name: "ReconciliationType",
                table: "reconciliation_reports",
                newName: "reconciliation_type");

            migrationBuilder.RenameColumn(
                name: "PeriodId",
                table: "reconciliation_reports",
                newName: "period_id");

            migrationBuilder.RenameColumn(
                name: "GeneralLedgerTotal",
                table: "reconciliation_reports",
                newName: "general_ledger_total");

            migrationBuilder.RenameColumn(
                name: "FinancialPeriodId",
                table: "reconciliation_reports",
                newName: "financial_period_id");

            migrationBuilder.RenameColumn(
                name: "DiscrepancyDetails",
                table: "reconciliation_reports",
                newName: "discrepancy_details");

            migrationBuilder.RenameIndex(
                name: "IX_reconciliation_reports_Status",
                table: "reconciliation_reports",
                newName: "ix_reconciliation_reports_status");

            migrationBuilder.RenameIndex(
                name: "IX_reconciliation_reports_PeriodId_ReconciliationType",
                table: "reconciliation_reports",
                newName: "ix_reconciliation_reports_period_id_reconciliation_type");

            migrationBuilder.RenameIndex(
                name: "IX_reconciliation_reports_FinancialPeriodId",
                table: "reconciliation_reports",
                newName: "ix_reconciliation_reports_financial_period_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "processed_event_registry",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProcessedAt",
                table: "processed_event_registry",
                newName: "processed_at");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "processed_event_registry",
                newName: "journal_entry_id");

            migrationBuilder.RenameColumn(
                name: "EventType",
                table: "processed_event_registry",
                newName: "event_type");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "processed_event_registry",
                newName: "event_id");

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                table: "processed_event_registry",
                newName: "correlation_id");

            migrationBuilder.RenameIndex(
                name: "IX_processed_event_registry_ProcessedAt",
                table: "processed_event_registry",
                newName: "ix_processed_event_registry_processed_at");

            migrationBuilder.RenameIndex(
                name: "IX_processed_event_registry_EventId",
                table: "processed_event_registry",
                newName: "ix_processed_event_registry_event_id");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "journal_entry_lines",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "journal_entry_lines",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "journal_entry_lines",
                newName: "supplier_id");

            migrationBuilder.RenameColumn(
                name: "ReferenceType",
                table: "journal_entry_lines",
                newName: "reference_type");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "journal_entry_lines",
                newName: "reference_id");

            migrationBuilder.RenameColumn(
                name: "LineSequence",
                table: "journal_entry_lines",
                newName: "line_sequence");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "journal_entry_lines",
                newName: "journal_entry_id");

            migrationBuilder.RenameColumn(
                name: "DebitAmount",
                table: "journal_entry_lines",
                newName: "debit_amount");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "journal_entry_lines",
                newName: "customer_id");

            migrationBuilder.RenameColumn(
                name: "CreditAmount",
                table: "journal_entry_lines",
                newName: "credit_amount");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "journal_entry_lines",
                newName: "account_id");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entry_lines_SupplierId",
                table: "journal_entry_lines",
                newName: "ix_journal_entry_lines_supplier_id");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entry_lines_JournalEntryId",
                table: "journal_entry_lines",
                newName: "ix_journal_entry_lines_journal_entry_id");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entry_lines_CustomerId",
                table: "journal_entry_lines",
                newName: "ix_journal_entry_lines_customer_id");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entry_lines_AccountId",
                table: "journal_entry_lines",
                newName: "ix_journal_entry_lines_account_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "journal_entries",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "journal_entries",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "journal_entries",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TotalDebit",
                table: "journal_entries",
                newName: "total_debit");

            migrationBuilder.RenameColumn(
                name: "TotalCredit",
                table: "journal_entries",
                newName: "total_credit");

            migrationBuilder.RenameColumn(
                name: "SourceSystem",
                table: "journal_entries",
                newName: "source_system");

            migrationBuilder.RenameColumn(
                name: "SourceEventId",
                table: "journal_entries",
                newName: "source_event_id");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "journal_entries",
                newName: "row_version");

            migrationBuilder.RenameColumn(
                name: "PostedBy",
                table: "journal_entries",
                newName: "posted_by");

            migrationBuilder.RenameColumn(
                name: "PostedAt",
                table: "journal_entries",
                newName: "posted_at");

            migrationBuilder.RenameColumn(
                name: "PeriodId",
                table: "journal_entries",
                newName: "period_id");

            migrationBuilder.RenameColumn(
                name: "FinancialPeriodId",
                table: "journal_entries",
                newName: "financial_period_id");

            migrationBuilder.RenameColumn(
                name: "EntryNumber",
                table: "journal_entries",
                newName: "entry_number");

            migrationBuilder.RenameColumn(
                name: "EntryDate",
                table: "journal_entries",
                newName: "entry_date");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "journal_entries",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "journal_entries",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entries_SourceEventId",
                table: "journal_entries",
                newName: "ix_journal_entries_source_event_id");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entries_PeriodId_Status",
                table: "journal_entries",
                newName: "ix_journal_entries_period_id_status");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entries_FinancialPeriodId",
                table: "journal_entries",
                newName: "ix_journal_entries_financial_period_id");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entries_EntryNumber",
                table: "journal_entries",
                newName: "ix_journal_entries_entry_number");

            migrationBuilder.RenameIndex(
                name: "IX_journal_entries_EntryDate",
                table: "journal_entries",
                newName: "ix_journal_entries_entry_date");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "fiscal_years",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "fiscal_years",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "fiscal_years",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "PeriodStructure",
                table: "fiscal_years",
                newName: "period_structure");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "fiscal_years",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "fiscal_years",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "fiscal_years",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_fiscal_years_Name",
                table: "fiscal_years",
                newName: "ix_fiscal_years_name");

            migrationBuilder.RenameIndex(
                name: "IX_fiscal_years_StartDate_EndDate",
                table: "fiscal_years",
                newName: "ix_fiscal_years_start_date_end_date");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "financial_periods",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "financial_periods",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "financial_periods",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "financial_periods",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "RowVersion",
                table: "financial_periods",
                newName: "row_version");

            migrationBuilder.RenameColumn(
                name: "FiscalYearId",
                table: "financial_periods",
                newName: "fiscal_year_id");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "financial_periods",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "ClosedBy",
                table: "financial_periods",
                newName: "closed_by");

            migrationBuilder.RenameColumn(
                name: "ClosedAt",
                table: "financial_periods",
                newName: "closed_at");

            migrationBuilder.RenameIndex(
                name: "IX_financial_periods_Status",
                table: "financial_periods",
                newName: "ix_financial_periods_status");

            migrationBuilder.RenameIndex(
                name: "IX_financial_periods_StartDate_EndDate",
                table: "financial_periods",
                newName: "ix_financial_periods_start_date_end_date");

            migrationBuilder.RenameIndex(
                name: "IX_financial_periods_FiscalYearId",
                table: "financial_periods",
                newName: "ix_financial_periods_fiscal_year_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "chart_of_accounts",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "chart_of_accounts",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "chart_of_accounts",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "chart_of_accounts",
                newName: "category");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "chart_of_accounts",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ParentAccountId",
                table: "chart_of_accounts",
                newName: "parent_account_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "chart_of_accounts",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "chart_of_accounts",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "chart_of_accounts",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AccountNumber",
                table: "chart_of_accounts",
                newName: "account_number");

            migrationBuilder.RenameIndex(
                name: "IX_chart_of_accounts_Type",
                table: "chart_of_accounts",
                newName: "ix_chart_of_accounts_type");

            migrationBuilder.RenameIndex(
                name: "IX_chart_of_accounts_ParentAccountId",
                table: "chart_of_accounts",
                newName: "ix_chart_of_accounts_parent_account_id");

            migrationBuilder.RenameIndex(
                name: "IX_chart_of_accounts_IsActive",
                table: "chart_of_accounts",
                newName: "ix_chart_of_accounts_is_active");

            migrationBuilder.RenameIndex(
                name: "IX_chart_of_accounts_AccountNumber",
                table: "chart_of_accounts",
                newName: "ix_chart_of_accounts_account_number");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "audit_trail_entries",
                newName: "reason");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "audit_trail_entries",
                newName: "action");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "audit_trail_entries",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PerformedBy",
                table: "audit_trail_entries",
                newName: "performed_by");

            migrationBuilder.RenameColumn(
                name: "PerformedAt",
                table: "audit_trail_entries",
                newName: "performed_at");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "audit_trail_entries",
                newName: "ip_address");

            migrationBuilder.RenameColumn(
                name: "EntityType",
                table: "audit_trail_entries",
                newName: "entity_type");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "audit_trail_entries",
                newName: "entity_id");

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                table: "audit_trail_entries",
                newName: "correlation_id");

            migrationBuilder.RenameColumn(
                name: "BeforeSnapshot",
                table: "audit_trail_entries",
                newName: "before_snapshot");

            migrationBuilder.RenameColumn(
                name: "AfterSnapshot",
                table: "audit_trail_entries",
                newName: "after_snapshot");

            migrationBuilder.RenameIndex(
                name: "IX_audit_trail_entries_PerformedBy",
                table: "audit_trail_entries",
                newName: "ix_audit_trail_entries_performed_by");

            migrationBuilder.RenameIndex(
                name: "IX_audit_trail_entries_EntityType_EntityId_PerformedAt",
                table: "audit_trail_entries",
                newName: "ix_audit_trail_entries_entity_type_entity_id_performed_at");

            migrationBuilder.RenameIndex(
                name: "IX_audit_trail_entries_CorrelationId",
                table: "audit_trail_entries",
                newName: "ix_audit_trail_entries_correlation_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "adjusting_entry_approvals",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "adjusting_entry_approvals",
                newName: "reason");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "adjusting_entry_approvals",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RequestedBy",
                table: "adjusting_entry_approvals",
                newName: "requested_by");

            migrationBuilder.RenameColumn(
                name: "RequestedAt",
                table: "adjusting_entry_approvals",
                newName: "requested_at");

            migrationBuilder.RenameColumn(
                name: "JournalEntryId",
                table: "adjusting_entry_approvals",
                newName: "journal_entry_id");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                table: "adjusting_entry_approvals",
                newName: "approved_by");

            migrationBuilder.RenameColumn(
                name: "ApprovedAt",
                table: "adjusting_entry_approvals",
                newName: "approved_at");

            migrationBuilder.RenameColumn(
                name: "ApprovalComments",
                table: "adjusting_entry_approvals",
                newName: "approval_comments");

            migrationBuilder.RenameIndex(
                name: "IX_adjusting_entry_approvals_Status",
                table: "adjusting_entry_approvals",
                newName: "ix_adjusting_entry_approvals_status");

            migrationBuilder.RenameIndex(
                name: "IX_adjusting_entry_approvals_JournalEntryId",
                table: "adjusting_entry_approvals",
                newName: "ix_adjusting_entry_approvals_journal_entry_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_tax_components",
                table: "tax_components",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_subledger_transactions",
                table: "subledger_transactions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_reconciliation_reports",
                table: "reconciliation_reports",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_processed_event_registry",
                table: "processed_event_registry",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_journal_entry_lines",
                table: "journal_entry_lines",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_journal_entries",
                table: "journal_entries",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_fiscal_years",
                table: "fiscal_years",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_financial_periods",
                table: "financial_periods",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_chart_of_accounts",
                table: "chart_of_accounts",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_audit_trail_entries",
                table: "audit_trail_entries",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_adjusting_entry_approvals",
                table: "adjusting_entry_approvals",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_adjusting_entry_approvals_journal_entries_journal_entry_id",
                table: "adjusting_entry_approvals",
                column: "journal_entry_id",
                principalTable: "journal_entries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_chart_of_accounts_chart_of_accounts_parent_account_id",
                table: "chart_of_accounts",
                column: "parent_account_id",
                principalTable: "chart_of_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_financial_periods_fiscal_years_fiscal_year_id",
                table: "financial_periods",
                column: "fiscal_year_id",
                principalTable: "fiscal_years",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_journal_entries_financial_periods_financial_period_id",
                table: "journal_entries",
                column: "financial_period_id",
                principalTable: "financial_periods",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_journal_entries_financial_periods_period_id",
                table: "journal_entries",
                column: "period_id",
                principalTable: "financial_periods",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_journal_entry_lines_chart_of_accounts_account_id",
                table: "journal_entry_lines",
                column: "account_id",
                principalTable: "chart_of_accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_journal_entry_lines_journal_entries_journal_entry_id",
                table: "journal_entry_lines",
                column: "journal_entry_id",
                principalTable: "journal_entries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_reconciliation_reports_financial_periods_financial_period_id",
                table: "reconciliation_reports",
                column: "financial_period_id",
                principalTable: "financial_periods",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_reconciliation_reports_financial_periods_period_id",
                table: "reconciliation_reports",
                column: "period_id",
                principalTable: "financial_periods",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_subledger_transactions_journal_entries_journal_entry_id",
                table: "subledger_transactions",
                column: "journal_entry_id",
                principalTable: "journal_entries",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_tax_components_journal_entry_lines_journal_entry_line_id",
                table: "tax_components",
                column: "journal_entry_line_id",
                principalTable: "journal_entry_lines",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_adjusting_entry_approvals_journal_entries_journal_entry_id",
                table: "adjusting_entry_approvals");

            migrationBuilder.DropForeignKey(
                name: "fk_chart_of_accounts_chart_of_accounts_parent_account_id",
                table: "chart_of_accounts");

            migrationBuilder.DropForeignKey(
                name: "fk_financial_periods_fiscal_years_fiscal_year_id",
                table: "financial_periods");

            migrationBuilder.DropForeignKey(
                name: "fk_journal_entries_financial_periods_financial_period_id",
                table: "journal_entries");

            migrationBuilder.DropForeignKey(
                name: "fk_journal_entries_financial_periods_period_id",
                table: "journal_entries");

            migrationBuilder.DropForeignKey(
                name: "fk_journal_entry_lines_chart_of_accounts_account_id",
                table: "journal_entry_lines");

            migrationBuilder.DropForeignKey(
                name: "fk_journal_entry_lines_journal_entries_journal_entry_id",
                table: "journal_entry_lines");

            migrationBuilder.DropForeignKey(
                name: "fk_reconciliation_reports_financial_periods_financial_period_id",
                table: "reconciliation_reports");

            migrationBuilder.DropForeignKey(
                name: "fk_reconciliation_reports_financial_periods_period_id",
                table: "reconciliation_reports");

            migrationBuilder.DropForeignKey(
                name: "fk_subledger_transactions_journal_entries_journal_entry_id",
                table: "subledger_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_tax_components_journal_entry_lines_journal_entry_line_id",
                table: "tax_components");

            migrationBuilder.DropPrimaryKey(
                name: "pk_tax_components",
                table: "tax_components");

            migrationBuilder.DropPrimaryKey(
                name: "pk_subledger_transactions",
                table: "subledger_transactions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_reconciliation_reports",
                table: "reconciliation_reports");

            migrationBuilder.DropPrimaryKey(
                name: "pk_processed_event_registry",
                table: "processed_event_registry");

            migrationBuilder.DropPrimaryKey(
                name: "pk_journal_entry_lines",
                table: "journal_entry_lines");

            migrationBuilder.DropPrimaryKey(
                name: "pk_journal_entries",
                table: "journal_entries");

            migrationBuilder.DropPrimaryKey(
                name: "pk_fiscal_years",
                table: "fiscal_years");

            migrationBuilder.DropPrimaryKey(
                name: "pk_financial_periods",
                table: "financial_periods");

            migrationBuilder.DropPrimaryKey(
                name: "pk_chart_of_accounts",
                table: "chart_of_accounts");

            migrationBuilder.DropPrimaryKey(
                name: "pk_audit_trail_entries",
                table: "audit_trail_entries");

            migrationBuilder.DropPrimaryKey(
                name: "pk_adjusting_entry_approvals",
                table: "adjusting_entry_approvals");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "tax_components",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "taxable_amount",
                table: "tax_components",
                newName: "TaxableAmount");

            migrationBuilder.RenameColumn(
                name: "tax_type",
                table: "tax_components",
                newName: "TaxType");

            migrationBuilder.RenameColumn(
                name: "tax_rate",
                table: "tax_components",
                newName: "TaxRate");

            migrationBuilder.RenameColumn(
                name: "tax_jurisdiction",
                table: "tax_components",
                newName: "TaxJurisdiction");

            migrationBuilder.RenameColumn(
                name: "tax_amount",
                table: "tax_components",
                newName: "TaxAmount");

            migrationBuilder.RenameColumn(
                name: "journal_entry_line_id",
                table: "tax_components",
                newName: "JournalEntryLineId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "tax_components",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_tax_components_tax_type",
                table: "tax_components",
                newName: "IX_tax_components_TaxType");

            migrationBuilder.RenameIndex(
                name: "ix_tax_components_journal_entry_line_id",
                table: "tax_components",
                newName: "IX_tax_components_JournalEntryLineId");

            migrationBuilder.RenameColumn(
                name: "metadata",
                table: "subledger_transactions",
                newName: "Metadata");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "subledger_transactions",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "subledger_transactions",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "subledger_transactions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "transaction_type",
                table: "subledger_transactions",
                newName: "TransactionType");

            migrationBuilder.RenameColumn(
                name: "transaction_date",
                table: "subledger_transactions",
                newName: "TransactionDate");

            migrationBuilder.RenameColumn(
                name: "supplier_id",
                table: "subledger_transactions",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "source_transaction_id",
                table: "subledger_transactions",
                newName: "SourceTransactionId");

            migrationBuilder.RenameColumn(
                name: "source_system",
                table: "subledger_transactions",
                newName: "SourceSystem");

            migrationBuilder.RenameColumn(
                name: "journal_entry_id",
                table: "subledger_transactions",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "subledger_transactions",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "subledger_transactions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_subledger_transactions_transaction_date",
                table: "subledger_transactions",
                newName: "IX_subledger_transactions_TransactionDate");

            migrationBuilder.RenameIndex(
                name: "ix_subledger_transactions_source_system_source_transaction_id",
                table: "subledger_transactions",
                newName: "IX_subledger_transactions_SourceSystem_SourceTransactionId");

            migrationBuilder.RenameIndex(
                name: "ix_subledger_transactions_journal_entry_id",
                table: "subledger_transactions",
                newName: "IX_subledger_transactions_JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "variance",
                table: "reconciliation_reports",
                newName: "Variance");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "reconciliation_reports",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "reconciliation_reports",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "subledger_total",
                table: "reconciliation_reports",
                newName: "SubledgerTotal");

            migrationBuilder.RenameColumn(
                name: "run_by",
                table: "reconciliation_reports",
                newName: "RunBy");

            migrationBuilder.RenameColumn(
                name: "run_at",
                table: "reconciliation_reports",
                newName: "RunAt");

            migrationBuilder.RenameColumn(
                name: "resolved_by",
                table: "reconciliation_reports",
                newName: "ResolvedBy");

            migrationBuilder.RenameColumn(
                name: "resolved_at",
                table: "reconciliation_reports",
                newName: "ResolvedAt");

            migrationBuilder.RenameColumn(
                name: "resolution_notes",
                table: "reconciliation_reports",
                newName: "ResolutionNotes");

            migrationBuilder.RenameColumn(
                name: "reconciliation_type",
                table: "reconciliation_reports",
                newName: "ReconciliationType");

            migrationBuilder.RenameColumn(
                name: "period_id",
                table: "reconciliation_reports",
                newName: "PeriodId");

            migrationBuilder.RenameColumn(
                name: "general_ledger_total",
                table: "reconciliation_reports",
                newName: "GeneralLedgerTotal");

            migrationBuilder.RenameColumn(
                name: "financial_period_id",
                table: "reconciliation_reports",
                newName: "FinancialPeriodId");

            migrationBuilder.RenameColumn(
                name: "discrepancy_details",
                table: "reconciliation_reports",
                newName: "DiscrepancyDetails");

            migrationBuilder.RenameIndex(
                name: "ix_reconciliation_reports_status",
                table: "reconciliation_reports",
                newName: "IX_reconciliation_reports_Status");

            migrationBuilder.RenameIndex(
                name: "ix_reconciliation_reports_period_id_reconciliation_type",
                table: "reconciliation_reports",
                newName: "IX_reconciliation_reports_PeriodId_ReconciliationType");

            migrationBuilder.RenameIndex(
                name: "ix_reconciliation_reports_financial_period_id",
                table: "reconciliation_reports",
                newName: "IX_reconciliation_reports_FinancialPeriodId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "processed_event_registry",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "processed_at",
                table: "processed_event_registry",
                newName: "ProcessedAt");

            migrationBuilder.RenameColumn(
                name: "journal_entry_id",
                table: "processed_event_registry",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "event_type",
                table: "processed_event_registry",
                newName: "EventType");

            migrationBuilder.RenameColumn(
                name: "event_id",
                table: "processed_event_registry",
                newName: "EventId");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                table: "processed_event_registry",
                newName: "CorrelationId");

            migrationBuilder.RenameIndex(
                name: "ix_processed_event_registry_processed_at",
                table: "processed_event_registry",
                newName: "IX_processed_event_registry_ProcessedAt");

            migrationBuilder.RenameIndex(
                name: "ix_processed_event_registry_event_id",
                table: "processed_event_registry",
                newName: "IX_processed_event_registry_EventId");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "journal_entry_lines",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "journal_entry_lines",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "supplier_id",
                table: "journal_entry_lines",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "reference_type",
                table: "journal_entry_lines",
                newName: "ReferenceType");

            migrationBuilder.RenameColumn(
                name: "reference_id",
                table: "journal_entry_lines",
                newName: "ReferenceId");

            migrationBuilder.RenameColumn(
                name: "line_sequence",
                table: "journal_entry_lines",
                newName: "LineSequence");

            migrationBuilder.RenameColumn(
                name: "journal_entry_id",
                table: "journal_entry_lines",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "debit_amount",
                table: "journal_entry_lines",
                newName: "DebitAmount");

            migrationBuilder.RenameColumn(
                name: "customer_id",
                table: "journal_entry_lines",
                newName: "CustomerId");

            migrationBuilder.RenameColumn(
                name: "credit_amount",
                table: "journal_entry_lines",
                newName: "CreditAmount");

            migrationBuilder.RenameColumn(
                name: "account_id",
                table: "journal_entry_lines",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entry_lines_supplier_id",
                table: "journal_entry_lines",
                newName: "IX_journal_entry_lines_SupplierId");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entry_lines_journal_entry_id",
                table: "journal_entry_lines",
                newName: "IX_journal_entry_lines_JournalEntryId");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entry_lines_customer_id",
                table: "journal_entry_lines",
                newName: "IX_journal_entry_lines_CustomerId");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entry_lines_account_id",
                table: "journal_entry_lines",
                newName: "IX_journal_entry_lines_AccountId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "journal_entries",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "journal_entries",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "journal_entries",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "total_debit",
                table: "journal_entries",
                newName: "TotalDebit");

            migrationBuilder.RenameColumn(
                name: "total_credit",
                table: "journal_entries",
                newName: "TotalCredit");

            migrationBuilder.RenameColumn(
                name: "source_system",
                table: "journal_entries",
                newName: "SourceSystem");

            migrationBuilder.RenameColumn(
                name: "source_event_id",
                table: "journal_entries",
                newName: "SourceEventId");

            migrationBuilder.RenameColumn(
                name: "row_version",
                table: "journal_entries",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "posted_by",
                table: "journal_entries",
                newName: "PostedBy");

            migrationBuilder.RenameColumn(
                name: "posted_at",
                table: "journal_entries",
                newName: "PostedAt");

            migrationBuilder.RenameColumn(
                name: "period_id",
                table: "journal_entries",
                newName: "PeriodId");

            migrationBuilder.RenameColumn(
                name: "financial_period_id",
                table: "journal_entries",
                newName: "FinancialPeriodId");

            migrationBuilder.RenameColumn(
                name: "entry_number",
                table: "journal_entries",
                newName: "EntryNumber");

            migrationBuilder.RenameColumn(
                name: "entry_date",
                table: "journal_entries",
                newName: "EntryDate");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "journal_entries",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "journal_entries",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entries_source_event_id",
                table: "journal_entries",
                newName: "IX_journal_entries_SourceEventId");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entries_period_id_status",
                table: "journal_entries",
                newName: "IX_journal_entries_PeriodId_Status");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entries_financial_period_id",
                table: "journal_entries",
                newName: "IX_journal_entries_FinancialPeriodId");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entries_entry_number",
                table: "journal_entries",
                newName: "IX_journal_entries_EntryNumber");

            migrationBuilder.RenameIndex(
                name: "ix_journal_entries_entry_date",
                table: "journal_entries",
                newName: "IX_journal_entries_EntryDate");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "fiscal_years",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "fiscal_years",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "fiscal_years",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "period_structure",
                table: "fiscal_years",
                newName: "PeriodStructure");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "fiscal_years",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "end_date",
                table: "fiscal_years",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "fiscal_years",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_fiscal_years_name",
                table: "fiscal_years",
                newName: "IX_fiscal_years_Name");

            migrationBuilder.RenameIndex(
                name: "ix_fiscal_years_start_date_end_date",
                table: "fiscal_years",
                newName: "IX_fiscal_years_StartDate_EndDate");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "financial_periods",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "financial_periods",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "financial_periods",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "financial_periods",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "row_version",
                table: "financial_periods",
                newName: "RowVersion");

            migrationBuilder.RenameColumn(
                name: "fiscal_year_id",
                table: "financial_periods",
                newName: "FiscalYearId");

            migrationBuilder.RenameColumn(
                name: "end_date",
                table: "financial_periods",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "closed_by",
                table: "financial_periods",
                newName: "ClosedBy");

            migrationBuilder.RenameColumn(
                name: "closed_at",
                table: "financial_periods",
                newName: "ClosedAt");

            migrationBuilder.RenameIndex(
                name: "ix_financial_periods_status",
                table: "financial_periods",
                newName: "IX_financial_periods_Status");

            migrationBuilder.RenameIndex(
                name: "ix_financial_periods_start_date_end_date",
                table: "financial_periods",
                newName: "IX_financial_periods_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "ix_financial_periods_fiscal_year_id",
                table: "financial_periods",
                newName: "IX_financial_periods_FiscalYearId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "chart_of_accounts",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "chart_of_accounts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "chart_of_accounts",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "category",
                table: "chart_of_accounts",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "chart_of_accounts",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "parent_account_id",
                table: "chart_of_accounts",
                newName: "ParentAccountId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "chart_of_accounts",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "chart_of_accounts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "chart_of_accounts",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "account_number",
                table: "chart_of_accounts",
                newName: "AccountNumber");

            migrationBuilder.RenameIndex(
                name: "ix_chart_of_accounts_type",
                table: "chart_of_accounts",
                newName: "IX_chart_of_accounts_Type");

            migrationBuilder.RenameIndex(
                name: "ix_chart_of_accounts_parent_account_id",
                table: "chart_of_accounts",
                newName: "IX_chart_of_accounts_ParentAccountId");

            migrationBuilder.RenameIndex(
                name: "ix_chart_of_accounts_is_active",
                table: "chart_of_accounts",
                newName: "IX_chart_of_accounts_IsActive");

            migrationBuilder.RenameIndex(
                name: "ix_chart_of_accounts_account_number",
                table: "chart_of_accounts",
                newName: "IX_chart_of_accounts_AccountNumber");

            migrationBuilder.RenameColumn(
                name: "reason",
                table: "audit_trail_entries",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "action",
                table: "audit_trail_entries",
                newName: "Action");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "audit_trail_entries",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "performed_by",
                table: "audit_trail_entries",
                newName: "PerformedBy");

            migrationBuilder.RenameColumn(
                name: "performed_at",
                table: "audit_trail_entries",
                newName: "PerformedAt");

            migrationBuilder.RenameColumn(
                name: "ip_address",
                table: "audit_trail_entries",
                newName: "IpAddress");

            migrationBuilder.RenameColumn(
                name: "entity_type",
                table: "audit_trail_entries",
                newName: "EntityType");

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "audit_trail_entries",
                newName: "EntityId");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                table: "audit_trail_entries",
                newName: "CorrelationId");

            migrationBuilder.RenameColumn(
                name: "before_snapshot",
                table: "audit_trail_entries",
                newName: "BeforeSnapshot");

            migrationBuilder.RenameColumn(
                name: "after_snapshot",
                table: "audit_trail_entries",
                newName: "AfterSnapshot");

            migrationBuilder.RenameIndex(
                name: "ix_audit_trail_entries_performed_by",
                table: "audit_trail_entries",
                newName: "IX_audit_trail_entries_PerformedBy");

            migrationBuilder.RenameIndex(
                name: "ix_audit_trail_entries_entity_type_entity_id_performed_at",
                table: "audit_trail_entries",
                newName: "IX_audit_trail_entries_EntityType_EntityId_PerformedAt");

            migrationBuilder.RenameIndex(
                name: "ix_audit_trail_entries_correlation_id",
                table: "audit_trail_entries",
                newName: "IX_audit_trail_entries_CorrelationId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "adjusting_entry_approvals",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "reason",
                table: "adjusting_entry_approvals",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "adjusting_entry_approvals",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "requested_by",
                table: "adjusting_entry_approvals",
                newName: "RequestedBy");

            migrationBuilder.RenameColumn(
                name: "requested_at",
                table: "adjusting_entry_approvals",
                newName: "RequestedAt");

            migrationBuilder.RenameColumn(
                name: "journal_entry_id",
                table: "adjusting_entry_approvals",
                newName: "JournalEntryId");

            migrationBuilder.RenameColumn(
                name: "approved_by",
                table: "adjusting_entry_approvals",
                newName: "ApprovedBy");

            migrationBuilder.RenameColumn(
                name: "approved_at",
                table: "adjusting_entry_approvals",
                newName: "ApprovedAt");

            migrationBuilder.RenameColumn(
                name: "approval_comments",
                table: "adjusting_entry_approvals",
                newName: "ApprovalComments");

            migrationBuilder.RenameIndex(
                name: "ix_adjusting_entry_approvals_status",
                table: "adjusting_entry_approvals",
                newName: "IX_adjusting_entry_approvals_Status");

            migrationBuilder.RenameIndex(
                name: "ix_adjusting_entry_approvals_journal_entry_id",
                table: "adjusting_entry_approvals",
                newName: "IX_adjusting_entry_approvals_JournalEntryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tax_components",
                table: "tax_components",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_subledger_transactions",
                table: "subledger_transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reconciliation_reports",
                table: "reconciliation_reports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_processed_event_registry",
                table: "processed_event_registry",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journal_entry_lines",
                table: "journal_entry_lines",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journal_entries",
                table: "journal_entries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_fiscal_years",
                table: "fiscal_years",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_financial_periods",
                table: "financial_periods",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_chart_of_accounts",
                table: "chart_of_accounts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_trail_entries",
                table: "audit_trail_entries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_adjusting_entry_approvals",
                table: "adjusting_entry_approvals",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_adjusting_entry_approvals_journal_entries_JournalEntryId",
                table: "adjusting_entry_approvals",
                column: "JournalEntryId",
                principalTable: "journal_entries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_chart_of_accounts_chart_of_accounts_ParentAccountId",
                table: "chart_of_accounts",
                column: "ParentAccountId",
                principalTable: "chart_of_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_financial_periods_fiscal_years_FiscalYearId",
                table: "financial_periods",
                column: "FiscalYearId",
                principalTable: "fiscal_years",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_financial_periods_FinancialPeriodId",
                table: "journal_entries",
                column: "FinancialPeriodId",
                principalTable: "financial_periods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entries_financial_periods_PeriodId",
                table: "journal_entries",
                column: "PeriodId",
                principalTable: "financial_periods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entry_lines_chart_of_accounts_AccountId",
                table: "journal_entry_lines",
                column: "AccountId",
                principalTable: "chart_of_accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_entry_lines_journal_entries_JournalEntryId",
                table: "journal_entry_lines",
                column: "JournalEntryId",
                principalTable: "journal_entries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_reconciliation_reports_financial_periods_FinancialPeriodId",
                table: "reconciliation_reports",
                column: "FinancialPeriodId",
                principalTable: "financial_periods",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reconciliation_reports_financial_periods_PeriodId",
                table: "reconciliation_reports",
                column: "PeriodId",
                principalTable: "financial_periods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_subledger_transactions_journal_entries_JournalEntryId",
                table: "subledger_transactions",
                column: "JournalEntryId",
                principalTable: "journal_entries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tax_components_journal_entry_lines_JournalEntryLineId",
                table: "tax_components",
                column: "JournalEntryLineId",
                principalTable: "journal_entry_lines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
