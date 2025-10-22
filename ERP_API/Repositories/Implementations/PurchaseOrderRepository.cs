using ERP_API.Data;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Repositories.Implementations;

public class PurchaseOrderRepository : IPurchaseOrderRepository
{
    private readonly AppDbContext _db;

    public PurchaseOrderRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<PurchaseOrder> items, int total)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? status = null,
        Guid? supplierId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sort = null)
    {
        var query = _db.Set<PurchaseOrder>()
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(po =>
                po.OrderNumber.ToLower().Contains(search) ||
                po.Supplier.Name.ToLower().Contains(search) ||
                (po.SupplierReference != null && po.SupplierReference.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<PurchaseOrderStatus>(status, true, out var poStatus))
        {
            query = query.Where(po => po.Status == poStatus);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(po => po.SupplierId == supplierId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(po => po.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(po => po.OrderDate <= toDate.Value);
        }

        query = sort?.ToLower() switch
        {
            "ordernumber:desc" => query.OrderByDescending(po => po.OrderNumber),
            "supplier:asc" => query.OrderBy(po => po.Supplier.Name),
            "supplier:desc" => query.OrderByDescending(po => po.Supplier.Name),
            "orderdate:asc" => query.OrderBy(po => po.OrderDate),
            "orderdate:desc" => query.OrderByDescending(po => po.OrderDate),
            "deliverydate:asc" => query.OrderBy(po => po.ExpectedDeliveryDate),
            "deliverydate:desc" => query.OrderByDescending(po => po.ExpectedDeliveryDate),
            "total:asc" => query.OrderBy(po => po.Total),
            "total:desc" => query.OrderByDescending(po => po.Total),
            "status:asc" => query.OrderBy(po => po.Status),
            "status:desc" => query.OrderByDescending(po => po.Status),
            _ => query.OrderByDescending(po => po.CreatedAt)
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<PurchaseOrder?> GetByIdAsync(Guid id)
    {
        return await _db.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<PurchaseOrder?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _db.Set<PurchaseOrder>()
            .Include(po => po.Supplier)
            .Include(po => po.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber)
    {
        return await _db.Set<PurchaseOrder>()
            .FirstOrDefaultAsync(po => po.OrderNumber == orderNumber);
    }

    public async Task<bool> ExistsByOrderNumberAsync(string orderNumber)
    {
        return await _db.Set<PurchaseOrder>()
            .AnyAsync(po => po.OrderNumber == orderNumber);
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PO-{year}-";

        var lastOrder = await _db.Set<PurchaseOrder>()
            .Where(po => po.OrderNumber.StartsWith(prefix))
            .OrderByDescending(po => po.OrderNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastOrder != null)
        {
            var numberPart = lastOrder.OrderNumber.Replace(prefix, "");
            if (int.TryParse(numberPart, out var currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetOverdueOrdersAsync()
    {
        var now = DateTime.UtcNow;

        return await _db.Set<PurchaseOrder>()
            .Include(po => po.Supplier)
            .Where(po =>
                po.ExpectedDeliveryDate < now &&
                po.Status != PurchaseOrderStatus.Received &&
                po.Status != PurchaseOrderStatus.Cancelled)
            .OrderBy(po => po.ExpectedDeliveryDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Dictionary<PurchaseOrderStatus, int>> GetOrderCountByStatusAsync()
    {
        return await _db.Set<PurchaseOrder>()
            .GroupBy(po => po.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetPendingOrdersBySupplierAsync(Guid supplierId)
    {
        return await _db.Set<PurchaseOrder>()
            .Include(po => po.Items)
            .Where(po =>
                po.SupplierId == supplierId &&
                (po.Status == PurchaseOrderStatus.Confirmed ||
                 po.Status == PurchaseOrderStatus.PartiallyReceived))
            .OrderBy(po => po.ExpectedDeliveryDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(PurchaseOrder purchaseOrder)
    {
        await _db.Set<PurchaseOrder>().AddAsync(purchaseOrder);
    }

    public Task UpdateAsync(PurchaseOrder purchaseOrder)
    {
        purchaseOrder.UpdatedAt = DateTime.UtcNow;
        _db.Set<PurchaseOrder>().Update(purchaseOrder);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PurchaseOrder purchaseOrder)
    {
        _db.Set<PurchaseOrder>().Remove(purchaseOrder);
        return Task.CompletedTask;
    }
}