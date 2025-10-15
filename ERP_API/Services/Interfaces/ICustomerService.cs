using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface ICustomerService
{
    Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort);
    Task<CustomerDto?> GetAsync(Guid id);
    Task<(bool ok, string? error, CustomerDto? dto)> CreateAsync(CustomerCreateDto dto);
    Task<(bool ok, string? error, CustomerDto? dto)> UpdateAsync(Guid id, CustomerUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
