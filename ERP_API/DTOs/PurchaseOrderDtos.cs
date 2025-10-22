namespace ERP_API.DTOs;

public record PurchaseOrderDto(
    Guid Id,
    string OrderNumber,
    Guid SupplierId,
    string SupplierName,
    string SupplierEmail,
    string Status,
    DateTime OrderDate,
    DateTime ExpectedDeliveryDate,
    DateTime? ActualDeliveryDate,
    decimal Subtotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal ShippingCost,
    decimal Total,
    decimal PaidAmount,
    decimal Balance,
    string? PaymentTerms,
    string? Notes,
    string? SupplierReference,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<PurchaseOrderItemDto> Items,
    bool CanBeEdited,
    bool CanBeSent,
    bool CanBeCancelled,
    bool CanReceiveItems,
    bool IsFullyReceived
);

public record PurchaseOrderListDto(
    Guid Id,
    string OrderNumber,
    string SupplierName,
    string Status,
    DateTime OrderDate,
    DateTime ExpectedDeliveryDate,
    decimal Total,
    decimal Balance,
    int ItemCount,
    bool IsFullyReceived
);

public record PurchaseOrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    string Description,
    string? SupplierSku,
    int OrderedQuantity,
    int ReceivedQuantity,
    int PendingQuantity,
    decimal UnitCost,
    decimal DiscountAmount,
    decimal Subtotal,
    decimal TaxAmount,
    decimal LineTotal,
    decimal ReceivedPercentage
);

public record PurchaseOrderCreateDto(
    Guid SupplierId,
    DateTime? OrderDate = null,
    DateTime? ExpectedDeliveryDate = null,
    decimal DiscountAmount = 0,
    decimal ShippingCost = 0,
    string? PaymentTerms = null,
    string? Notes = null,
    string? SupplierReference = null,
    List<PurchaseOrderItemCreateDto> Items = default!
);

public record PurchaseOrderItemCreateDto(
    Guid ProductId,
    string? Description = null,
    string? SupplierSku = null,
    int OrderedQuantity = 1,
    decimal? UnitCost = null,
    decimal DiscountAmount = 0,
    string? Notes = null
);

public record PurchaseOrderUpdateDto(
    DateTime OrderDate,
    DateTime ExpectedDeliveryDate,
    decimal DiscountAmount,
    decimal ShippingCost,
    string? PaymentTerms,
    string? Notes,
    string? SupplierReference,
    List<PurchaseOrderItemCreateDto> Items
);


public record ReceiveItemsDto(
    List<ReceiveItemDto> Items,
    string? Notes = null
);


public record ReceiveItemDto(
    Guid PurchaseOrderItemId,
    int ReceivedQuantity,
    string? Notes = null
);


public record ConfirmPurchaseOrderDto(
    string? SupplierReference = null,
    DateTime? ExpectedDeliveryDate = null,
    string? Notes = null
);

public record PurchaseOrderSummaryDto(
    int TotalOrders,
    int DraftOrders,
    int SentOrders,
    int ConfirmedOrders,
    int PartiallyReceivedOrders,
    int ReceivedOrders,
    int CancelledOrders,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalOutstanding,
    int OverdueOrders
);

public record OverduePurchaseOrderDto(
    Guid Id,
    string OrderNumber,
    string SupplierName,
    DateTime OrderDate,
    DateTime ExpectedDeliveryDate,
    int DaysOverdue,
    decimal Total,
    decimal Balance,
    string Status
);

public record ReorderSuggestionDto(
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int CurrentStock,
    int MinimumStock,
    int SuggestedOrderQuantity,
    Guid? PreferredSupplierId,
    string? PreferredSupplierName,
    decimal? EstimatedCost
);