using Maliev.AccountingService.Data.Models;

namespace Maliev.AccountingService.Data.Data;

public static class AccountingPermissions
{
    public static IEnumerable<Permission> GetPermissions()
    {
        return new List<Permission>
        {
            // Journal Entry Operations
            new() { Code = "accounting.journal-entries.create", Description = "Create journal entries", IsCritical = false },
            new() { Code = "accounting.journal-entries.read", Description = "Read journal entries", IsCritical = false },
            new() { Code = "accounting.journal-entries.update", Description = "Update journal entries", IsCritical = false },
            new() { Code = "accounting.journal-entries.post", Description = "Post journal entries", IsCritical = true },
            new() { Code = "accounting.journal-entries.reverse", Description = "Reverse journal entries", IsCritical = true },

            // Account Operations
            new() { Code = "accounting.accounts.create", Description = "Create chart of accounts", IsCritical = false },
            new() { Code = "accounting.accounts.read", Description = "Read account details", IsCritical = false },
            new() { Code = "accounting.accounts.update", Description = "Update accounts", IsCritical = false },
            new() { Code = "accounting.accounts.delete", Description = "Deactivate accounts", IsCritical = false },
            new() { Code = "accounting.accounts.close", Description = "Close accounts", IsCritical = true },

            // Financial Report Operations
            new() { Code = "accounting.reports.balance-sheet", Description = "View balance sheet", IsCritical = false },
            new() { Code = "accounting.reports.income-statement", Description = "View income statement", IsCritical = false },
            new() { Code = "accounting.reports.cash-flow", Description = "View cash flow statement", IsCritical = false },
            new() { Code = "accounting.reports.trial-balance", Description = "View trial balance", IsCritical = false },
            new() { Code = "accounting.reports.export", Description = "Export financial reports", IsCritical = false },

            // Period Operations
            new() { Code = "accounting.periods.open", Description = "Open accounting periods", IsCritical = false },
            new() { Code = "accounting.periods.close", Description = "Close accounting periods", IsCritical = true },
            new() { Code = "accounting.periods.reopen", Description = "Reopen closed periods", IsCritical = true }
        };
    }
}
