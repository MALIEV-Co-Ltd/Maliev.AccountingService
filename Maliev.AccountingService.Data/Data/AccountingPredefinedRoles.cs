using Maliev.AccountingService.Data.Models;

namespace Maliev.AccountingService.Data.Data;

public static class AccountingPredefinedRoles
{
    public static IEnumerable<Role> GetRoles()
    {
        return new List<Role>
        {
            new() { Name = "roles.accounting.admin", Description = "Full access to all accounting operations" },
            new() { Name = "roles.accounting.manager", Description = "General accounting management access" },
            new() { Name = "roles.accounting.clerk", Description = "Basic journal and account data entry" },
            new() { Name = "roles.accounting.controller", Description = "Advanced accounting and period management" },
            new() { Name = "roles.accounting.viewer", Description = "Read-only access to accounting data and reports" }
        };
    }

    public static IEnumerable<RolePermission> GetRolePermissions()
    {
        var allPermissions = AccountingPermissions.GetPermissions().Select(p => p.Code).ToList();
        var roles = new List<RolePermission>();

        // roles.accounting.admin: All permissions
        foreach (var p in allPermissions)
        {
            roles.Add(new RolePermission { RoleName = "roles.accounting.admin", PermissionCode = p });
        }

        // roles.accounting.manager: All except periods.close, periods.reopen
        foreach (var p in allPermissions.Where(p => p != "accounting.periods.close" && p != "accounting.periods.reopen"))
        {
            roles.Add(new RolePermission { RoleName = "roles.accounting.manager", PermissionCode = p });
        }

        // roles.accounting.clerk: journal-entries.create/read, accounts.read, reports.*
        var clerkPerms = new List<string> { "accounting.journal-entries.create", "accounting.journal-entries.read", "accounting.accounts.read" };
        clerkPerms.AddRange(allPermissions.Where(p => p.StartsWith("accounting.reports.")));
        foreach (var p in clerkPerms)
        {
            roles.Add(new RolePermission { RoleName = "roles.accounting.clerk", PermissionCode = p });
        }

        // roles.accounting.controller: journal-entries.*, accounts.*, periods.*, reports.*
        foreach (var p in allPermissions)
        {
            roles.Add(new RolePermission { RoleName = "roles.accounting.controller", PermissionCode = p });
        }

        // roles.accounting.viewer: *.read, reports.*
        var viewerPerms = allPermissions.Where(p => p.EndsWith(".read") || p.StartsWith("accounting.reports.")).ToList();
        foreach (var p in viewerPerms)
        {
            roles.Add(new RolePermission { RoleName = "roles.accounting.viewer", PermissionCode = p });
        }

        return roles.DistinctBy(rp => new { rp.RoleName, rp.PermissionCode });
    }
}
