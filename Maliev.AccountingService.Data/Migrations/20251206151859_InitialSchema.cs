using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.AccountingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_trail_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PerformedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BeforeSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    AfterSnapshot = table.Column<string>(type: "jsonb", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_trail_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chart_of_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParentAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chart_of_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chart_of_accounts_chart_of_accounts_ParentAccountId",
                        column: x => x.ParentAccountId,
                        principalTable: "chart_of_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiscal_years",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodStructure = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal_years", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "processed_event_registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_event_registry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "financial_periods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FiscalYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_periods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_financial_periods_fiscal_years_FiscalYearId",
                        column: x => x.FiscalYearId,
                        principalTable: "fiscal_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SourceEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalDebit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    FinancialPeriodId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_journal_entries_financial_periods_FinancialPeriodId",
                        column: x => x.FinancialPeriodId,
                        principalTable: "financial_periods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_journal_entries_financial_periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "financial_periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReconciliationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubledgerTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GeneralLedgerTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Variance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DiscrepancyDetails = table.Column<string>(type: "jsonb", nullable: true),
                    RunBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FinancialPeriodId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reconciliation_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reconciliation_reports_financial_periods_FinancialPeriodId",
                        column: x => x.FinancialPeriodId,
                        principalTable: "financial_periods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_reconciliation_reports_financial_periods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "financial_periods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "adjusting_entry_approvals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApprovalComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adjusting_entry_approvals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_adjusting_entry_approvals_journal_entries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "journal_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journal_entry_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineSequence = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DebitAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal_entry_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_journal_entry_lines_chart_of_accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "chart_of_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_journal_entry_lines_journal_entries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "journal_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subledger_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceSystem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subledger_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_subledger_transactions_journal_entries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "journal_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tax_components",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalEntryLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxJurisdiction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_components", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tax_components_journal_entry_lines_JournalEntryLineId",
                        column: x => x.JournalEntryLineId,
                        principalTable: "journal_entry_lines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_adjusting_entry_approvals_JournalEntryId",
                table: "adjusting_entry_approvals",
                column: "JournalEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_adjusting_entry_approvals_Status",
                table: "adjusting_entry_approvals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_audit_trail_entries_CorrelationId",
                table: "audit_trail_entries",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_trail_entries_EntityType_EntityId_PerformedAt",
                table: "audit_trail_entries",
                columns: new[] { "EntityType", "EntityId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_trail_entries_PerformedBy",
                table: "audit_trail_entries",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_chart_of_accounts_AccountNumber",
                table: "chart_of_accounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chart_of_accounts_IsActive",
                table: "chart_of_accounts",
                column: "IsActive",
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_chart_of_accounts_ParentAccountId",
                table: "chart_of_accounts",
                column: "ParentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_chart_of_accounts_Type",
                table: "chart_of_accounts",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_financial_periods_FiscalYearId",
                table: "financial_periods",
                column: "FiscalYearId");

            migrationBuilder.CreateIndex(
                name: "IX_financial_periods_StartDate_EndDate",
                table: "financial_periods",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_financial_periods_Status",
                table: "financial_periods",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_years_Name",
                table: "fiscal_years",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_years_StartDate_EndDate",
                table: "fiscal_years",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_EntryDate",
                table: "journal_entries",
                column: "EntryDate");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_EntryNumber",
                table: "journal_entries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_FinancialPeriodId",
                table: "journal_entries",
                column: "FinancialPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_PeriodId_Status",
                table: "journal_entries",
                columns: new[] { "PeriodId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_journal_entries_SourceEventId",
                table: "journal_entries",
                column: "SourceEventId");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_AccountId",
                table: "journal_entry_lines",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_CustomerId",
                table: "journal_entry_lines",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_JournalEntryId",
                table: "journal_entry_lines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_journal_entry_lines_SupplierId",
                table: "journal_entry_lines",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_processed_event_registry_EventId",
                table: "processed_event_registry",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processed_event_registry_ProcessedAt",
                table: "processed_event_registry",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_reports_FinancialPeriodId",
                table: "reconciliation_reports",
                column: "FinancialPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_reports_PeriodId_ReconciliationType",
                table: "reconciliation_reports",
                columns: new[] { "PeriodId", "ReconciliationType" });

            migrationBuilder.CreateIndex(
                name: "IX_reconciliation_reports_Status",
                table: "reconciliation_reports",
                column: "Status",
                filter: "\"Status\" != 'Resolved'");

            migrationBuilder.CreateIndex(
                name: "IX_subledger_transactions_JournalEntryId",
                table: "subledger_transactions",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_subledger_transactions_SourceSystem_SourceTransactionId",
                table: "subledger_transactions",
                columns: new[] { "SourceSystem", "SourceTransactionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subledger_transactions_TransactionDate",
                table: "subledger_transactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_tax_components_JournalEntryLineId",
                table: "tax_components",
                column: "JournalEntryLineId");

            migrationBuilder.CreateIndex(
                name: "IX_tax_components_TaxType",
                table: "tax_components",
                column: "TaxType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adjusting_entry_approvals");

            migrationBuilder.DropTable(
                name: "audit_trail_entries");

            migrationBuilder.DropTable(
                name: "processed_event_registry");

            migrationBuilder.DropTable(
                name: "reconciliation_reports");

            migrationBuilder.DropTable(
                name: "subledger_transactions");

            migrationBuilder.DropTable(
                name: "tax_components");

            migrationBuilder.DropTable(
                name: "journal_entry_lines");

            migrationBuilder.DropTable(
                name: "chart_of_accounts");

            migrationBuilder.DropTable(
                name: "journal_entries");

            migrationBuilder.DropTable(
                name: "financial_periods");

            migrationBuilder.DropTable(
                name: "fiscal_years");
        }
    }
}
