using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Data.Data;
using Maliev.AccountingService.Tests.Testing;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Maliev.AccountingService.Tests;

public class TestWebApplicationFactory : BaseIntegrationTestFactory<Program, AccountingDbContext>
{
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
        // Use InMemory event idempotency for tests
        services.RemoveAll<IEventIdempotencyService>();
        services.AddSingleton<IEventIdempotencyService, InMemoryEventIdempotencyService>();
    }

    /// <summary>
    /// Publishes an event to MassTransit for testing event-driven workflows.
    /// </summary>
    public async Task PublishEventAsync<T>(T message) where T : class
    {
        using var scope = Services.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        await publishEndpoint.Publish(message);
    }
}
