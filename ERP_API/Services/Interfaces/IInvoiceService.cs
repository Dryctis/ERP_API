using ERP_API.Common.Results;
using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;


public interface IInvoiceService
{
 
    Task<object> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? status = null,
        Guid? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sort = null);


    Task<Result<InvoiceDto>> GetAsync(Guid id);

    Task<Result<InvoiceDto>> CreateFromOrderAsync(CreateInvoiceFromOrderDto dto);


    Task<Result<InvoiceDto>> CreateAsync(InvoiceCreateDto dto);


    Task<Result<InvoiceDto>> UpdateAsync(Guid id, InvoiceUpdateDto dto);


    Task<Result> DeleteAsync(Guid id);

    Task<Result<InvoiceDto>> SendInvoiceAsync(Guid id);

    Task<Result<InvoiceDto>> CancelInvoiceAsync(Guid id);



    Task<Result<InvoiceDto>> AddPaymentAsync(Guid invoiceId, InvoicePaymentCreateDto dto);


    Task<Result<List<InvoicePaymentDto>>> GetPaymentsAsync(Guid invoiceId);

    Task<Result> DeletePaymentAsync(Guid invoiceId, Guid paymentId);

 

    
    Task<Result<List<OverdueInvoiceDto>>> GetOverdueInvoicesAsync();

   
    Task<Result<InvoiceSummaryDto>> GetSummaryAsync();
}