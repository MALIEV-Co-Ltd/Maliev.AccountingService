using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Maliev.AccountingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "journal_entry_number_seq");

            migrationBuilder.CreateTable(
                name: "audit_trail_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    performed_by = table.Column<Guid>(type: "uuid", nullable: false),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    before_snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    after_snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_trail_entries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "chart_of_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    parent_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_chart_of_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_chart_of_accounts_chart_of_accounts_parent_account_id",
                        column: x => x.parent_account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fiscal_years",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period_structure = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fiscal_years", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_critical = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "processed_event_registry",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    journal_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processed_event_registry", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.name);
                });

            migrationBuilder.CreateTable(
                name: "financial_periods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fiscal_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_financial_periods", x => x.id);
                    table.ForeignKey(
                        name: "fk_financial_periods_fiscal_years_fiscal_year_id",
                        column: x => x.fiscal_year_id,
                        principalTable: "fiscal_years",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    role_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    permission_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_name, x.permission_code });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_code",
                        column: x => x.permission_code,
                        principalTable: "permissions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_name",
                        column: x => x.role_name,
                        principalTable: "roles",
                        principalColumn: "name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journal_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    entry_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    source_system = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    source_event_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    total_debit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_credit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    posted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    posted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    financial_period_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_journal_entries_financial_periods_financial_period_id",
                        column: x => x.financial_period_id,
                        principalTable: "financial_periods",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_journal_entries_financial_periods_period_id",
                        column: x => x.period_id,
                        principalTable: "financial_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reconciliation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    subledger_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    general_ledger_total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    variance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    discrepancy_details = table.Column<string>(type: "jsonb", nullable: true),
                    run_by = table.Column<Guid>(type: "uuid", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolution_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    financial_period_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reconciliation_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_reconciliation_reports_financial_periods_financial_period_id",
                        column: x => x.financial_period_id,
                        principalTable: "financial_periods",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_reconciliation_reports_financial_periods_period_id",
                        column: x => x.period_id,
                        principalTable: "financial_periods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "adjusting_entry_approvals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    journal_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    approval_comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adjusting_entry_approvals", x => x.id);
                    table.ForeignKey(
                        name: "fk_adjusting_entry_approvals_journal_entries_journal_entry_id",
                        column: x => x.journal_entry_id,
                        principalTable: "journal_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journal_entry_lines",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    journal_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    line_sequence = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    debit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    credit_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    reference_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    reference_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_entry_lines", x => x.id);
                    table.ForeignKey(
                        name: "fk_journal_entry_lines_chart_of_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "chart_of_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_journal_entry_lines_journal_entries_journal_entry_id",
                        column: x => x.journal_entry_id,
                        principalTable: "journal_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subledger_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_system = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    source_transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    supplier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: true),
                    journal_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subledger_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_subledger_transactions_journal_entries_journal_entry_id",
                        column: x => x.journal_entry_id,
                        principalTable: "journal_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tax_components",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    journal_entry_line_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tax_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tax_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    taxable_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    tax_jurisdiction = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tax_components", x => x.id);
                    table.ForeignKey(
                        name: "fk_tax_components_journal_entry_lines_journal_entry_line_id",
                        column: x => x.journal_entry_line_id,
                        principalTable: "journal_entry_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "code", "description", "is_critical" },
                values: new object[,]
                {
                    { "accounting.accounts.close", "Close accounts", true },
                    { "accounting.accounts.create", "Create chart of accounts", false },
                    { "accounting.accounts.delete", "Deactivate accounts", false },
                    { "accounting.accounts.read", "Read account details", false },
                    { "accounting.accounts.update", "Update accounts", false },
                    { "accounting.journal-entries.create", "Create journal entries", false },
                    { "accounting.journal-entries.post", "Post journal entries", true },
                    { "accounting.journal-entries.read", "Read journal entries", false },
                    { "accounting.journal-entries.reverse", "Reverse journal entries", true },
                    { "accounting.journal-entries.update", "Update journal entries", false },
                    { "accounting.periods.close", "Close accounting periods", true },
                    { "accounting.periods.open", "Open accounting periods", false },
                    { "accounting.periods.reopen", "Reopen closed periods", true },
                    { "accounting.reconciliation.read", "Read reconciliation reports", false },
                    { "accounting.reconciliation.run", "Run financial reconciliation", true },
                    { "accounting.reports.balance-sheet", "View balance sheet", false },
                    { "accounting.reports.cash-flow", "View cash flow statement", false },
                    { "accounting.reports.export", "Export financial reports", false },
                    { "accounting.reports.income-statement", "View income statement", false },
                    { "accounting.reports.trial-balance", "View trial balance", false }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "name", "description" },
                values: new object[,]
                {
                    { "roles.accounting.admin", "Full access to all accounting operations" },
                    { "roles.accounting.clerk", "Basic journal and account data entry" },
                    { "roles.accounting.controller", "Advanced accounting and period management" },
                    { "roles.accounting.manager", "General accounting management access" },
                    { "roles.accounting.viewer", "Read-only access to accounting data and reports" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "accounting.accounts.close", "roles.accounting.admin" },
                    { "accounting.accounts.create", "roles.accounting.admin" },
                    { "accounting.accounts.delete", "roles.accounting.admin" },
                    { "accounting.accounts.read", "roles.accounting.admin" },
                    { "accounting.accounts.update", "roles.accounting.admin" },
                    { "accounting.journal-entries.create", "roles.accounting.admin" },
                    { "accounting.journal-entries.post", "roles.accounting.admin" },
                    { "accounting.journal-entries.read", "roles.accounting.admin" },
                    { "accounting.journal-entries.reverse", "roles.accounting.admin" },
                    { "accounting.journal-entries.update", "roles.accounting.admin" },
                    { "accounting.periods.close", "roles.accounting.admin" },
                    { "accounting.periods.open", "roles.accounting.admin" },
                    { "accounting.periods.reopen", "roles.accounting.admin" },
                    { "accounting.reconciliation.read", "roles.accounting.admin" },
                    { "accounting.reconciliation.run", "roles.accounting.admin" },
                    { "accounting.reports.balance-sheet", "roles.accounting.admin" },
                    { "accounting.reports.cash-flow", "roles.accounting.admin" },
                    { "accounting.reports.export", "roles.accounting.admin" },
                    { "accounting.reports.income-statement", "roles.accounting.admin" },
                    { "accounting.reports.trial-balance", "roles.accounting.admin" },
                    { "accounting.accounts.read", "roles.accounting.clerk" },
                    { "accounting.journal-entries.create", "roles.accounting.clerk" },
                    { "accounting.journal-entries.read", "roles.accounting.clerk" },
                    { "accounting.reports.balance-sheet", "roles.accounting.clerk" },
                    { "accounting.reports.cash-flow", "roles.accounting.clerk" },
                    { "accounting.reports.export", "roles.accounting.clerk" },
                    { "accounting.reports.income-statement", "roles.accounting.clerk" },
                    { "accounting.reports.trial-balance", "roles.accounting.clerk" },
                    { "accounting.accounts.close", "roles.accounting.controller" },
                    { "accounting.accounts.create", "roles.accounting.controller" },
                    { "accounting.accounts.delete", "roles.accounting.controller" },
                    { "accounting.accounts.read", "roles.accounting.controller" },
                    { "accounting.accounts.update", "roles.accounting.controller" },
                    { "accounting.journal-entries.create", "roles.accounting.controller" },
                    { "accounting.journal-entries.post", "roles.accounting.controller" },
                    { "accounting.journal-entries.read", "roles.accounting.controller" },
                    { "accounting.journal-entries.reverse", "roles.accounting.controller" },
                    { "accounting.journal-entries.update", "roles.accounting.controller" },
                    { "accounting.periods.close", "roles.accounting.controller" },
                    { "accounting.periods.open", "roles.accounting.controller" },
                    { "accounting.periods.reopen", "roles.accounting.controller" },
                    { "accounting.reconciliation.read", "roles.accounting.controller" },
                    { "accounting.reconciliation.run", "roles.accounting.controller" },
                    { "accounting.reports.balance-sheet", "roles.accounting.controller" },
                    { "accounting.reports.cash-flow", "roles.accounting.controller" },
                    { "accounting.reports.export", "roles.accounting.controller" },
                    { "accounting.reports.income-statement", "roles.accounting.controller" },
                    { "accounting.reports.trial-balance", "roles.accounting.controller" },
                    { "accounting.accounts.close", "roles.accounting.manager" },
                    { "accounting.accounts.create", "roles.accounting.manager" },
                    { "accounting.accounts.delete", "roles.accounting.manager" },
                    { "accounting.accounts.read", "roles.accounting.manager" },
                    { "accounting.accounts.update", "roles.accounting.manager" },
                    { "accounting.journal-entries.create", "roles.accounting.manager" },
                    { "accounting.journal-entries.post", "roles.accounting.manager" },
                    { "accounting.journal-entries.read", "roles.accounting.manager" },
                    { "accounting.journal-entries.reverse", "roles.accounting.manager" },
                    { "accounting.journal-entries.update", "roles.accounting.manager" },
                    { "accounting.periods.open", "roles.accounting.manager" },
                    { "accounting.reconciliation.read", "roles.accounting.manager" },
                    { "accounting.reconciliation.run", "roles.accounting.manager" },
                    { "accounting.reports.balance-sheet", "roles.accounting.manager" },
                    { "accounting.reports.cash-flow", "roles.accounting.manager" },
                    { "accounting.reports.export", "roles.accounting.manager" },
                    { "accounting.reports.income-statement", "roles.accounting.manager" },
                    { "accounting.reports.trial-balance", "roles.accounting.manager" },
                    { "accounting.accounts.read", "roles.accounting.viewer" },
                    { "accounting.journal-entries.read", "roles.accounting.viewer" },
                    { "accounting.reconciliation.read", "roles.accounting.viewer" },
                    { "accounting.reports.balance-sheet", "roles.accounting.viewer" },
                    { "accounting.reports.cash-flow", "roles.accounting.viewer" },
                    { "accounting.reports.export", "roles.accounting.viewer" },
                    { "accounting.reports.income-statement", "roles.accounting.viewer" },
                    { "accounting.reports.trial-balance", "roles.accounting.viewer" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_adjusting_entry_approvals_journal_entry_id",
                table: "adjusting_entry_approvals",
                column: "journal_entry_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_adjusting_entry_approvals_status",
                table: "adjusting_entry_approvals",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_audit_trail_entries_correlation_id",
                table: "audit_trail_entries",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_trail_entries_entity_type_entity_id_performed_at",
                table: "audit_trail_entries",
                columns: new[] { "entity_type", "entity_id", "performed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_trail_entries_performed_by",
                table: "audit_trail_entries",
                column: "performed_by");

            migrationBuilder.CreateIndex(
                name: "ix_chart_of_accounts_account_number",
                table: "chart_of_accounts",
                column: "account_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_chart_of_accounts_is_active",
                table: "chart_of_accounts",
                column: "is_active",
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "ix_chart_of_accounts_parent_account_id",
                table: "chart_of_accounts",
                column: "parent_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_chart_of_accounts_type",
                table: "chart_of_accounts",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_financial_periods_fiscal_year_id",
                table: "financial_periods",
                column: "fiscal_year_id");

            migrationBuilder.CreateIndex(
                name: "ix_financial_periods_start_date_end_date",
                table: "financial_periods",
                columns: new[] { "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "ix_financial_periods_status",
                table: "financial_periods",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_years_name",
                table: "fiscal_years",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_years_start_date_end_date",
                table: "fiscal_years",
                columns: new[] { "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_entry_date",
                table: "journal_entries",
                column: "entry_date");

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_entry_number",
                table: "journal_entries",
                column: "entry_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_financial_period_id",
                table: "journal_entries",
                column: "financial_period_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_period_id_status",
                table: "journal_entries",
                columns: new[] { "period_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_journal_entries_source_event_id",
                table: "journal_entries",
                column: "source_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_entry_lines_account_id",
                table: "journal_entry_lines",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_entry_lines_customer_id",
                table: "journal_entry_lines",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_entry_lines_journal_entry_id",
                table: "journal_entry_lines",
                column: "journal_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_entry_lines_supplier_id",
                table: "journal_entry_lines",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "ix_processed_event_registry_event_id",
                table: "processed_event_registry",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_processed_event_registry_processed_at",
                table: "processed_event_registry",
                column: "processed_at");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_reports_financial_period_id",
                table: "reconciliation_reports",
                column: "financial_period_id");

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_reports_period_id_reconciliation_type",
                table: "reconciliation_reports",
                columns: new[] { "period_id", "reconciliation_type" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliation_reports_status",
                table: "reconciliation_reports",
                column: "status",
                filter: "status != 'Resolved'");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_code",
                table: "role_permissions",
                column: "permission_code");

            migrationBuilder.CreateIndex(
                name: "ix_subledger_transactions_journal_entry_id",
                table: "subledger_transactions",
                column: "journal_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_subledger_transactions_source_system_source_transaction_id",
                table: "subledger_transactions",
                columns: new[] { "source_system", "source_transaction_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subledger_transactions_transaction_date",
                table: "subledger_transactions",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "ix_tax_components_journal_entry_line_id",
                table: "tax_components",
                column: "journal_entry_line_id");

            migrationBuilder.CreateIndex(
                name: "ix_tax_components_tax_type",
                table: "tax_components",
                column: "tax_type");
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
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "subledger_transactions");

            migrationBuilder.DropTable(
                name: "tax_components");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

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

            migrationBuilder.DropSequence(
                name: "journal_entry_number_seq");
        }
    }
}
