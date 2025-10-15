using ERP_API.Entities;

namespace ERP_API.Repositories.Interfaces;

public interface IProductRepository
{
    Task<(IReadOnlyList<Product> items, int total)> GetPagedAsync(int page, int pageSize, string? q, string? sort);
    Task<Product?> GetByIdAsync(Guid id);
    Task AddAsync(Product p);
    Task UpdateAsync(Product p);
    Task DeleteAsync(Product p);
    Task SaveAsync();
}
