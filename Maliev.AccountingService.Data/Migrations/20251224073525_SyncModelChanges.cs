using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Maliev.AccountingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.close", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.create", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.delete", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.update", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.create", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.post", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.reverse", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.update", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.close", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.open", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.reopen", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "accounting-admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "accounting-clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.create", "accounting-clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "accounting-clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "accounting-clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "accounting-clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "accounting-clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "accounting-clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "accounting-clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.close", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.create", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.delete", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.update", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.create", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.post", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.reverse", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.update", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.close", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.open", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.reopen", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "accounting-controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.close", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.create", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.delete", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.update", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.create", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.post", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.reverse", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.update", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.open", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "accounting-manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "accounting-viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "accounting-viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "accounting-viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "accounting-viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "accounting-viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "accounting-viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "accounting-viewer" });

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "accounting-admin");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "accounting-clerk");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "accounting-controller");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "accounting-manager");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "accounting-viewer");

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
                    { "accounting.reports.balance-sheet", "roles.accounting.manager" },
                    { "accounting.reports.cash-flow", "roles.accounting.manager" },
                    { "accounting.reports.export", "roles.accounting.manager" },
                    { "accounting.reports.income-statement", "roles.accounting.manager" },
                    { "accounting.reports.trial-balance", "roles.accounting.manager" },
                    { "accounting.accounts.read", "roles.accounting.viewer" },
                    { "accounting.journal-entries.read", "roles.accounting.viewer" },
                    { "accounting.reports.balance-sheet", "roles.accounting.viewer" },
                    { "accounting.reports.cash-flow", "roles.accounting.viewer" },
                    { "accounting.reports.export", "roles.accounting.viewer" },
                    { "accounting.reports.income-statement", "roles.accounting.viewer" },
                    { "accounting.reports.trial-balance", "roles.accounting.viewer" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.close", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.create", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.delete", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.update", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.create", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.post", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.reverse", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.update", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.close", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.open", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.reopen", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "roles.accounting.clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.create", "roles.accounting.clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "roles.accounting.clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "roles.accounting.clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "roles.accounting.clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "roles.accounting.clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "roles.accounting.clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "roles.accounting.clerk" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.close", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.create", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.delete", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.update", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.create", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.post", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.reverse", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.update", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.close", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.open", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.reopen", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.close", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.create", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.delete", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.update", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.create", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.post", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.reverse", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.update", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.periods.open", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.accounts.read", "roles.accounting.viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.journal-entries.read", "roles.accounting.viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.balance-sheet", "roles.accounting.viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.cash-flow", "roles.accounting.viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.export", "roles.accounting.viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.income-statement", "roles.accounting.viewer" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reports.trial-balance", "roles.accounting.viewer" });

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "roles.accounting.admin");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "roles.accounting.clerk");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "roles.accounting.controller");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "roles.accounting.manager");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "name",
                keyValue: "roles.accounting.viewer");

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
        }
    }
}
