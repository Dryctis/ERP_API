using ERP_API.Common.Results;
using ERP_API.DTOs;

namespace ERP_API.Services.Interfaces;

public interface IPurchaseOrderService
{
    Task<object> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? status = null,
        Guid? supplierId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sort = null);

    Task<Result<PurchaseOrderDto>> GetAsync(Guid id);
    Task<Result<PurchaseOrderSummaryDto>> GetSummaryAsync();
    Task<Result<List<OverduePurchaseOrderDto>>> GetOverdueOrdersAsync();
    Task<Result<List<ReorderSuggestionDto>>> GetReorderSuggestionsAsync(int threshold = 10);

    Task<Result<PurchaseOrderDto>> CreateAsync(PurchaseOrderCreateDto dto);
    Task<Result<PurchaseOrderDto>> UpdateAsync(Guid id, PurchaseOrderUpdateDto dto);
    Task<Result> DeleteAsync(Guid id);  

    Task<Result<PurchaseOrderDto>> SendOrderAsync(Guid id);
    Task<Result<PurchaseOrderDto>> ConfirmOrderAsync(Guid id, ConfirmPurchaseOrderDto dto);
    Task<Result<PurchaseOrderDto>> ReceiveItemsAsync(Guid id, ReceiveItemsDto dto);
    Task<Result<PurchaseOrderDto>> CancelOrderAsync(Guid id, string? reason = null);
}