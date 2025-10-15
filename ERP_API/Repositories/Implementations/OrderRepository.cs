using ERP_API.Data;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Repositories.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(Guid id) =>
        _db.Orders
           .Include(o => o.Customer)
           .Include(o => o.Items).ThenInclude(i => i.Product)
           .FirstOrDefaultAsync(o => o.Id == id);

    public async Task AddAsync(Order order) { _db.Add(order); await Task.CompletedTask; }

    public Task SaveAsync() => _db.SaveChangesAsync();
}
