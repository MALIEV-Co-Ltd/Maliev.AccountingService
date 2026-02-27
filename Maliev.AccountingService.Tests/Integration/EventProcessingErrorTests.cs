using Maliev.AccountingService.Api.Consumers;
using Maliev.MessagingContracts.Contracts.Accounting;
using Maliev.MessagingContracts;
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
        var messageId = Guid.NewGuid();
        var @event = new InventoryMovementEvent(
            MessageId: messageId,
            MessageName: nameof(InventoryMovementEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new InventoryMovementEventPayload
            {
                MovementId = Guid.NewGuid(),
                MovementNumber = "MOV-001",
                MovementType = "Purchase",
                TotalCost = 100.0,
                MovementDate = DateTimeOffset.UtcNow,
                ProductName = "Test Product"
            }
        );

        // Act
        await Factory.PublishEventAsync(@event);

        // Wait for processing (or failure)
        await Task.Delay(2000);

        // Assert
        var dbContext = Factory.GetDbContext();
        var entryExists = await dbContext.JournalEntries.AnyAsync(e => e.SourceEventId == messageId.ToString());
        Assert.False(entryExists);
    }
}
