using Maliev.AccountingService.Infrastructure.Data;
using Maliev.Aspire.ServiceDefaults.IAM;

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
    /// <param name="configuration">Application configuration.</param>
    /// <param name="logger">The logger.</param>
    public AccountingIAMRegistrationService(
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        ILogger<AccountingIAMRegistrationService> logger)
        : base(configuration, logger, "accounting")
    {
    }

    /// <summary>
    /// Gets the list of permissions to register.
    /// </summary>
    /// <returns>A collection of permission registrations.</returns>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return AccountingPermissions.AllWithDescriptions.Select(p => new PermissionRegistration
        {
            PermissionId = p.Key,
            Description = p.Value
        });
    }

    /// <summary>
    /// Gets the list of predefined roles to register.
    /// </summary>
    /// <returns>A collection of role registrations.</returns>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return AccountingPredefinedRoles.All.Select(r => new RoleRegistration
        {
            RoleId = r.RoleId,
            Description = r.Description,
            PermissionIds = r.Permissions.ToList(),
            IsCustom = false
        });
    }
}
