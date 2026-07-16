using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Maliev.AccountingService.Application.Authorization;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

public class AuthZTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthZTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CallProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/accounting/v1/chart-of-accounts");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CallProtectedEndpoint_WithInsufficientPermission_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient(roles: new[] { "unknown-role" });

        // Act
        var response = await client.GetAsync("/accounting/v1/chart-of-accounts");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPeriods_WithCorrectPermission_ReturnsSuccess()
    {
        // Arrange
        var permissions = AccountingPredefinedRoles.GetRolePermissions()
            .Where(rp => rp.RoleName == "roles.accounting.manager")
            .Select(rp => rp.PermissionCode)
            .ToArray();
        var client = _factory.CreateAuthenticatedClient(roles: new[] { "accounting-manager" }, permissions: permissions);

        // Act
        var response = await client.GetAsync("/accounting/v1/periods");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetPeriods_WithViewerRole_ReturnsSuccessWithoutMutationAuthority()
    {
        var permissions = AccountingPredefinedRoles.GetRolePermissions()
            .Where(rp => rp.RoleName == AccountingPredefinedRoles.Viewer)
            .Select(rp => rp.PermissionCode)
            .ToArray();
        var client = _factory.CreateAuthenticatedClient(
            roles: new[] { AccountingPredefinedRoles.Viewer },
            permissions: permissions);

        var readResponse = await client.GetAsync("/accounting/v1/periods");
        var mutationResponse = await client.PostAsync(
            "/accounting/v1/periods/open?date=2026-07-17",
            null);

        readResponse.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Forbidden, mutationResponse.StatusCode);
    }

    [Fact]
    public async Task GetPeriods_WithOnlyPeriodsOpenPermission_ReturnsForbidden()
    {
        var client = _factory.CreateAuthenticatedClient(
            permissions: new[] { AccountingPermissions.PeriodsOpen });

        var response = await client.GetAsync("/accounting/v1/periods");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    public async Task RunReconciliation_WithoutRunPermission_ReturnsForbidden(string verb)
    {
        var client = _factory.CreateAuthenticatedClient(permissions: Array.Empty<string>());
        using var request = new HttpRequestMessage(
            new HttpMethod(verb),
            $"/accounting/v1/reconciliation/run?sourceSystem=Sales&periodId={Guid.NewGuid()}");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", AccountingPermissions.ReconciliationRun)]
    [InlineData("POST", AccountingPermissions.ReconciliationsRun)]
    public async Task RunReconciliation_WithMatchingPermission_ReturnsSuccess(
        string verb,
        string permission)
    {
        var client = _factory.CreateAuthenticatedClient(permissions: new[] { permission });
        using var request = new HttpRequestMessage(
            new HttpMethod(verb),
            $"/accounting/v1/reconciliation/run?sourceSystem=Sales&periodId={Guid.NewGuid()}");

        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();
    }

    [Theory]
    [InlineData("GET", AccountingPermissions.ReconciliationsRun)]
    [InlineData("POST", AccountingPermissions.ReconciliationRun)]
    public async Task RunReconciliation_WithOtherVerbPermission_ReturnsForbidden(
        string verb,
        string permission)
    {
        var client = _factory.CreateAuthenticatedClient(permissions: new[] { permission });
        using var request = new HttpRequestMessage(
            new HttpMethod(verb),
            $"/accounting/v1/reconciliation/run?sourceSystem=Sales&periodId={Guid.NewGuid()}");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetBalanceSheet_WithCorrectPermission_ReturnsSuccess()
    {
        // Arrange
        var permissions = AccountingPredefinedRoles.GetRolePermissions()
            .Where(rp => rp.RoleName == "roles.accounting.clerk")
            .Select(rp => rp.PermissionCode)
            .ToArray();
        var client = _factory.CreateAuthenticatedClient(roles: new[] { "accounting-clerk" }, permissions: permissions);

        // Act
        var response = await client.GetAsync("/accounting/v1/reports/balance-sheet");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ClosePeriod_WithClerkRole_ReturnsForbidden()
    {
        // Arrange - accounting-clerk does NOT have periods.close
        var permissions = AccountingPredefinedRoles.GetRolePermissions()
            .Where(rp => rp.RoleName == "roles.accounting.clerk")
            .Select(rp => rp.PermissionCode)
            .ToArray();
        var client = _factory.CreateAuthenticatedClient(roles: new[] { "accounting-clerk" }, permissions: permissions);

        // Act
        var response = await client.PostAsync($"/accounting/v1/periods/{Guid.NewGuid()}/close", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
