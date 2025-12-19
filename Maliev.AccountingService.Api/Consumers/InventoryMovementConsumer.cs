using Maliev.AccountingService.Api.Events;
using Maliev.AccountingService.Api.Services;
using MassTransit;
using System.Diagnostics;

namespace Maliev.AccountingService.Api.Consumers;

/// <summary>
/// Consumer for InventoryMovement events from Inventory service
/// </summary>
public class InventoryMovementConsumer : IConsumer<InventoryMovementEvent>
{
    private readonly IEventProcessingService _eventProcessingService;
    private readonly ILogger<InventoryMovementConsumer> _logger;

    public InventoryMovementConsumer(
        IEventProcessingService eventProcessingService,
        ILogger<InventoryMovementConsumer> logger)
    {
        _eventProcessingService = eventProcessingService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InventoryMovementEvent> context)
    {
        using var activity = Activity.Current?.Source.StartActivity("ConsumeInventoryMovement");
        activity?.SetTag("event.id", context.Message.EventId);
        activity?.SetTag("movement.id", context.Message.MovementId);

        _logger.LogInformation(
            "Received InventoryMovement event {EventId} for movement {MovementId}",
            context.Message.EventId,
            context.Message.MovementId);

        try
        {
            var journalEntryId = await _eventProcessingService.ProcessInventoryMovementAsync(
                context.Message,
                context.CancellationToken);

            activity?.SetTag("journal.entry.id", journalEntryId);

            _logger.LogInformation(
                "Successfully processed InventoryMovement event {EventId}, created journal entry {JournalEntryId}",
                context.Message.EventId,
                journalEntryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process InventoryMovement event {EventId}",
                context.Message.EventId);
            throw;
        }
    }
}
