using ERP_API.Data;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Repositories.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Product>, int)> GetPagedAsync(int page, int pageSize, string? q, string? sort)
    {
        var query = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.Name.Contains(q) || x.Sku.Contains(q));

        // sort básico
        query = sort?.ToLower() switch
        {
            "name:desc" => query.OrderByDescending(x => x.Name),
            "price:asc" => query.OrderBy(x => x.Price),
            "price:desc" => query.OrderByDescending(x => x.Price),
            _ => query.OrderBy(x => x.Name)
        };

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    public Task<Product?> GetByIdAsync(Guid id) => _db.Products.FirstOrDefaultAsync(p => p.Id == id);
    public async Task AddAsync(Product p) { _db.Add(p); await Task.CompletedTask; }
    public Task UpdateAsync(Product p) { _db.Update(p); return Task.CompletedTask; }
    public Task DeleteAsync(Product p) { _db.Remove(p); return Task.CompletedTask; }
    public Task SaveAsync() => _db.SaveChangesAsync();
}
