namespace Maliev.AccountingService.Data.Data;

/// <summary>
/// Predefined roles for the Accounting Service.
/// </summary>
public static class AccountingPredefinedRoles
{
    /// <summary>Administrator role with full access.</summary>
    public const string Admin = "roles.accounting.admin";
    /// <summary>Manager role for accounting management.</summary>
    public const string Manager = "roles.accounting.manager";
    /// <summary>Clerk role for data entry.</summary>
    public const string Clerk = "roles.accounting.clerk";
    /// <summary>Controller role for auditing and periods.</summary>
    public const string Controller = "roles.accounting.controller";
    /// <summary>Viewer role for read-only access.</summary>
    public const string Viewer = "roles.accounting.viewer";

    /// <summary>
    /// Collection of all predefined roles for the Accounting Service.
    /// </summary>
    public static readonly IReadOnlyList<(string RoleId, string Description, string[] Permissions)> All = new List<(string, string, string[])>
    {
        (Admin, "Full access to all accounting operations", AccountingPermissions.AllWithDescriptions.Keys.ToArray()),

        (Manager, "General accounting management access", AccountingPermissions.AllWithDescriptions.Keys
            .Where(p => p != AccountingPermissions.PeriodsClose && p != AccountingPermissions.PeriodsReopen).ToArray()),

        (Clerk, "Basic journal and account data entry", new[]
        {
            AccountingPermissions.JournalEntriesCreate,
            AccountingPermissions.JournalEntriesRead,
            AccountingPermissions.AccountsRead
        }.Concat(AccountingPermissions.AllWithDescriptions.Keys.Where(p => p.StartsWith("accounting.reports."))).ToArray()),

        (Controller, "Advanced accounting and period management", AccountingPermissions.AllWithDescriptions.Keys.ToArray()),

        (Viewer, "Read-only access to accounting data and reports", AccountingPermissions.AllWithDescriptions.Keys
            .Where(p => p.EndsWith(".read") || p.StartsWith("accounting.reports.")).ToArray())
    };

    /// <summary>
    /// Gets all role-permission mappings for testing and registration.
    /// </summary>
    public static IEnumerable<(string RoleName, string PermissionCode)> GetRolePermissions()
    {
        foreach (var role in All)
        {
            foreach (var permission in role.Permissions)
            {
                yield return (role.RoleId, permission);
            }
        }
    }
}
