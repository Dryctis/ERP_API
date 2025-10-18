using ERP_API.Common.Results;
using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface IProductService
{
    Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort);
    Task<Result<ProductDto>> GetAsync(Guid id);
    Task<Result<ProductDto>> CreateAsync(ProductCreateDto dto);
    Task<Result<ProductDto>> UpdateAsync(Guid id, ProductUpdateDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result<ProductDto>> RestoreAsync(Guid id);
    Task<object> GetDeletedAsync(int page, int pageSize);
}