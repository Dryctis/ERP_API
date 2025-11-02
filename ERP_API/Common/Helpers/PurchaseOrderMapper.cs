using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Common.Helpers;

public static class PurchaseOrderMapper
{

    public static PurchaseOrderDto ToDto(PurchaseOrder po)
    {
        if (po == null) throw new ArgumentNullException(nameof(po));

        return new PurchaseOrderDto(
            Id: po.Id,
            OrderNumber: po.OrderNumber,
            SupplierId: po.SupplierId,
            SupplierName: po.Supplier?.Name ?? "N/A",
            SupplierEmail: po.Supplier?.Email ?? "N/A",
            Status: po.Status.ToString(),
            OrderDate: po.OrderDate,
            ExpectedDeliveryDate: po.ExpectedDeliveryDate,
            ActualDeliveryDate: po.ActualDeliveryDate,
            Subtotal: po.Subtotal,
            TaxAmount: po.TaxAmount,
            DiscountAmount: po.DiscountAmount,
            ShippingCost: po.ShippingCost,
            Total: po.Total,
            PaidAmount: po.PaidAmount,
            Balance: po.Balance,
            PaymentTerms: po.PaymentTerms,
            Notes: po.Notes,
            SupplierReference: po.SupplierReference,
            CreatedAt: po.CreatedAt,
            UpdatedAt: po.UpdatedAt,
            Items: po.Items?.Select(ToItemDto).OrderBy(i => i.Id).ToList() ?? new List<PurchaseOrderItemDto>(),
            CanBeEdited: po.CanBeEdited(),
            CanBeSent: po.CanBeSent(),
            CanBeCancelled: po.CanBeCancelled(),
            CanReceiveItems: po.CanReceiveItems(),
            IsFullyReceived: po.IsFullyReceived()
        );
    }

    private static PurchaseOrderItemDto ToItemDto(PurchaseOrderItem item)
    {
        return new PurchaseOrderItemDto(
            Id: item.Id,
            ProductId: item.ProductId,
            ProductName: item.Product?.Name ?? "N/A",
            ProductSku: item.Product?.Sku ?? "N/A",
            Description: item.Description,
            SupplierSku: item.SupplierSku,
            OrderedQuantity: item.OrderedQuantity,
            ReceivedQuantity: item.ReceivedQuantity,
            PendingQuantity: item.PendingQuantity,
            UnitCost: item.UnitCost,
            DiscountAmount: item.DiscountAmount,
            Subtotal: item.Subtotal,
            TaxAmount: item.TaxAmount,
            LineTotal: item.LineTotal,
            ReceivedPercentage: item.GetReceivedPercentage()
        );
    }
}