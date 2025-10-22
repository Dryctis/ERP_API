using ERP_API.Entities;

namespace ERP_API.Repositories.Interfaces;

public interface IPurchaseOrderRepository
{
    Task<(IReadOnlyList<PurchaseOrder> items, int total)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? status = null,
        Guid? supplierId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sort = null);

    Task<PurchaseOrder?> GetByIdAsync(Guid id);
    Task<PurchaseOrder?> GetByIdWithDetailsAsync(Guid id);
    Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber);
    Task<bool> ExistsByOrderNumberAsync(string orderNumber);
    Task<string> GenerateOrderNumberAsync();
    Task<IReadOnlyList<PurchaseOrder>> GetOverdueOrdersAsync();
    Task<Dictionary<PurchaseOrderStatus, int>> GetOrderCountByStatusAsync();
    Task<IReadOnlyList<PurchaseOrder>> GetPendingOrdersBySupplierAsync(Guid supplierId);
    Task AddAsync(PurchaseOrder purchaseOrder);
    Task UpdateAsync(PurchaseOrder purchaseOrder);
    Task DeleteAsync(PurchaseOrder purchaseOrder);
}