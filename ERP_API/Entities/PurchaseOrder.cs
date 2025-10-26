using System.ComponentModel.DataAnnotations;
using ERP_API.Common.Entities;

namespace ERP_API.Entities;

public class PurchaseOrder : ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = default!;

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance => Total - PaidAmount;

    [MaxLength(500)]
    public string? PaymentTerms { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? SupplierReference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();

    public bool CanBeEdited() => Status == PurchaseOrderStatus.Draft;
    public bool CanBeSent() => Status == PurchaseOrderStatus.Draft && Items.Any();
    public bool CanBeCancelled() => Status != PurchaseOrderStatus.Received && Status != PurchaseOrderStatus.Cancelled;
    public bool CanReceiveItems() => Status == PurchaseOrderStatus.Confirmed || Status == PurchaseOrderStatus.PartiallyReceived;
    public bool IsFullyReceived() => Items.All(i => i.IsFullyReceived());

    public void UpdateStatusBasedOnReceipts()
    {
        if (Status == PurchaseOrderStatus.Cancelled || Status == PurchaseOrderStatus.Draft)
            return;

        if (IsFullyReceived())
        {
            Status = PurchaseOrderStatus.Received;
            ActualDeliveryDate = DateTime.UtcNow;
        }
        else if (Items.Any(i => i.ReceivedQuantity > 0))
        {
            Status = PurchaseOrderStatus.PartiallyReceived;
        }
    }
}