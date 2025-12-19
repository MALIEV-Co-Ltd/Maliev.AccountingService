using Maliev.AccountingService.Api.Events;
using Maliev.AccountingService.Api.Services;
using MassTransit;
using System.Diagnostics;

namespace Maliev.AccountingService.Api.Consumers;

/// <summary>
/// Consumer for PayrollProcessed events from Payroll service
/// </summary>
public class PayrollProcessedConsumer : IConsumer<PayrollProcessedEvent>
{
    private readonly IEventProcessingService _eventProcessingService;
    private readonly ILogger<PayrollProcessedConsumer> _logger;

    public PayrollProcessedConsumer(
        IEventProcessingService eventProcessingService,
        ILogger<PayrollProcessedConsumer> logger)
    {
        _eventProcessingService = eventProcessingService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PayrollProcessedEvent> context)
    {
        using var activity = Activity.Current?.Source.StartActivity("ConsumePayrollProcessed");
        activity?.SetTag("event.id", context.Message.EventId);
        activity?.SetTag("payroll.id", context.Message.PayrollId);

        _logger.LogInformation(
            "Received PayrollProcessed event {EventId} for payroll {PayrollId}",
            context.Message.EventId,
            context.Message.PayrollId);

        try
        {
            var journalEntryId = await _eventProcessingService.ProcessPayrollProcessedAsync(
                context.Message,
                context.CancellationToken);

            activity?.SetTag("journal.entry.id", journalEntryId);

            _logger.LogInformation(
                "Successfully processed PayrollProcessed event {EventId}, created journal entry {JournalEntryId}",
                context.Message.EventId,
                journalEntryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process PayrollProcessed event {EventId}",
                context.Message.EventId);
            throw;
        }
    }
}
