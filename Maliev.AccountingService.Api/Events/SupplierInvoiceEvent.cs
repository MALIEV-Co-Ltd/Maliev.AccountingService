namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Procurement service when a supplier invoice is received
/// </summary>
public class SupplierInvoiceEvent
{
    public string EventId { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string TaxType { get; set; } = "VAT";
    public decimal TaxRate { get; set; }
    public string? ExpenseCategory { get; set; }
    public List<SupplierInvoiceLineItem> LineItems { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class SupplierInvoiceLineItem
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ExpenseCategory { get; set; }
}
