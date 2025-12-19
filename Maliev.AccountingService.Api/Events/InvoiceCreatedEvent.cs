namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Sales service when an invoice is created
/// </summary>
public class InvoiceCreatedEvent
{
    public string EventId { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string TaxType { get; set; } = "VAT";
    public decimal TaxRate { get; set; }
    public List<InvoiceLineItem> LineItems { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class InvoiceLineItem
{
    public Guid ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}
