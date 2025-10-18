using ERP_API.Common.Results;
using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface ICustomerService
{
    Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort);
    Task<Result<CustomerDto>> GetAsync(Guid id);
    Task<Result<CustomerDto>> CreateAsync(CustomerCreateDto dto);
    Task<Result<CustomerDto>> UpdateAsync(Guid id, CustomerUpdateDto dto);
    Task<Result> DeleteAsync(Guid id);
}