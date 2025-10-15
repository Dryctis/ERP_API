using ERP_API.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers.V1;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _svc;
    public OrdersController(IOrderService svc) => _svc = svc;

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(OrderCreateDto dto)
    {
        var (ok, error, created) = await _svc.CreateAsync(dto);
        if (!ok && error == "CustomerNotFound") return NotFound(new { message = error });
        if (!ok && error != null && error.StartsWith("InsufficientStock")) return Conflict(new { message = error });
        if (!ok) return BadRequest(new { message = error });
        return CreatedAtAction(nameof(Get), new { id = created!.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> Get(Guid id)
        => (await _svc.GetAsync(id)) is { } dto ? Ok(dto) : NotFound();
}
