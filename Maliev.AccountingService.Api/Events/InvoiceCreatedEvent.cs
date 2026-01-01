namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Sales service when an invoice is created
/// </summary>
public class InvoiceCreatedEvent
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the invoice ID.
    /// </summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the invoice number.
    /// </summary>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer ID.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Gets or sets the invoice date.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Gets or sets the subtotal amount.
    /// </summary>
    public decimal SubtotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax amount.
    /// </summary>
    public decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the total amount.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the tax type.
    /// </summary>
    public string TaxType { get; set; } = "VAT";

    /// <summary>
    /// Gets or sets the tax rate.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets the line items.
    /// </summary>
    public List<InvoiceLineItem> LineItems { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents a line item in an invoice event
/// </summary>
public class InvoiceLineItem
{
    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the total amount for this line.
    /// </summary>
    public decimal Amount { get; set; }
}
