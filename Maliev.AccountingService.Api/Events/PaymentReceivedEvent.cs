namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Sales service when a payment is received
/// </summary>
public class PaymentReceivedEvent
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment ID.
    /// </summary>
    public Guid PaymentId { get; set; }

    /// <summary>
    /// Gets or sets the payment number.
    /// </summary>
    public string PaymentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer ID.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the optional invoice ID associated with the payment.
    /// </summary>
    public Guid? InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the payment date.
    /// </summary>
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Gets or sets the payment amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the payment method (e.g., CreditCard, Cash, BankTransfer).
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment reference.
    /// </summary>
    public string? Reference { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
