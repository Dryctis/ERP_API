using ERP_API.Entities;

namespace ERP_API.Repositories.Interfaces;

public interface IInvoiceRepository
{

    Task<(IReadOnlyList<Invoice> items, int total)> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? status = null,
        Guid? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sort = null);


    Task<Invoice?> GetByIdAsync(Guid id);

    Task<Invoice?> GetByIdWithDetailsAsync(Guid id);

    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);

    Task<bool> ExistsForOrderAsync(Guid orderId);

    Task<Invoice?> GetByOrderIdAsync(Guid orderId);


    Task<string> GenerateInvoiceNumberAsync();


    Task<IReadOnlyList<Invoice>> GetOverdueInvoicesAsync();


    Task<Dictionary<InvoiceStatus, int>> GetInvoiceCountByStatusAsync();

    Task AddAsync(Invoice invoice);


    Task UpdateAsync(Invoice invoice);

    Task DeleteAsync(Invoice invoice);
}


public interface IInvoicePaymentRepository
{

    Task<IReadOnlyList<InvoicePayment>> GetByInvoiceIdAsync(Guid invoiceId);

 
    Task<InvoicePayment?> GetByIdAsync(Guid id);

  
    Task AddAsync(InvoicePayment payment);

  
    Task DeleteAsync(InvoicePayment payment);

  
    Task<decimal> GetTotalPaidAsync(Guid invoiceId);
}