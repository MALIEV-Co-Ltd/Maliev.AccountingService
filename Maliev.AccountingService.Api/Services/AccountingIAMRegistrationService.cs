using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.AccountingService.Data.Data;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Background service that registers Accounting Service permissions and roles with IAM.
/// Uses the standard IAMRegistrationService base class.
/// </summary>
public class AccountingIAMRegistrationService : IAMRegistrationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccountingIAMRegistrationService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <param name="logger">The logger.</param>
    public AccountingIAMRegistrationService(
        IHttpClientFactory httpClientFactory,
        ILogger<AccountingIAMRegistrationService> logger)
        : base(httpClientFactory, logger, "accounting")
    {
    }

    /// <summary>
    /// Gets the list of permissions to register.
    /// </summary>
    /// <returns>A collection of permission registrations.</returns>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return AccountingPermissions.GetPermissions().Select(p => new PermissionRegistration
        {
            PermissionId = p.Code,
            Description = p.Description
        });
    }

    /// <summary>
    /// Gets the list of predefined roles to register.
    /// </summary>
    /// <returns>A collection of role registrations.</returns>
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