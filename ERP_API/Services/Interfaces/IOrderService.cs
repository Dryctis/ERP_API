using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetAsync(Guid id);
    Task<(bool ok, string? error, OrderDto? dto)> CreateAsync(OrderCreateDto dto);
}
