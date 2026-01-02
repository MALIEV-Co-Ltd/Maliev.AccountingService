using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Maliev.AccountingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class CompleteFeatureSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "code", "description", "is_critical" },
                values: new object[,]
                {
                    { "accounting.reconciliation.read", "Read reconciliation reports", false },
                    { "accounting.reconciliation.run", "Run financial reconciliation", true }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_code", "role_name" },
                values: new object[,]
                {
                    { "accounting.reconciliation.read", "roles.accounting.admin" },
                    { "accounting.reconciliation.run", "roles.accounting.admin" },
                    { "accounting.reconciliation.read", "roles.accounting.controller" },
                    { "accounting.reconciliation.run", "roles.accounting.controller" },
                    { "accounting.reconciliation.read", "roles.accounting.manager" },
                    { "accounting.reconciliation.run", "roles.accounting.manager" },
                    { "accounting.reconciliation.read", "roles.accounting.viewer" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reconciliation.read", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reconciliation.run", "roles.accounting.admin" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reconciliation.read", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reconciliation.run", "roles.accounting.controller" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reconciliation.read", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reconciliation.run", "roles.accounting.manager" });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_code", "role_name" },
                keyValues: new object[] { "accounting.reconciliation.read", "roles.accounting.viewer" });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "accounting.reconciliation.read");

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "accounting.reconciliation.run");
        }
    }
}
