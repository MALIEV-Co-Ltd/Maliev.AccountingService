using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.AccountingService.Data.Data;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Background service that registers Accounting Service permissions and roles with IAM.
/// Uses the standard IAMRegistrationService base class.
/// </summary>
public class AccountingIAMRegistrationService : IAMRegistrationService
{
    public AccountingIAMRegistrationService(
        IHttpClientFactory httpClientFactory,
        ILogger<AccountingIAMRegistrationService> logger)
        : base(httpClientFactory, logger, "accounting")
    {
    }

    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return AccountingPermissions.GetPermissions().Select(p => new PermissionRegistration
        {
            PermissionId = p.Code,
            Description = p.Description
        });
    }

    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        var rolePermissions = AccountingPredefinedRoles.GetRolePermissions().ToList();

        return AccountingPredefinedRoles.GetRoles().Select(r => new RoleRegistration
        {
            RoleId = r.Name,
            Description = r.Description,
            PermissionIds = rolePermissions
                .Where(rp => rp.RoleName == r.Name)
                .Select(rp => rp.PermissionCode)
                .ToList(),
            IsCustom = false
        });
    }
}