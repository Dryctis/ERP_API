using ERP_API.Common.Results;
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
    public Task<object> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] string? sort = "name:asc")
        => _svc.GetPagedAsync(page, pageSize, q, sort);

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id)
    {
        var result = await _svc.GetAsync(id);
        return result.ToActionResult();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductCreateDto dto)
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
    public async Task<ActionResult<ProductDto>> Update(Guid id, ProductUpdateDto dto)
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
    [HttpGet("deleted")]
    public Task<object> GetDeleted(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        => _svc.GetDeletedAsync(page, pageSize);

   
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<ProductDto>> Restore(Guid id)
    {
        var result = await _svc.RestoreAsync(id);
        return result.ToActionResult();
    }
}