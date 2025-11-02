using ERP_API.Common.Configuration;
using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Common.Helpers;


public static class PurchaseOrderCalculationHelper
{

    public static PurchaseOrderItemsCalculationResult CalculateItems(
        IEnumerable<PurchaseOrderItemCreateDto> itemDtos,
        IDictionary<Guid, Product> products,
        Guid supplierId)
    {
        if (itemDtos == null) throw new ArgumentNullException(nameof(itemDtos));
        if (products == null) throw new ArgumentNullException(nameof(products));

        var items = new List<PurchaseOrderItem>();
        decimal subtotal = 0;
        decimal taxTotal = 0;
        int sortOrder = 1;

        foreach (var itemDto in itemDtos)
        {
            if (!products.TryGetValue(itemDto.ProductId, out var product))
            {
                throw new InvalidOperationException(
                    $"Product {itemDto.ProductId} not found in the provided dictionary");
            }

            var itemCalculation = CalculateSingleItem(itemDto, product, supplierId, sortOrder);

            items.Add(itemCalculation.Item);
            subtotal += itemCalculation.Subtotal;
            taxTotal += itemCalculation.TaxAmount;
            sortOrder++;
        }

        return new PurchaseOrderItemsCalculationResult
        {
            Items = items,
            Subtotal = subtotal,
            TaxAmount = taxTotal
        };
    }

    private static SinglePurchaseOrderItemCalculation CalculateSingleItem(
        PurchaseOrderItemCreateDto itemDto,
        Product product,
        Guid supplierId,
        int sortOrder)
    {
        var unitCost = itemDto.UnitCost ?? GetSupplierCost(product, supplierId);
        var lineSubtotal = (itemDto.OrderedQuantity * unitCost) - itemDto.DiscountAmount;
        var lineTax = BusinessConstants.Tax.Calculate(lineSubtotal);
        var lineTotal = lineSubtotal + lineTax;

        var item = new PurchaseOrderItem
        {
            ProductId = itemDto.ProductId,
            Description = itemDto.Description ?? product.Name,
            SupplierSku = itemDto.SupplierSku,
            OrderedQuantity = itemDto.OrderedQuantity,
            ReceivedQuantity = 0,
            UnitCost = unitCost,
            DiscountAmount = itemDto.DiscountAmount,
            Subtotal = lineSubtotal,
            TaxAmount = lineTax,
            LineTotal = lineTotal,
            SortOrder = sortOrder,
            Notes = itemDto.Notes
        };

        return new SinglePurchaseOrderItemCalculation
        {
            Item = item,
            Subtotal = lineSubtotal,
            TaxAmount = lineTax
        };
    }

    public static decimal CalculateTotal(
        decimal subtotal,
        decimal taxAmount,
        decimal discountAmount,
        decimal shippingCost)
    {
        return Math.Round(
            (subtotal + taxAmount + shippingCost) - discountAmount,
            2,
            MidpointRounding.AwayFromZero);
    }

 
    private static decimal GetSupplierCost(Product product, Guid supplierId)
    {
        var supplierRelation = product.ProductSuppliers
            ?.FirstOrDefault(ps => ps.SupplierId == supplierId);

        return supplierRelation?.SupplierPrice ?? product.Price;
    }


    private class SinglePurchaseOrderItemCalculation
    {
        public PurchaseOrderItem Item { get; set; } = default!;
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
    }
}


public class PurchaseOrderItemsCalculationResult
{

    public List<PurchaseOrderItem> Items { get; set; } = new();

  
    public decimal Subtotal { get; set; }

 
    public decimal TaxAmount { get; set; }
}