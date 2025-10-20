using ERP_API.Data;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Repositories.Implementations;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _db;

    public InvoiceRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Invoice> items, int total)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? status = null,
        Guid? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sort = null)
    {
        var query = _db.Set<Invoice>()
            .Include(i => i.Customer)
            .Include(i => i.Order)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(search) ||
                i.Customer.Name.ToLower().Contains(search) ||
                i.Customer.Email.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<InvoiceStatus>(status, true, out var invoiceStatus))
        {
            query = query.Where(i => i.Status == invoiceStatus);
        }

        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(i => i.IssueDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(i => i.IssueDate <= toDate.Value);
        }

        query = sort?.ToLower() switch
        {
            "number:desc" => query.OrderByDescending(i => i.InvoiceNumber),
            "customer:asc" => query.OrderBy(i => i.Customer.Name),
            "customer:desc" => query.OrderByDescending(i => i.Customer.Name),
            "date:asc" => query.OrderBy(i => i.IssueDate),
            "date:desc" => query.OrderByDescending(i => i.IssueDate),
            "duedate:asc" => query.OrderBy(i => i.DueDate),
            "duedate:desc" => query.OrderByDescending(i => i.DueDate),
            "total:asc" => query.OrderBy(i => i.Total),
            "total:desc" => query.OrderByDescending(i => i.Total),
            "status:asc" => query.OrderBy(i => i.Status),
            "status:desc" => query.OrderByDescending(i => i.Status),
            _ => query.OrderByDescending(i => i.CreatedAt)
        };

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await _db.Set<Invoice>()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invoice?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _db.Set<Invoice>()
            .Include(i => i.Customer)
            .Include(i => i.Order)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        return await _db.Set<Invoice>()
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<bool> ExistsForOrderAsync(Guid orderId)
    {
        return await _db.Set<Invoice>()
            .AnyAsync(i => i.OrderId == orderId);
    }

    public async Task<Invoice?> GetByOrderIdAsync(Guid orderId)
    {
        return await _db.Set<Invoice>()
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.OrderId == orderId);
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var lastInvoice = await _db.Set<Invoice>()
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastInvoice != null)
        {
            var numberPart = lastInvoice.InvoiceNumber.Replace(prefix, "");
            if (int.TryParse(numberPart, out var currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"{prefix}{nextNumber:D4}";
    }

    public async Task<IReadOnlyList<Invoice>> GetOverdueInvoicesAsync()
    {
        var now = DateTime.UtcNow;

        return await _db.Set<Invoice>()
            .Include(i => i.Customer)
            .Where(i =>
                i.DueDate < now &&
                i.Status != InvoiceStatus.Paid &&
                i.Status != InvoiceStatus.Cancelled)
            .OrderBy(i => i.DueDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Dictionary<InvoiceStatus, int>> GetInvoiceCountByStatusAsync()
    {
        return await _db.Set<Invoice>()
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    public async Task AddAsync(Invoice invoice)
    {
        await _db.Set<Invoice>().AddAsync(invoice);
    }

    public Task UpdateAsync(Invoice invoice)
    {
        invoice.UpdatedAt = DateTime.UtcNow;
        _db.Set<Invoice>().Update(invoice);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Invoice invoice)
    {
        _db.Set<Invoice>().Remove(invoice);
        return Task.CompletedTask;
    }
}

public class InvoicePaymentRepository : IInvoicePaymentRepository
{
    private readonly AppDbContext _db;

    public InvoicePaymentRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<InvoicePayment>> GetByInvoiceIdAsync(Guid invoiceId)
    {
        return await _db.Set<InvoicePayment>()
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<InvoicePayment?> GetByIdAsync(Guid id)
    {
        return await _db.Set<InvoicePayment>()
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(InvoicePayment payment)
    {
        await _db.Set<InvoicePayment>().AddAsync(payment);
    }

    public Task DeleteAsync(InvoicePayment payment)
    {
        _db.Set<InvoicePayment>().Remove(payment);
        return Task.CompletedTask;
    }

    public async Task<decimal> GetTotalPaidAsync(Guid invoiceId)
    {
        return await _db.Set<InvoicePayment>()
            .Where(p => p.InvoiceId == invoiceId)
            .SumAsync(p => p.Amount);
    }
}