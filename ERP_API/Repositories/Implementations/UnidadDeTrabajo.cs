using ERP_API.Data;
using ERP_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ERP_API.Repositories.Implementations;


public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

   
    private IProductRepository? _products;
    private ICustomerRepository? _customers;
    private IOrderRepository? _orders;
    private IInventoryRepository? _inventory;

    public UnidadDeTrabajo(AppDbContext context)
    {
        _context = context;
    }

    
    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public ICustomerRepository Customers =>
        _customers ??= new CustomerRepository(_context);

    public IOrderRepository Orders =>
        _orders ??= new OrderRepository(_context);

    public IInventoryRepository Inventory =>
        _inventory ??= new InventoryRepository(_context);

    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    
    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Ya existe una transacción activa");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    
    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();

            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    
    public AppDbContext GetDbContext() => _context;

    
    public void Dispose()
    {
        _transaction?.Dispose();
        
    }
}