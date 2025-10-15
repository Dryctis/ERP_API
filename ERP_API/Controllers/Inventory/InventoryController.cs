using ERP_API.DTOs;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _service;

        public InventoryController(IInventoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InventoryMovementCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("stock/{productId}")]
        public async Task<IActionResult> GetStock(Guid productId)
        {
            var stock = await _service.GetStockAsync(productId);
            return stock == null ? NotFound() : Ok(stock);
        }

        [HttpGet("movements/filter")]
        public async Task<IActionResult> GetFiltered([FromQuery] Guid? productId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _service.GetFilteredAsync(productId, from, to);
            return Ok(result);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock([FromQuery] int threshold = 10)
        {
            var result = await _service.GetLowStockAsync(threshold);
            return Ok(result);
        }
    }
}
