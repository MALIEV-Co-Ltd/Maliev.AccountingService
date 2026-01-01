using Maliev.AccountingService.Api.Events;
using Maliev.AccountingService.Api.Services;
using MassTransit;
using System.Diagnostics;

namespace Maliev.AccountingService.Api.Consumers;

/// <summary>
/// Consumer for SupplierInvoice events from Procurement service
/// </summary>
public class SupplierInvoiceConsumer : IConsumer<SupplierInvoiceEvent>
{
    private readonly IEventProcessingService _eventProcessingService;
    private readonly ILogger<SupplierInvoiceConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SupplierInvoiceConsumer"/> class.
    /// </summary>
    /// <param name="eventProcessingService">The event processing service.</param>
    /// <param name="logger">The logger.</param>
    public SupplierInvoiceConsumer(
        IEventProcessingService eventProcessingService,
        ILogger<SupplierInvoiceConsumer> logger)
    {
        _eventProcessingService = eventProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Consumes the <see cref="SupplierInvoiceEvent"/>.
    /// </summary>
    /// <param name="context">The consume context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<SupplierInvoiceEvent> context)
    {
        using var activity = Activity.Current?.Source.StartActivity("ConsumeSupplierInvoice");
        activity?.SetTag("event.id", context.Message.EventId);
        activity?.SetTag("invoice.id", context.Message.InvoiceId);

        _logger.LogInformation(
            "Received SupplierInvoice event {EventId} for invoice {InvoiceId}",
            context.Message.EventId,
            context.Message.InvoiceId);

        try
        {
            var journalEntryId = await _eventProcessingService.ProcessSupplierInvoiceAsync(
                context.Message,
                context.CancellationToken);

            activity?.SetTag("journal.entry.id", journalEntryId);

            _logger.LogInformation(
                "Successfully processed SupplierInvoice event {EventId}, created journal entry {JournalEntryId}",
                context.Message.EventId,
                journalEntryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process SupplierInvoice event {EventId}",
                context.Message.EventId);
            throw;
        }
    }
}
