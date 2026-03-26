using Maliev.AccountingService.Application.Authorization;

namespace Maliev.AccountingService.Tests.Integration;

/// <summary>
/// Base class for integration tests providing common utilities and cleanup helpers.
/// </summary>
[Collection("Integration Tests")]
public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected BaseIntegrationTest(IntegrationTestFixture fixture)
    {
        Factory = fixture.WebAppFactory;

        var roles = new[] { "roles.accounting.admin", "roles.accounting.controller" };
        var permissions = AccountingPredefinedRoles.GetRolePermissions()
            .Where(rp => roles.Contains(rp.RoleName))
            .Select(rp => rp.PermissionCode)
            .Distinct()
            .ToArray();

        Client = Factory.CreateAuthenticatedClient(roles: roles, permissions: permissions);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Clean database after all tests in this class complete
        await Factory.CleanDatabaseAsync();
    }

    /// <summary>
    /// Cleans the database to ensure test isolation.
    /// Call this at the start of each test method to ensure a clean state.
    /// </summary>
    protected async Task CleanDatabaseAsync()
    {
        await Factory.CleanDatabaseAsync();
        Factory.ClearCache();
    }
}
