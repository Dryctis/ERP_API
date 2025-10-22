using System.ComponentModel.DataAnnotations;

namespace ERP_API.Entities;

public class PurchaseOrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SupplierSku { get; set; }

    public int OrderedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public int PendingQuantity => OrderedQuantity - ReceivedQuantity;

    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public int SortOrder { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsFullyReceived() => ReceivedQuantity >= OrderedQuantity;
    public bool HasPending() => ReceivedQuantity < OrderedQuantity;
    public decimal GetReceivedPercentage() => OrderedQuantity > 0 ? (decimal)ReceivedQuantity / OrderedQuantity * 100 : 0;
}