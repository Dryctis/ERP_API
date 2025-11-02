using ERP_API.Common.Configuration;
using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Common.Helpers;


public static class InvoiceCalculationHelper
{

    public static InvoiceItemsCalculationResult CalculateItems(
        IEnumerable<InvoiceItemCreateDto> itemDtos,
        IDictionary<Guid, Product> products)
    {
        if (itemDtos == null) throw new ArgumentNullException(nameof(itemDtos));
        if (products == null) throw new ArgumentNullException(nameof(products));

        var items = new List<InvoiceItem>();
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

            var itemCalculation = CalculateSingleItem(itemDto, product, sortOrder);

            items.Add(itemCalculation.Item);
            subtotal += itemCalculation.Subtotal;
            taxTotal += itemCalculation.TaxAmount;
            sortOrder++;
        }

        return new InvoiceItemsCalculationResult
        {
            Items = items,
            Subtotal = subtotal,
            TaxAmount = taxTotal
        };
    }

    private static SingleItemCalculation CalculateSingleItem(
        InvoiceItemCreateDto itemDto,
        Product product,
        int sortOrder)
    {
        var unitPrice = itemDto.UnitPrice ?? product.Price;
        var lineSubtotal = (itemDto.Quantity * unitPrice) - itemDto.DiscountAmount;
        var lineTax = BusinessConstants.Tax.Calculate(lineSubtotal);
        var lineTotal = lineSubtotal + lineTax;

        var item = new InvoiceItem
        {
            ProductId = itemDto.ProductId,
            Description = itemDto.Description ?? product.Name,
            Quantity = itemDto.Quantity,
            UnitPrice = unitPrice,
            DiscountAmount = itemDto.DiscountAmount,
            Subtotal = lineSubtotal,
            TaxAmount = lineTax,
            LineTotal = lineTotal,
            SortOrder = sortOrder
        };

        return new SingleItemCalculation
        {
            Item = item,
            Subtotal = lineSubtotal,
            TaxAmount = lineTax
        };
    }

 
    public static decimal CalculateTotal(decimal subtotal, decimal taxAmount, decimal discountAmount)
    {
        return Math.Round(
            (subtotal + taxAmount) - discountAmount,
            2,
            MidpointRounding.AwayFromZero);
    }

    private class SingleItemCalculation
    {
        public InvoiceItem Item { get; set; } = default!;
        public decimal Subtotal { get; set; }
        public decimal TaxAmount { get; set; }
    }
}

public class InvoiceItemsCalculationResult
{

    public List<InvoiceItem> Items { get; set; } = new();

    public decimal Subtotal { get; set; }


    public decimal TaxAmount { get; set; }
}