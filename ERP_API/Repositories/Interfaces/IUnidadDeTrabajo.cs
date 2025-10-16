using ERP_API.Data;

namespace ERP_API.Repositories.Interfaces;


public interface IUnidadDeTrabajo : IDisposable
{
    
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    IOrderRepository Orders { get; }
    IInventoryRepository Inventory { get; }

    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    AppDbContext GetDbContext();
}