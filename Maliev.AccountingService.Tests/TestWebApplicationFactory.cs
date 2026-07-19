using Maliev.AccountingService.Api.Services;
using Maliev.AccountingService.Infrastructure.Data;
using Maliev.AccountingService.Tests.Testing;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Maliev.AccountingService.Tests;

public class TestWebApplicationFactory : BaseIntegrationTestFactory<Program, AccountingDbContext>
{
    private ITestHarness? _testHarness;
    private IBusControl? _busControl;

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
        // Use InMemory event idempotency for tests
        services.RemoveAll<IEventIdempotencyService>();
        services.AddSingleton<IEventIdempotencyService, InMemoryEventIdempotencyService>();
    }

    /// <summary>
    /// Gets the MassTransit test harness for verifying message consumption.
    /// </summary>
    public ITestHarness GetTestHarness()
    {
        if (_testHarness == null)
        {
            _testHarness = Services.GetRequiredService<ITestHarness>();
            _busControl = Services.GetRequiredService<IBusControl>();
        }
        return _testHarness;
    }

    /// <summary>
    /// Publishes an event to MassTransit for testing event-driven workflows.
    /// </summary>
    public async Task PublishEventAsync<T>(T message) where T : class
    {
        var logger = Services.GetService<ILogger<TestWebApplicationFactory>>();

        try
        {
            var bus = _busControl ?? Services.GetRequiredService<IBusControl>();
            var address = bus.Address;
            logger?.LogInformation("Publishing message to {Address}", address);

            await bus.Publish(message);

            logger?.LogInformation("Message published successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to publish message");
            throw;
        }
    }
}
