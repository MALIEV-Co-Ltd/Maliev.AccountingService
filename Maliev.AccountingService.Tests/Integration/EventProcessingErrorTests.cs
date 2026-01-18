using Maliev.AccountingService.Api.Consumers;
using Maliev.AccountingService.Api.Events;
using Maliev.AccountingService.Api.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.AccountingService.Tests.Integration;

public class EventProcessingErrorTests : BaseIntegrationTest
{
    public EventProcessingErrorTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task InventoryMovementEvent_ShouldNotCreateEntry_WhenAccountMissing()
    {
        await CleanDatabaseAsync(); // Ensure no accounts exist

        // Arrange
        var @event = new InventoryMovementEvent
        {
            EventId = Guid.NewGuid().ToString(),
            MovementId = Guid.NewGuid(),
            MovementType = "Purchase",
            TotalCost = 100m,
            MovementDate = DateTime.UtcNow
        };

        // Act
        await Factory.PublishEventAsync(@event);

        // Wait for processing (or failure)
        await Task.Delay(2000);

        // Assert
        var dbContext = Factory.GetDbContext();
        var entryExists = await dbContext.JournalEntries.AnyAsync(e => e.SourceEventId == @event.EventId.ToString());
        Assert.False(entryExists);
    }
}
