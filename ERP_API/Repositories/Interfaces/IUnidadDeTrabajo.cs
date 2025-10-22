using ERP_API.Data;
using ERP_API.Repositories.Implementations;

namespace ERP_API.Repositories.Interfaces;

public interface IUnidadDeTrabajo : IDisposable
{
    
    IProductRepository Products { get; }
    ICustomerRepository Customers { get; }
    IOrderRepository Orders { get; }
    IInventoryRepository Inventory { get; }
    ISupplierRepository Suppliers { get; }
    IProductSupplierRepository ProductSuppliers { get; }
    IInvoiceRepository Invoices { get; }
    IInvoicePaymentRepository InvoicePayments { get; }

    IPurchaseOrderRepository PurchaseOrders { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    AppDbContext GetDbContext();
}