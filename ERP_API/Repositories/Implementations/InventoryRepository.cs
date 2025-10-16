using ERP_API.Data;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Repositories.Implementations;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryMovement>> GetAllAsync()
    {
        return await _context.InventoryMovements
            .Include(m => m.Product)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<InventoryMovement?> GetByIdAsync(Guid id)
    {
        return await _context.InventoryMovements
            .Include(m => m.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(InventoryMovement movement)
    {
        await _context.InventoryMovements.AddAsync(movement);
    }

    
}