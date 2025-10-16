using ERP_API.Data;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Repositories.Implementations;


public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Customer> items, int total)> GetPagedAsync(
        int page, int pageSize, string? q, string? sort)
    {
        var query = _db.Customers.AsNoTracking();

        
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                x.Name.Contains(q) ||
                x.Email.Contains(q) ||
                (x.Phone != null && x.Phone.Contains(q)));
        }

       
        query = (sort?.ToLower()) switch
        {
            "name:desc" => query.OrderByDescending(x => x.Name),
            "email:asc" => query.OrderBy(x => x.Email),
            "email:desc" => query.OrderByDescending(x => x.Email),
            "createdat:asc" => query.OrderBy(x => x.CreatedAt),
            "createdat:desc" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderBy(x => x.Name)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<Customer?> GetByIdAsync(Guid id) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null) =>
        _db.Customers.AnyAsync(c =>
            c.Email == email &&
            (excludeId == null || c.Id != excludeId.Value));

    public async Task AddAsync(Customer c)
    {
        await _db.Customers.AddAsync(c);
    }

    public Task UpdateAsync(Customer c)
    {
        _db.Customers.Update(c);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Customer c)
    {
        _db.Customers.Remove(c);
        return Task.CompletedTask;
    }

   
}