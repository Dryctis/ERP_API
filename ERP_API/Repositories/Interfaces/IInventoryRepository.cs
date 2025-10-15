using ERP_API.Entities;

namespace ERP_API.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<InventoryMovement>> GetAllAsync();
        Task<InventoryMovement?> GetByIdAsync(Guid id);
        Task AddAsync(InventoryMovement movement);
        Task SaveChangesAsync();
    }
}
