using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Maliev.AccountingService.Data.Data;
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
    public async Task GetPermissions_ReturnsSuccessAndCorrectCount()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/accounting/v1/permissions");

        // Assert
        response.EnsureSuccessStatusCode();
        var permissions = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(permissions);
        Assert.Equal(20, permissions.Count);
    }

    [Fact]
    public async Task GetRoles_ReturnsSuccessAndCorrectCount()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/accounting/v1/permissions/roles");

        // Assert
        response.EnsureSuccessStatusCode();
        var roles = await response.Content.ReadFromJsonAsync<List<object>>();
        Assert.NotNull(roles);
        Assert.Equal(5, roles.Count);
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
