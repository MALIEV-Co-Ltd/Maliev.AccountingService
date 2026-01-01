namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Inventory service when stock movement is recorded
/// </summary>
public class InventoryMovementEvent
{
    /// <summary>
    /// Gets or sets the event ID.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the movement ID.
    /// </summary>
    public Guid MovementId { get; set; }

    /// <summary>
    /// Gets or sets the movement number.
    /// </summary>
    public string MovementNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the movement date.
    /// </summary>
    public DateTime MovementDate { get; set; }

    /// <summary>
    /// Gets or sets the movement type (Purchase, Sale, Adjustment).
    /// </summary>
    public string MovementType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit cost.
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Gets or sets the total cost.
    /// </summary>
    public decimal TotalCost { get; set; }

    /// <summary>
    /// Gets or sets the supplier ID.
    /// </summary>
    public Guid? SupplierId { get; set; }

    /// <summary>
    /// Gets or sets the purchase order ID.
    /// </summary>
    public Guid? PurchaseOrderId { get; set; }

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
