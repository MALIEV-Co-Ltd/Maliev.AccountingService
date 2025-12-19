using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.AccountingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChartOfAccountConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add check constraint for valid AccountType enum values (0-4)
            migrationBuilder.Sql(@"
                ALTER TABLE chart_of_accounts
                ADD CONSTRAINT CK_chart_of_accounts_Type
                CHECK (""Type"" >= 0 AND ""Type"" <= 4);
            ");

            // Add check constraint for account number format (minimum 4 characters, alphanumeric with dashes)
            migrationBuilder.Sql(@"
                ALTER TABLE chart_of_accounts
                ADD CONSTRAINT CK_chart_of_accounts_AccountNumber_Format
                CHECK (LENGTH(""AccountNumber"") >= 4 AND ""AccountNumber"" ~ '^[A-Z0-9][A-Z0-9-]*$');
            ");

            // Add check constraint to ensure account name is not empty
            migrationBuilder.Sql(@"
                ALTER TABLE chart_of_accounts
                ADD CONSTRAINT CK_chart_of_accounts_Name_NotEmpty
                CHECK (LENGTH(TRIM(""Name"")) > 0);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE chart_of_accounts DROP CONSTRAINT IF EXISTS CK_chart_of_accounts_Type;");
            migrationBuilder.Sql("ALTER TABLE chart_of_accounts DROP CONSTRAINT IF EXISTS CK_chart_of_accounts_AccountNumber_Format;");
            migrationBuilder.Sql("ALTER TABLE chart_of_accounts DROP CONSTRAINT IF EXISTS CK_chart_of_accounts_Name_NotEmpty;");
        }
    }
}
