using Maliev.AccountingService.Api.Events;
using Maliev.AccountingService.Api.Services;
using MassTransit;
using System.Diagnostics;

namespace Maliev.AccountingService.Api.Consumers;

/// <summary>
/// Consumer for InvoiceCreated events from Sales service
/// </summary>
public class InvoiceCreatedConsumer : IConsumer<InvoiceCreatedEvent>
{
    private readonly IEventProcessingService _eventProcessingService;
    private readonly ILogger<InvoiceCreatedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceCreatedConsumer"/> class.
    /// </summary>
    /// <param name="eventProcessingService">The event processing service.</param>
    /// <param name="logger">The logger.</param>
    public InvoiceCreatedConsumer(
        IEventProcessingService eventProcessingService,
        ILogger<InvoiceCreatedConsumer> logger)
    {
        _eventProcessingService = eventProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Consumes the <see cref="InvoiceCreatedEvent"/>.
    /// </summary>
    /// <param name="context">The consume context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<InvoiceCreatedEvent> context)
    {
        using var activity = Activity.Current?.Source.StartActivity("ConsumeInvoiceCreated");
        activity?.SetTag("event.id", context.Message.EventId);
        activity?.SetTag("invoice.id", context.Message.InvoiceId);

        _logger.LogInformation(
            "Received InvoiceCreated event {EventId} for invoice {InvoiceId}",
            context.Message.EventId,
            context.Message.InvoiceId);

        try
        {
            var journalEntryId = await _eventProcessingService.ProcessInvoiceCreatedAsync(
                context.Message,
                context.CancellationToken);

            activity?.SetTag("journal.entry.id", journalEntryId);

            _logger.LogInformation(
                "Successfully processed InvoiceCreated event {EventId}, created journal entry {JournalEntryId}",
                context.Message.EventId,
                journalEntryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process InvoiceCreated event {EventId}",
                context.Message.EventId);

            throw; // Let MassTransit handle retry
        }
    }
}
