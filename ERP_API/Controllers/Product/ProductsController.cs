using ERP_API.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers.V1;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;
    public ProductsController(IProductService svc) => _svc = svc;

    
    [AllowAnonymous]
    [HttpGet]
    public Task<object> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
                             [FromQuery] string? q = null, [FromQuery] string? sort = "name:asc")
        => _svc.GetPagedAsync(page, pageSize, q, sort);

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id)
        => (await _svc.GetAsync(id)) is { } dto ? Ok(dto) : NotFound();

    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductCreateDto dto)
    {
        var created = await _svc.CreateAsync(dto);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, ProductUpdateDto dto)
        => (await _svc.UpdateAsync(id, dto)) is { } updated ? Ok(updated) : NotFound();

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
        => await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}
