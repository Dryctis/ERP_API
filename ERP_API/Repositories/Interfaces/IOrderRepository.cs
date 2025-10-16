using ERP_API.Entities;

namespace ERP_API.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task AddAsync(Order order);
    //Task SaveAsync();
}
