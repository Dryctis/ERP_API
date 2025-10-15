namespace ERP_API.DTOs;

public record OrderItemCreateDto(Guid ProductId, int Quantity);
public record OrderCreateDto(Guid CustomerId, List<OrderItemCreateDto> Items);

public record OrderItemDto(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
public record OrderDto(Guid Id, Guid CustomerId, string CustomerName, decimal Subtotal, decimal Tax, decimal Total, DateTime CreatedAt, List<OrderItemDto> Items);
