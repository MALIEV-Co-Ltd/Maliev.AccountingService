using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Application.Authorization;
using Maliev.Aspire.ServiceDefaults.IAM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Maliev.AccountingService.Tests.Unit;

/// <summary>
/// Verifies the canonical Accounting permission registry and least-privilege role mappings.
/// </summary>
public sealed class AccountingPermissionContractTests
{
    /// <summary>
    /// Period reads must be registered independently from period mutations.
    /// </summary>
    [Fact]
    public void PeriodsRead_IsCanonicalRegisteredPermission()
    {
        Assert.Equal("accounting.periods.read", AccountingPermissions.PeriodsRead);
        Assert.Equal("Read accounting periods", AccountingPermissions.AllWithDescriptions[AccountingPermissions.PeriodsRead]);
    }

    /// <summary>
    /// Viewer access must include period reads without granting period mutation authority.
    /// </summary>
    [Fact]
    public void ViewerRole_CanReadPeriodsButCannotMutatePeriodsOrRunReconciliation()
    {
        var permissions = AccountingPredefinedRoles.GetRolePermissions()
            .Where(mapping => mapping.RoleName == AccountingPredefinedRoles.Viewer)
            .Select(mapping => mapping.PermissionCode)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains(AccountingPermissions.PeriodsRead, permissions);
        Assert.DoesNotContain(AccountingPermissions.PeriodsOpen, permissions);
        Assert.DoesNotContain(AccountingPermissions.PeriodsClose, permissions);
        Assert.DoesNotContain(AccountingPermissions.PeriodsReopen, permissions);
        Assert.DoesNotContain(AccountingPermissions.ReconciliationRun, permissions);
        Assert.DoesNotContain(AccountingPermissions.ReconciliationsRun, permissions);
    }

    /// <summary>
    /// Reconciliation mutations use the canonical plural resource while legacy GET callers retain their permission.
    /// </summary>
    [Fact]
    public void ReconciliationRun_RegistersCanonicalAndLegacyCompatibilityPermissions()
    {
        Assert.Equal("accounting.reconciliation.run", AccountingPermissions.ReconciliationRun);
        Assert.Equal("accounting.reconciliations.run", AccountingPermissions.ReconciliationsRun);
        Assert.Equal(
            "Run financial reconciliation",
            AccountingPermissions.AllWithDescriptions[AccountingPermissions.ReconciliationRun]);
        Assert.Equal(
            "Run financial reconciliations using the state-changing API",
            AccountingPermissions.AllWithDescriptions[AccountingPermissions.ReconciliationsRun]);

        var managerPermissions = AccountingPredefinedRoles.GetRolePermissions()
            .Where(mapping => mapping.RoleName == AccountingPredefinedRoles.Manager)
            .Select(mapping => mapping.PermissionCode)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains(AccountingPermissions.ReconciliationRun, managerPermissions);
        Assert.Contains(AccountingPermissions.ReconciliationsRun, managerPermissions);
    }

    /// <summary>
    /// IAM registration must publish the read permission and its canonical description.
    /// </summary>
    [Fact]
    public void IamRegistration_IncludesPeriodsReadPermission()
    {
        var service = new RegistrationProbe(
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<AccountingIAMRegistrationService>>());

        var permission = Assert.Single(
            service.GetRegisteredPermissions(),
            registration => registration.PermissionId == AccountingPermissions.PeriodsRead);

        Assert.Equal("Read accounting periods", permission.Description);
    }

    /// <summary>
    /// IAM registration must publish both sides of the additive reconciliation permission migration.
    /// </summary>
    [Fact]
    public void IamRegistration_IncludesCanonicalAndLegacyReconciliationPermissions()
    {
        var service = new RegistrationProbe(
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<AccountingIAMRegistrationService>>());
        var permissions = service.GetRegisteredPermissions().ToDictionary(
            registration => registration.PermissionId,
            registration => registration.Description,
            StringComparer.Ordinal);

        Assert.Equal(
            "Run financial reconciliation",
            permissions[AccountingPermissions.ReconciliationRun]);
        Assert.Equal(
            "Run financial reconciliations using the state-changing API",
            permissions[AccountingPermissions.ReconciliationsRun]);
    }

    private sealed class RegistrationProbe(
        IConfiguration configuration,
        ILogger<AccountingIAMRegistrationService> logger)
        : AccountingIAMRegistrationService(configuration, logger)
    {
        public IEnumerable<PermissionRegistration> GetRegisteredPermissions() => GetPermissions();
    }
}
