namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Sales service when a payment is received
/// </summary>
public class PaymentReceivedEvent
{
    public string EventId { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid? InvoiceId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public DateTime CreatedAt { get; set; }
}
