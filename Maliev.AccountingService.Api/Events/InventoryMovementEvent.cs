namespace Maliev.AccountingService.Api.Events;

/// <summary>
/// Event published by Inventory service when stock movement is recorded
/// </summary>
public class InventoryMovementEvent
{
    public string EventId { get; set; } = string.Empty;
    public Guid MovementId { get; set; }
    public string MovementNumber { get; set; } = string.Empty;
    public DateTime MovementDate { get; set; }
    public string MovementType { get; set; } = string.Empty; // Purchase, Sale, Adjustment
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}
