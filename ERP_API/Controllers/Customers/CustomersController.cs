using ERP_API.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers.V1;

[ApiController]
[Route("api/v1/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _svc;
    public CustomersController(ICustomerService svc) => _svc = svc;

    [AllowAnonymous]
    [HttpGet]
    public Task<object> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? q = null, [FromQuery] string? sort = "name:asc")
        => _svc.GetPagedAsync(page, pageSize, q, sort);

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Get(Guid id)
        => (await _svc.GetAsync(id)) is { } dto ? Ok(dto) : NotFound();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerCreateDto dto)
    {
        var (ok, error, created) = await _svc.CreateAsync(dto);
        if (!ok && error == "Email already exists") return Conflict(new { message = error });
        return CreatedAtAction(nameof(Get), new { id = created!.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, CustomerUpdateDto dto)
    {
        var (ok, error, updated) = await _svc.UpdateAsync(id, dto);
        if (!ok && error == "NotFound") return NotFound();
        if (!ok && error == "Email already exists") return Conflict(new { message = error });
        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
