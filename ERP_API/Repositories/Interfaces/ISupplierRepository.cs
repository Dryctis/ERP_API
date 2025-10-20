using ERP_API.Entities;

namespace ERP_API.Repositories.Interfaces;

public interface ISupplierRepository
{

    Task<(IReadOnlyList<Supplier> items, int total)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm,
        string? sort,
        bool? isActive = null);


    Task<Supplier?> GetByIdAsync(Guid id);


    Task<Supplier?> GetByIdWithProductsAsync(Guid id);


    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);


    Task<bool> ExistsByTaxIdAsync(string taxId, Guid? excludeId = null);


    Task AddAsync(Supplier supplier);

    Task UpdateAsync(Supplier supplier);


    Task DeleteAsync(Supplier supplier);


    Task<(IReadOnlyList<Supplier> items, int total)> GetDeletedPagedAsync(int page, int pageSize);
}


public interface IProductSupplierRepository
{

    Task<ProductSupplier?> GetByIdAsync(Guid id);


    Task<IReadOnlyList<ProductSupplier>> GetByProductIdAsync(Guid productId);


    Task<IReadOnlyList<ProductSupplier>> GetBySupplierId(Guid supplierId);


    Task<bool> ExistsAsync(Guid productId, Guid supplierId, Guid? excludeId = null);


    Task<ProductSupplier?> GetPreferredByProductIdAsync(Guid productId);

    Task AddAsync(ProductSupplier productSupplier);


    Task UpdateAsync(ProductSupplier productSupplier);

    Task DeleteAsync(ProductSupplier productSupplier);
}