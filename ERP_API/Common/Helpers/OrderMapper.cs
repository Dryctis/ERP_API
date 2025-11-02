using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Helpers;

public class OrderMapper
{
   
    public OrderDto MapToDto(Order order)
    {
        return new OrderDto(
            Id: order.Id,
            CustomerId: order.CustomerId,
            CustomerName: order.Customer.Name,
            Subtotal: order.Subtotal,
            Tax: order.Tax,
            Total: order.Total,
            CreatedAt: order.CreatedAt,
            Items: order.Items.Select(MapItemToDto).ToList()
        );
    }

    private OrderItemDto MapItemToDto(OrderItem item)
    {
        return new OrderItemDto(
            ProductId: item.ProductId,
            ProductName: item.Product.Name,
            UnitPrice: item.UnitPrice,
            Quantity: item.Quantity,
            LineTotal: item.LineTotal
        );
    }
}