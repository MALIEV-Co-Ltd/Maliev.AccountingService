namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Procurement service when a supplier invoice is received
/// </summary>
public class SupplierInvoiceEvent
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
    /// Gets or sets the supplier ID.
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Gets or sets the supplier name.
    /// </summary>
    public string? SupplierName { get; set; }

    /// <summary>
    /// Gets or sets the invoice date.
    /// </summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Gets or sets the due date.
    /// </summary>
    public DateTime DueDate { get; set; }

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
    /// Gets or sets the main expense category.
    /// </summary>
    public string? ExpenseCategory { get; set; }

    /// <summary>
    /// Gets or sets the line items.
    /// </summary>
    public List<SupplierInvoiceLineItem> LineItems { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Represents a line item in a supplier invoice event
/// </summary>
public class SupplierInvoiceLineItem
{
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the amount for this line.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the expense category for this line.
    /// </summary>
    public string? ExpenseCategory { get; set; }
}
