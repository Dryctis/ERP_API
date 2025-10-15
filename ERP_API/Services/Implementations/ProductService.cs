using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;

namespace ERP_API.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize, q, sort);
        var result = items.Select(x => new ProductDto(x.Id, x.Sku, x.Name, x.Price, x.Stock)).ToList();
        return new { total, page, pageSize, items = result };
    }

    public async Task<ProductDto?> GetAsync(Guid id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p is null ? null : new ProductDto(p.Id, p.Sku, p.Name, p.Price, p.Stock);
    }

    public async Task<ProductDto> CreateAsync(ProductCreateDto dto)
    {
        var p = new Product { Sku = dto.Sku, Name = dto.Name, Price = dto.Price, Stock = dto.Stock };
        await _repo.AddAsync(p);
        await _repo.SaveAsync();
        return new ProductDto(p.Id, p.Sku, p.Name, p.Price, p.Stock);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, ProductUpdateDto dto)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p is null) return null;
        p.Sku = dto.Sku; p.Name = dto.Name; p.Price = dto.Price; p.Stock = dto.Stock;
        await _repo.UpdateAsync(p);
        await _repo.SaveAsync();
        return new ProductDto(p.Id, p.Sku, p.Name, p.Price, p.Stock);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p is null) return false;
        await _repo.DeleteAsync(p);
        await _repo.SaveAsync();
        return true;
    }
}
