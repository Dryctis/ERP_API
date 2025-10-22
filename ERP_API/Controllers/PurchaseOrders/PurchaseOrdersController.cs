using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers.V1;


[ApiController]
[Route("api/v1/purchase-orders")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service) => _service = service;


    [HttpGet]
    public Task<object> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sort = "orderdate:desc")
        => _service.GetPagedAsync(page, pageSize, q, status, supplierId, fromDate, toDate, sort);


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderDto>> Get(Guid id)
    {
        var result = await _service.GetAsync(id);
        return result.ToActionResult();
    }


    [HttpGet("summary")]
    public async Task<ActionResult<PurchaseOrderSummaryDto>> GetSummary()
    {
        var result = await _service.GetSummaryAsync();
        return result.ToActionResult();
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<List<OverduePurchaseOrderDto>>> GetOverdue()
    {
        var result = await _service.GetOverdueOrdersAsync();
        return result.ToActionResult();
    }


    [HttpGet("reorder-suggestions")]
    public async Task<ActionResult<List<ReorderSuggestionDto>>> GetReorderSuggestions(
        [FromQuery] int threshold = 10)
    {
        var result = await _service.GetReorderSuggestionsAsync(threshold);
        return result.ToActionResult();
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<PurchaseOrderDto>> Create(PurchaseOrderCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(Get),
                new { id = result.Value!.Id },
                result.Value
            );
        }

        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderDto>> Update(Guid id, PurchaseOrderUpdateDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _service.DeleteAsync(id);
        return result.ToNoContentResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/send")]
    public async Task<ActionResult<PurchaseOrderDto>> Send(Guid id)
    {
        var result = await _service.SendOrderAsync(id);
        return result.ToActionResult();
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<PurchaseOrderDto>> Confirm(Guid id, ConfirmPurchaseOrderDto dto)
    {
        var result = await _service.ConfirmOrderAsync(id, dto);
        return result.ToActionResult();
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/receive")]
    public async Task<ActionResult<PurchaseOrderDto>> ReceiveItems(Guid id, ReceiveItemsDto dto)
    {
        var result = await _service.ReceiveItemsAsync(id, dto);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<PurchaseOrderDto>> Cancel(Guid id, [FromBody] string? reason = null)
    {
        var result = await _service.CancelOrderAsync(id, reason);
        return result.ToActionResult();
    }
}