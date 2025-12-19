using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maliev.AccountingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBalancedEntryConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add check constraint to ensure journal entries are balanced (total debit = total credit)
            migrationBuilder.Sql(@"
                ALTER TABLE journal_entries
                ADD CONSTRAINT CK_journal_entries_balanced
                CHECK (""TotalDebit"" = ""TotalCredit"");
            ");

            // Add check constraints on journal entry lines to ensure non-negative amounts
            migrationBuilder.Sql(@"
                ALTER TABLE journal_entry_lines
                ADD CONSTRAINT CK_journal_entry_lines_debit_non_negative
                CHECK (""DebitAmount"" >= 0);
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE journal_entry_lines
                ADD CONSTRAINT CK_journal_entry_lines_credit_non_negative
                CHECK (""CreditAmount"" >= 0);
            ");

            // Create function to prevent modification/deletion of posted journal entries
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION prevent_posted_entry_modification()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF OLD.""Status"" = 1 THEN -- 1 = Posted status (EntryStatus.Posted enum value)
                        RAISE EXCEPTION 'Cannot modify or delete a posted journal entry. Entry ID: %', OLD.""Id"";
                    END IF;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger on UPDATE for journal_entries
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_prevent_posted_entry_update
                BEFORE UPDATE ON journal_entries
                FOR EACH ROW
                EXECUTE FUNCTION prevent_posted_entry_modification();
            ");

            // Create function for DELETE trigger (returns OLD instead of NEW)
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION prevent_posted_entry_deletion()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF OLD.""Status"" = 1 THEN -- 1 = Posted status
                        RAISE EXCEPTION 'Cannot delete a posted journal entry. Entry ID: %', OLD.""Id"";
                    END IF;
                    RETURN OLD;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Create trigger on DELETE for journal_entries
            migrationBuilder.Sql(@"
                CREATE TRIGGER trg_prevent_posted_entry_delete
                BEFORE DELETE ON journal_entries
                FOR EACH ROW
                EXECUTE FUNCTION prevent_posted_entry_deletion();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop triggers
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_prevent_posted_entry_delete ON journal_entries;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS trg_prevent_posted_entry_update ON journal_entries;");

            // Drop functions
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS prevent_posted_entry_deletion();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS prevent_posted_entry_modification();");

            // Drop check constraints
            migrationBuilder.Sql("ALTER TABLE journal_entry_lines DROP CONSTRAINT IF EXISTS CK_journal_entry_lines_credit_non_negative;");
            migrationBuilder.Sql("ALTER TABLE journal_entry_lines DROP CONSTRAINT IF EXISTS CK_journal_entry_lines_debit_non_negative;");
            migrationBuilder.Sql("ALTER TABLE journal_entries DROP CONSTRAINT IF EXISTS CK_journal_entries_balanced;");
        }
    }
}
