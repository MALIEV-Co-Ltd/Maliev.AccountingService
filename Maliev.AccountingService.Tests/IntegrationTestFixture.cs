namespace Maliev.AccountingService.Tests;

/// <summary>
/// Simplified integration test fixture using BaseIntegrationTestFactory.
/// BaseIntegrationTestFactory already provides PostgreSQL, Redis, and RabbitMQ containers.
/// </summary>
public class IntegrationTestFixture : IAsyncLifetime
{
    public TestWebApplicationFactory WebAppFactory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // BaseIntegrationTestFactory handles all container initialization
        WebAppFactory = new TestWebApplicationFactory();
        await WebAppFactory.InitializeAsync();

        // Trigger server creation to start hosted services (MassTransit consumers)
        _ = WebAppFactory.Server;

        // Seed data using the factory's DbContext
        var dbContext = WebAppFactory.GetDbContext();
        await TestDataSeeder.SeedChartOfAccountsAsync(dbContext);

        // Give MassTransit time to connect and start consumers
        await Task.Delay(TimeSpan.FromSeconds(5));
    }

    public async Task DisposeAsync()
    {
        if (WebAppFactory != null)
        {
            await WebAppFactory.DisposeAsync();
        }
    }
}

/// <summary>
/// Collection definition for integration tests
/// </summary>
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
