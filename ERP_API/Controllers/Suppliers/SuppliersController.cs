using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers.V1;

[ApiController]
[Route("api/v1/suppliers")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _svc;

    public SuppliersController(ISupplierService svc) => _svc = svc;

    [AllowAnonymous]
    [HttpGet]
    public Task<object> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] string? sort = "name:asc",
        [FromQuery] bool? isActive = null)
        => _svc.GetPagedAsync(page, pageSize, q, sort, isActive);


    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SupplierDto>> Get(Guid id)
    {
        var result = await _svc.GetAsync(id);
        return result.ToActionResult();
    }


    [AllowAnonymous]
    [HttpGet("{id:guid}/with-products")]
    public async Task<ActionResult<SupplierDto>> GetWithProducts(Guid id)
    {
        var result = await _svc.GetWithProductsAsync(id);
        return result.ToActionResult();
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create(SupplierCreateDto dto)
    {
        var result = await _svc.CreateAsync(dto);
        return result.ToCreatedResult(nameof(Get), new { id = result.Value?.Id });
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SupplierDto>> Update(Guid id, SupplierUpdateDto dto)
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
    public async Task<ActionResult<SupplierDto>> Restore(Guid id)
    {
        var result = await _svc.RestoreAsync(id);
        return result.ToActionResult();
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("product-suppliers")]
    public async Task<ActionResult<ProductSupplierDto>> AssignToProduct(ProductSupplierCreateDto dto)
    {
        var result = await _svc.AssignToProductAsync(dto);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetProductSupplier),
                new { id = result.Value!.Id },
                result.Value
            );
        }

        return result.ToActionResult();
    }


    [AllowAnonymous]
    [HttpGet("product-suppliers/{id:guid}")]
    public async Task<ActionResult<ProductSupplierDto>> GetProductSupplier(Guid id)
    {
        var result = await _svc.GetProductSupplierByIdAsync(id);
        return result.ToActionResult();
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("product-suppliers/{id:guid}")]
    public async Task<ActionResult<ProductSupplierDto>> UpdateProductSupplier(
        Guid id,
        ProductSupplierUpdateDto dto)
    {
        var result = await _svc.UpdateProductSupplierAsync(id, dto);
        return result.ToActionResult();
    }


    [Authorize(Roles = "Admin")]
    [HttpDelete("product-suppliers/{id:guid}")]
    public async Task<IActionResult> RemoveFromProduct(Guid id)
    {
        var result = await _svc.RemoveFromProductAsync(id);
        return result.ToNoContentResult();
    }

    [AllowAnonymous]
    [HttpGet("by-product/{productId:guid}")]
    public async Task<ActionResult<List<SupplierForProductDto>>> GetSuppliersByProduct(Guid productId)
    {
        var result = await _svc.GetSuppliersByProductAsync(productId);
        return result.ToActionResult();
    }

    [AllowAnonymous]
    [HttpGet("{supplierId:guid}/products")]
    public async Task<ActionResult<List<ProductSupplierDto>>> GetProductsBySupplier(Guid supplierId)
    {
        var result = await _svc.GetProductsBySupplierId(supplierId);
        return result.ToActionResult();
    }
}