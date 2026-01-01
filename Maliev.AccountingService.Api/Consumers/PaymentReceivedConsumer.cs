using Maliev.AccountingService.Api.Events;
using Maliev.AccountingService.Api.Services;
using MassTransit;
using System.Diagnostics;

namespace Maliev.AccountingService.Api.Consumers;

/// <summary>
/// Consumer for PaymentReceived events from Sales service
/// </summary>
public class PaymentReceivedConsumer : IConsumer<PaymentReceivedEvent>
{
    private readonly IEventProcessingService _eventProcessingService;
    private readonly ILogger<PaymentReceivedConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentReceivedConsumer"/> class.
    /// </summary>
    /// <param name="eventProcessingService">The event processing service.</param>
    /// <param name="logger">The logger.</param>
    public PaymentReceivedConsumer(
        IEventProcessingService eventProcessingService,
        ILogger<PaymentReceivedConsumer> logger)
    {
        _eventProcessingService = eventProcessingService;
        _logger = logger;
    }

    /// <summary>
    /// Consumes the <see cref="PaymentReceivedEvent"/>.
    /// </summary>
    /// <param name="context">The consume context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Consume(ConsumeContext<PaymentReceivedEvent> context)
    {
        using var activity = Activity.Current?.Source.StartActivity("ConsumePaymentReceived");
        activity?.SetTag("event.id", context.Message.EventId);
        activity?.SetTag("payment.id", context.Message.PaymentId);

        _logger.LogInformation(
            "Received PaymentReceived event {EventId} for payment {PaymentId}",
            context.Message.EventId,
            context.Message.PaymentId);

        try
        {
            var journalEntryId = await _eventProcessingService.ProcessPaymentReceivedAsync(
                context.Message,
                context.CancellationToken);

            activity?.SetTag("journal.entry.id", journalEntryId);

            _logger.LogInformation(
                "Successfully processed PaymentReceived event {EventId}, created journal entry {JournalEntryId}",
                context.Message.EventId,
                journalEntryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process PaymentReceived event {EventId}",
                context.Message.EventId);
            throw;
        }
    }
}
