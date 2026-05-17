using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.AccountingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalEntryCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "transaction_credit_amount",
                table: "journal_entry_lines",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "transaction_debit_amount",
                table: "journal_entry_lines",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "currency_code",
                table: "journal_entries",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "THB");

            migrationBuilder.AddColumn<decimal>(
                name: "exchange_rate_to_base",
                table: "journal_entries",
                type: "numeric(18,8)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<decimal>(
                name: "transaction_total_credit",
                table: "journal_entries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "transaction_total_debit",
                table: "journal_entries",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(
                """
                UPDATE journal_entry_lines
                SET transaction_debit_amount = debit_amount,
                    transaction_credit_amount = credit_amount;

                UPDATE journal_entries
                SET transaction_total_debit = total_debit,
                    transaction_total_credit = total_credit,
                    currency_code = 'THB',
                    exchange_rate_to_base = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transaction_credit_amount",
                table: "journal_entry_lines");

            migrationBuilder.DropColumn(
                name: "transaction_debit_amount",
                table: "journal_entry_lines");

            migrationBuilder.DropColumn(
                name: "currency_code",
                table: "journal_entries");

            migrationBuilder.DropColumn(
                name: "exchange_rate_to_base",
                table: "journal_entries");

            migrationBuilder.DropColumn(
                name: "transaction_total_credit",
                table: "journal_entries");

            migrationBuilder.DropColumn(
                name: "transaction_total_debit",
                table: "journal_entries");
        }
    }
}
