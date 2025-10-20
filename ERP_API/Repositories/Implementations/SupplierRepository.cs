using ERP_API.Data;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Repositories.Implementations;

public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _db;

    public SupplierRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Supplier> items, int total)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm,
        string? sort,
        bool? isActive = null)
    {
        var query = _db.Set<Supplier>()
            .Include(s => s.ProductSuppliers)
            .AsNoTracking();

        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                s.Email.ToLower().Contains(search) ||
                (s.ContactName != null && s.ContactName.ToLower().Contains(search)) ||
                (s.TaxId != null && s.TaxId.ToLower().Contains(search)) ||
                (s.City != null && s.City.ToLower().Contains(search)) ||
                (s.Country != null && s.Country.ToLower().Contains(search))
            );
        }

        
        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

       
        query = sort?.ToLower() switch
        {
            "name:desc" => query.OrderByDescending(s => s.Name),
            "email:asc" => query.OrderBy(s => s.Email),
            "email:desc" => query.OrderByDescending(s => s.Email),
            "city:asc" => query.OrderBy(s => s.City),
            "city:desc" => query.OrderByDescending(s => s.City),
            "country:asc" => query.OrderBy(s => s.Country),
            "country:desc" => query.OrderByDescending(s => s.Country),
            "createdat:asc" => query.OrderBy(s => s.CreatedAt),
            "createdat:desc" => query.OrderByDescending(s => s.CreatedAt),
            _ => query.OrderBy(s => s.Name) 
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Supplier?> GetByIdAsync(Guid id)
    {
        return await _db.Set<Supplier>()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Supplier?> GetByIdWithProductsAsync(Guid id)
    {
        return await _db.Set<Supplier>()
            .Include(s => s.ProductSuppliers)
                .ThenInclude(ps => ps.Product)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null)
    {
        var query = _db.Set<Supplier>()
            .Where(s => s.Email.ToLower() == email.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByTaxIdAsync(string taxId, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(taxId))
            return false;

        var query = _db.Set<Supplier>()
            .Where(s => s.TaxId != null && s.TaxId.ToLower() == taxId.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(Supplier supplier)
    {
        await _db.Set<Supplier>().AddAsync(supplier);
    }

    public Task UpdateAsync(Supplier supplier)
    {
        supplier.UpdatedAt = DateTime.UtcNow;
        _db.Set<Supplier>().Update(supplier);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Supplier supplier)
    {
        _db.Set<Supplier>().Remove(supplier);
        return Task.CompletedTask;
    }

    public async Task<(IReadOnlyList<Supplier> items, int total)> GetDeletedPagedAsync(
        int page,
        int pageSize)
    {
        var query = _db.Set<Supplier>()
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted)
            .OrderByDescending(s => s.DeletedAt);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}


public class ProductSupplierRepository : IProductSupplierRepository
{
    private readonly AppDbContext _db;

    public ProductSupplierRepository(AppDbContext db) => _db = db;

    public async Task<ProductSupplier?> GetByIdAsync(Guid id)
    {
        return await _db.Set<ProductSupplier>()
            .Include(ps => ps.Product)
            .Include(ps => ps.Supplier)
            .FirstOrDefaultAsync(ps => ps.Id == id);
    }

    public async Task<IReadOnlyList<ProductSupplier>> GetByProductIdAsync(Guid productId)
    {
        return await _db.Set<ProductSupplier>()
            .Include(ps => ps.Supplier)
            .Where(ps => ps.ProductId == productId)
            .OrderByDescending(ps => ps.IsPreferred)
            .ThenBy(ps => ps.SupplierPrice)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ProductSupplier>> GetBySupplierId(Guid supplierId)
    {
        return await _db.Set<ProductSupplier>()
            .Include(ps => ps.Product)
            .Where(ps => ps.SupplierId == supplierId)
            .OrderBy(ps => ps.Product.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid productId, Guid supplierId, Guid? excludeId = null)
    {
        var query = _db.Set<ProductSupplier>()
            .Where(ps => ps.ProductId == productId && ps.SupplierId == supplierId);

        if (excludeId.HasValue)
        {
            query = query.Where(ps => ps.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<ProductSupplier?> GetPreferredByProductIdAsync(Guid productId)
    {
        return await _db.Set<ProductSupplier>()
            .Include(ps => ps.Supplier)
            .Where(ps => ps.ProductId == productId && ps.IsPreferred)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(ProductSupplier productSupplier)
    {
        await _db.Set<ProductSupplier>().AddAsync(productSupplier);
    }

    public Task UpdateAsync(ProductSupplier productSupplier)
    {
        productSupplier.UpdatedAt = DateTime.UtcNow;
        _db.Set<ProductSupplier>().Update(productSupplier);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProductSupplier productSupplier)
    {
        _db.Set<ProductSupplier>().Remove(productSupplier);
        return Task.CompletedTask;
    }
}