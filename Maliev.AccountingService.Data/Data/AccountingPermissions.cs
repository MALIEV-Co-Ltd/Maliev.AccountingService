using Maliev.AccountingService.Data.Models;

namespace Maliev.AccountingService.Data.Data;

/// <summary>
/// Defines permission constants for the Accounting Service.
/// Note: Constants include "Permission:" prefix for integration with ServiceDefaults policy provider.
/// </summary>
public static class AccountingPermissions
{
    // Journal Entry Operations
    public const string JournalEntriesCreate = "Permission:accounting.journal-entries.create";
    public const string JournalEntriesRead = "Permission:accounting.journal-entries.read";
    public const string JournalEntriesUpdate = "Permission:accounting.journal-entries.update";
    public const string JournalEntriesPost = "Permission:accounting.journal-entries.post";
    public const string JournalEntriesReverse = "Permission:accounting.journal-entries.reverse";

    // Account Operations
    public const string AccountsCreate = "Permission:accounting.accounts.create";
    public const string AccountsRead = "Permission:accounting.accounts.read";
    public const string AccountsUpdate = "Permission:accounting.accounts.update";
    public const string AccountsDelete = "Permission:accounting.accounts.delete";
    public const string AccountsClose = "Permission:accounting.accounts.close";

    // Financial Report Operations
    public const string ReportsBalanceSheet = "Permission:accounting.reports.balance-sheet";
    public const string ReportsIncomeStatement = "Permission:accounting.reports.income-statement";
    public const string ReportsCashFlow = "Permission:accounting.reports.cash-flow";
    public const string ReportsTrialBalance = "Permission:accounting.reports.trial-balance";
    public const string ReportsExport = "Permission:accounting.reports.export";

    // Period Operations
    public const string PeriodsOpen = "Permission:accounting.periods.open";
    public const string PeriodsClose = "Permission:accounting.periods.close";
    public const string PeriodsReopen = "Permission:accounting.periods.reopen";

    // Reconciliation Operations
    public const string ReconciliationRun = "Permission:accounting.reconciliation.run";
    public const string ReconciliationRead = "Permission:accounting.reconciliation.read";

    /// <summary>
    /// Gets the list of permissions for registration with IAM.
    /// Strips the "Permission:" prefix from codes.
    /// </summary>
    public static IEnumerable<Permission> GetPermissions()
    {
        return new List<Permission>
        {
            // Journal Entry Operations
            new() { Code = JournalEntriesCreate.Replace("Permission:", ""), Description = "Create journal entries", IsCritical = false },
            new() { Code = JournalEntriesRead.Replace("Permission:", ""), Description = "Read journal entries", IsCritical = false },
            new() { Code = JournalEntriesUpdate.Replace("Permission:", ""), Description = "Update journal entries", IsCritical = false },
            new() { Code = JournalEntriesPost.Replace("Permission:", ""), Description = "Post journal entries", IsCritical = true },
            new() { Code = JournalEntriesReverse.Replace("Permission:", ""), Description = "Reverse journal entries", IsCritical = true },

            // Account Operations
            new() { Code = AccountsCreate.Replace("Permission:", ""), Description = "Create chart of accounts", IsCritical = false },
            new() { Code = AccountsRead.Replace("Permission:", ""), Description = "Read account details", IsCritical = false },
            new() { Code = AccountsUpdate.Replace("Permission:", ""), Description = "Update accounts", IsCritical = false },
            new() { Code = AccountsDelete.Replace("Permission:", ""), Description = "Deactivate accounts", IsCritical = false },
            new() { Code = AccountsClose.Replace("Permission:", ""), Description = "Close accounts", IsCritical = true },

            // Financial Report Operations
            new() { Code = ReportsBalanceSheet.Replace("Permission:", ""), Description = "View balance sheet", IsCritical = false },
            new() { Code = ReportsIncomeStatement.Replace("Permission:", ""), Description = "View income statement", IsCritical = false },
            new() { Code = ReportsCashFlow.Replace("Permission:", ""), Description = "View cash flow statement", IsCritical = false },
            new() { Code = ReportsTrialBalance.Replace("Permission:", ""), Description = "View trial balance", IsCritical = false },
            new() { Code = ReportsExport.Replace("Permission:", ""), Description = "Export financial reports", IsCritical = false },

            // Period Operations
            new() { Code = PeriodsOpen.Replace("Permission:", ""), Description = "Open accounting periods", IsCritical = false },
            new() { Code = PeriodsClose.Replace("Permission:", ""), Description = "Close accounting periods", IsCritical = true },
            new() { Code = PeriodsReopen.Replace("Permission:", ""), Description = "Reopen closed periods", IsCritical = true },

            // Reconciliation Operations
            new() { Code = ReconciliationRun.Replace("Permission:", ""), Description = "Run financial reconciliation", IsCritical = true },
            new() { Code = ReconciliationRead.Replace("Permission:", ""), Description = "Read reconciliation reports", IsCritical = false }
        };
    }
}