using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Maliev.AccountingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionsAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    { "accounting-admin", "Full access to all accounting operations" },
                    { "accounting-clerk", "Basic journal and account data entry" },
                    { "accounting-controller", "Advanced accounting and period management" },
                    { "accounting-manager", "General accounting management access" },
                    { "accounting-viewer", "Read-only access to accounting data and reports" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "accounting.accounts.close", "accounting-admin" },
                    { "accounting.accounts.create", "accounting-admin" },
                    { "accounting.accounts.delete", "accounting-admin" },
                    { "accounting.accounts.read", "accounting-admin" },
                    { "accounting.accounts.update", "accounting-admin" },
                    { "accounting.journal-entries.create", "accounting-admin" },
                    { "accounting.journal-entries.post", "accounting-admin" },
                    { "accounting.journal-entries.read", "accounting-admin" },
                    { "accounting.journal-entries.reverse", "accounting-admin" },
                    { "accounting.journal-entries.update", "accounting-admin" },
                    { "accounting.periods.close", "accounting-admin" },
                    { "accounting.periods.open", "accounting-admin" },
                    { "accounting.periods.reopen", "accounting-admin" },
                    { "accounting.reports.balance-sheet", "accounting-admin" },
                    { "accounting.reports.cash-flow", "accounting-admin" },
                    { "accounting.reports.export", "accounting-admin" },
                    { "accounting.reports.income-statement", "accounting-admin" },
                    { "accounting.reports.trial-balance", "accounting-admin" },
                    { "accounting.accounts.read", "accounting-clerk" },
                    { "accounting.journal-entries.create", "accounting-clerk" },
                    { "accounting.journal-entries.read", "accounting-clerk" },
                    { "accounting.reports.balance-sheet", "accounting-clerk" },
                    { "accounting.reports.cash-flow", "accounting-clerk" },
                    { "accounting.reports.export", "accounting-clerk" },
                    { "accounting.reports.income-statement", "accounting-clerk" },
                    { "accounting.reports.trial-balance", "accounting-clerk" },
                    { "accounting.accounts.close", "accounting-controller" },
                    { "accounting.accounts.create", "accounting-controller" },
                    { "accounting.accounts.delete", "accounting-controller" },
                    { "accounting.accounts.read", "accounting-controller" },
                    { "accounting.accounts.update", "accounting-controller" },
                    { "accounting.journal-entries.create", "accounting-controller" },
                    { "accounting.journal-entries.post", "accounting-controller" },
                    { "accounting.journal-entries.read", "accounting-controller" },
                    { "accounting.journal-entries.reverse", "accounting-controller" },
                    { "accounting.journal-entries.update", "accounting-controller" },
                    { "accounting.periods.close", "accounting-controller" },
                    { "accounting.periods.open", "accounting-controller" },
                    { "accounting.periods.reopen", "accounting-controller" },
                    { "accounting.reports.balance-sheet", "accounting-controller" },
                    { "accounting.reports.cash-flow", "accounting-controller" },
                    { "accounting.reports.export", "accounting-controller" },
                    { "accounting.reports.income-statement", "accounting-controller" },
                    { "accounting.reports.trial-balance", "accounting-controller" },
                    { "accounting.accounts.close", "accounting-manager" },
                    { "accounting.accounts.create", "accounting-manager" },
                    { "accounting.accounts.delete", "accounting-manager" },
                    { "accounting.accounts.read", "accounting-manager" },
                    { "accounting.accounts.update", "accounting-manager" },
                    { "accounting.journal-entries.create", "accounting-manager" },
                    { "accounting.journal-entries.post", "accounting-manager" },
                    { "accounting.journal-entries.read", "accounting-manager" },
                    { "accounting.journal-entries.reverse", "accounting-manager" },
                    { "accounting.journal-entries.update", "accounting-manager" },
                    { "accounting.periods.open", "accounting-manager" },
                    { "accounting.reports.balance-sheet", "accounting-manager" },
                    { "accounting.reports.cash-flow", "accounting-manager" },
                    { "accounting.reports.export", "accounting-manager" },
                    { "accounting.reports.income-statement", "accounting-manager" },
                    { "accounting.reports.trial-balance", "accounting-manager" },
                    { "accounting.accounts.read", "accounting-viewer" },
                    { "accounting.journal-entries.read", "accounting-viewer" },
                    { "accounting.reports.balance-sheet", "accounting-viewer" },
                    { "accounting.reports.cash-flow", "accounting-viewer" },
                    { "accounting.reports.export", "accounting-viewer" },
                    { "accounting.reports.income-statement", "accounting-viewer" },
                    { "accounting.reports.trial-balance", "accounting-viewer" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_code",
                table: "role_permissions",
                column: "permission_code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
