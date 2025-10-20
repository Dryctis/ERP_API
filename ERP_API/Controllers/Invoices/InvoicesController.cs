using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers.V1;


[ApiController]
[Route("api/v1/invoices")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _svc;

    public InvoicesController(IInvoiceService svc) => _svc = svc;


    [HttpGet]
    public Task<object> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sort = "date:desc")
        => _svc.GetPagedAsync(page, pageSize, q, status, customerId, fromDate, toDate, sort);


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> Get(Guid id)
    {
        var result = await _svc.GetAsync(id);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("from-order")]
    public async Task<ActionResult<InvoiceDto>> CreateFromOrder(CreateInvoiceFromOrderDto dto)
    {
        var result = await _svc.CreateFromOrderAsync(dto);

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
    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create(InvoiceCreateDto dto)
    {
        var result = await _svc.CreateAsync(dto);

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
    public async Task<ActionResult<InvoiceDto>> Update(Guid id, InvoiceUpdateDto dto)
    {
        var result = await _svc.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _svc.DeleteAsync(id);
        return result.ToNoContentResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/send")]
    public async Task<ActionResult<InvoiceDto>> Send(Guid id)
    {
        var result = await _svc.SendInvoiceAsync(id);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<InvoiceDto>> Cancel(Guid id)
    {
        var result = await _svc.CancelInvoiceAsync(id);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult<InvoiceDto>> AddPayment(Guid id, InvoicePaymentCreateDto dto)
    {
        var result = await _svc.AddPaymentAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}/payments")]
    public async Task<ActionResult<List<InvoicePaymentDto>>> GetPayments(Guid id)
    {
        var result = await _svc.GetPaymentsAsync(id);
        return result.ToActionResult();
    }


    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}/payments/{paymentId:guid}")]
    public async Task<IActionResult> DeletePayment(Guid id, Guid paymentId)
    {
        var result = await _svc.DeletePaymentAsync(id, paymentId);
        return result.ToNoContentResult();
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<List<OverdueInvoiceDto>>> GetOverdue()
    {
        var result = await _svc.GetOverdueInvoicesAsync();
        return result.ToActionResult();
    }

    [HttpGet("summary")]
    public async Task<ActionResult<InvoiceSummaryDto>> GetSummary()
    {
        var result = await _svc.GetSummaryAsync();
        return result.ToActionResult();
    }
}