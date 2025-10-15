using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface IProductService
{
    Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort);
    Task<ProductDto?> GetAsync(Guid id);
    Task<ProductDto> CreateAsync(ProductCreateDto dto);
    Task<ProductDto?> UpdateAsync(Guid id, ProductUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);
}
