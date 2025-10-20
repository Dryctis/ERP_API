using ERP_API.Common.Results;
using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface ISupplierService
{

    Task<object> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm,
        string? sort,
        bool? isActive = null);

    Task<Result<SupplierDto>> GetAsync(Guid id);

    Task<Result<SupplierDto>> GetWithProductsAsync(Guid id);

    Task<Result<SupplierDto>> CreateAsync(SupplierCreateDto dto);

    Task<Result<SupplierDto>> UpdateAsync(Guid id, SupplierUpdateDto dto);

    Task<Result> DeleteAsync(Guid id);

    Task<Result<SupplierDto>> RestoreAsync(Guid id);

    Task<object> GetDeletedAsync(int page, int pageSize);

    Task<Result<ProductSupplierDto>> AssignToProductAsync(ProductSupplierCreateDto dto);


    Task<Result<ProductSupplierDto>> UpdateProductSupplierAsync(Guid id, ProductSupplierUpdateDto dto);

    Task<Result> RemoveFromProductAsync(Guid productSupplierId);

    Task<Result<List<SupplierForProductDto>>> GetSuppliersByProductAsync(Guid productId);

    Task<Result<List<ProductSupplierDto>>> GetProductsBySupplierId(Guid supplierId);

    Task<Result<ProductSupplierDto>> GetProductSupplierByIdAsync(Guid id);
}