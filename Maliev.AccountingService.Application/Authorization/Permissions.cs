namespace Maliev.AccountingService.Application.Authorization;

/// <summary>
/// Defines permission constants for the Accounting Service.
/// Follows GCP-style naming: {service}.{resource}.{action}
/// </summary>
public static class AccountingPermissions
{
    // Journal Entry Operations
    /// <summary>Permission to create journal entries.</summary>
    public const string JournalEntriesCreate = "accounting.journal-entries.create";
    /// <summary>Permission to read journal entries.</summary>
    public const string JournalEntriesRead = "accounting.journal-entries.read";
    /// <summary>Permission to update journal entries.</summary>
    public const string JournalEntriesUpdate = "accounting.journal-entries.update";
    /// <summary>Permission to post journal entries.</summary>
    public const string JournalEntriesPost = "accounting.journal-entries.post";
    /// <summary>Permission to reverse journal entries.</summary>
    public const string JournalEntriesReverse = "accounting.journal-entries.reverse";

    // Account Operations
    /// <summary>Permission to create accounts.</summary>
    public const string AccountsCreate = "accounting.accounts.create";
    /// <summary>Permission to read accounts.</summary>
    public const string AccountsRead = "accounting.accounts.read";
    /// <summary>Permission to update accounts.</summary>
    public const string AccountsUpdate = "accounting.accounts.update";
    /// <summary>Permission to delete accounts.</summary>
    public const string AccountsDelete = "accounting.accounts.delete";
    /// <summary>Permission to close accounts.</summary>
    public const string AccountsClose = "accounting.accounts.close";

    // Financial Report Operations
    /// <summary>Permission to view balance sheet.</summary>
    public const string ReportsBalanceSheet = "accounting.reports.balance-sheet";
    /// <summary>Permission to view income statement.</summary>
    public const string ReportsIncomeStatement = "accounting.reports.income-statement";
    /// <summary>Permission to view cash flow statement.</summary>
    public const string ReportsCashFlow = "accounting.reports.cash-flow";
    /// <summary>Permission to view trial balance.</summary>
    public const string ReportsTrialBalance = "accounting.reports.trial-balance";
    /// <summary>Permission to export reports.</summary>
    public const string ReportsExport = "accounting.reports.export";

    // Period Operations
    /// <summary>Permission to open periods.</summary>
    public const string PeriodsOpen = "accounting.periods.open";
    /// <summary>Permission to close periods.</summary>
    public const string PeriodsClose = "accounting.periods.close";
    /// <summary>Permission to reopen periods.</summary>
    public const string PeriodsReopen = "accounting.periods.reopen";

    // Reconciliation Operations
    /// <summary>Permission to run reconciliation.</summary>
    public const string ReconciliationRun = "accounting.reconciliation.run";
    /// <summary>Permission to read reconciliation results.</summary>
    public const string ReconciliationRead = "accounting.reconciliation.read";

    /// <summary>
    /// Collection of all defined accounting permissions with descriptions.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> AllWithDescriptions = new Dictionary<string, string>
    {
        { JournalEntriesCreate, "Create journal entries" },
        { JournalEntriesRead, "Read journal entries" },
        { JournalEntriesUpdate, "Update journal entries" },
        { JournalEntriesPost, "Post journal entries" },
        { JournalEntriesReverse, "Reverse journal entries" },
        { AccountsCreate, "Create chart of accounts" },
        { AccountsRead, "Read account details" },
        { AccountsUpdate, "Update accounts" },
        { AccountsDelete, "Deactivate accounts" },
        { AccountsClose, "Close accounts" },
        { ReportsBalanceSheet, "View balance sheet" },
        { ReportsIncomeStatement, "View income statement" },
        { ReportsCashFlow, "View cash flow statement" },
        { ReportsTrialBalance, "View trial balance" },
        { ReportsExport, "Export financial reports" },
        { PeriodsOpen, "Open accounting periods" },
        { PeriodsClose, "Close accounting periods" },
        { PeriodsReopen, "Reopen closed periods" },
        { ReconciliationRun, "Run financial reconciliation" },
        { ReconciliationRead, "Read reconciliation reports" }
    };

    /// <summary>
    /// Gets the list of all permission codes.
    /// </summary>
    public static IEnumerable<string> All => AllWithDescriptions.Keys;

    /// <summary>
    /// Collection of permissions that are considered critical and require additional security checks.
    /// </summary>
    public static readonly IReadOnlySet<string> CriticalPermissions = new HashSet<string>
    {
        JournalEntriesPost,
        JournalEntriesReverse,
        AccountsClose,
        PeriodsClose,
        PeriodsReopen,
        ReconciliationRun
    };
}
