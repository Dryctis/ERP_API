using ERP_API.Common.Results;
using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface IOrderService
{
    Task<Result<OrderDto>> GetAsync(Guid id);
    Task<Result<OrderDto>> CreateAsync(OrderCreateDto dto);
}