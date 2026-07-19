using Maliev.MessagingContracts.Contracts.Accounting;
using Maliev.MessagingContracts.Contracts.Invoices;

namespace Maliev.AccountingService.Api.Services;

/// <summary>
/// Service for processing financial events and transforming them into journal entries
/// </summary>
public interface IEventProcessingService
{
    /// <summary>
    /// Processes an invoice created event and creates corresponding journal entries
    /// </summary>
    Task<Guid> ProcessInvoiceCreatedAsync(InvoiceCreatedEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a payment received event and creates corresponding journal entries
    /// </summary>
    Task<Guid> ProcessPaymentReceivedAsync(PaymentRecordedEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a supplier invoice event and creates corresponding journal entries
    /// </summary>
    Task<Guid> ProcessSupplierInvoiceAsync(SupplierInvoiceReceivedEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes an inventory movement event and creates corresponding journal entries
    /// </summary>
    Task<Guid> ProcessInventoryMovementAsync(InventoryMovementEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a payroll processed event and creates corresponding journal entries
    /// </summary>
    Task<Guid> ProcessPayrollProcessedAsync(PayrollProcessedEvent @event, CancellationToken cancellationToken = default);
}
