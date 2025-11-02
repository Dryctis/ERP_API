using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Services.Interfaces;

namespace ERP_API.Helpers;

public class OrderCalculationHelper
{
    private readonly ITaxCalculator _taxCalculator;

    public OrderCalculationHelper(ITaxCalculator taxCalculator)
    {
        _taxCalculator = taxCalculator;
    }

    public OrderCalculationResult CalculateOrderTotals(
        List<OrderItemCreateDto> items,
        Dictionary<Guid, Product> products)
    {
        var itemCalculations = new List<OrderItemCalculation>();
        decimal subtotal = 0m;

        foreach (var itemDto in items)
        {
            var product = products[itemDto.ProductId];
            var lineTotal = product.Price * itemDto.Quantity;
            subtotal += lineTotal;

            itemCalculations.Add(new OrderItemCalculation
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
                LineTotal = lineTotal
            });
        }

      
        var taxAmount = _taxCalculator.CalculateTax(subtotal, TaxType.IVA);
        var total = subtotal + taxAmount;

        return new OrderCalculationResult
        {
            ItemCalculations = itemCalculations,
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            Total = total
        };
    }
}

public class OrderCalculationResult
{
    public List<OrderItemCalculation> ItemCalculations { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
}


public class OrderItemCalculation
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}