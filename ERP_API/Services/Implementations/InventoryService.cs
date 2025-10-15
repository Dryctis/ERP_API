using AutoMapper;
using ERP_API.Data;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Services.Implementations
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public InventoryService(IInventoryRepository repository, AppDbContext db, IMapper mapper)
        {
            _repository = repository;
            _db = db;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InventoryMovementDto>> GetAllAsync()
        {
            var movements = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<InventoryMovementDto>>(movements);
        }

        public async Task<InventoryMovementDto?> GetByIdAsync(Guid id)
        {
            var movement = await _repository.GetByIdAsync(id);
            return movement == null ? null : _mapper.Map<InventoryMovementDto>(movement);
        }

        public async Task<InventoryMovementDto> CreateAsync(InventoryMovementCreateDto dto)
        {
            var movement = _mapper.Map<InventoryMovement>(dto);

           
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
            if (product == null)
                throw new Exception("Producto no encontrado");

            
            if (movement.MovementType == MovementType.Increase)
            {
                product.Stock += movement.Quantity;
            }
            else if (movement.MovementType == MovementType.Decrease)
            {
                if (product.Stock < movement.Quantity)
                    throw new Exception("Stock insuficiente");
                product.Stock -= movement.Quantity;
            }

            
            await _repository.AddAsync(movement);
            await _repository.SaveChangesAsync();

            return _mapper.Map<InventoryMovementDto>(movement);
        }

        public async Task<ProductStockDto?> GetStockAsync(Guid productId)
        {
            var p = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
            return p == null ? null : new ProductStockDto(p.Id, p.Name, p.Stock);
        }

        public async Task<IEnumerable<InventoryMovementDto>> GetFilteredAsync(Guid? productId, DateTime? from, DateTime? to)
        {
            var query = _db.InventoryMovements.Include(m => m.Product).AsQueryable();

            if (productId.HasValue)
                query = query.Where(m => m.ProductId == productId.Value);

            if (from.HasValue)
                query = query.Where(m => m.CreatedAt >= from.Value);

            if (to.HasValue)
                query = query.Where(m => m.CreatedAt <= to.Value);

            var movements = await query.ToListAsync();
            return _mapper.Map<IEnumerable<InventoryMovementDto>>(movements);
        }

        public async Task<IEnumerable<ProductStockDto>> GetLowStockAsync(int threshold)
        {
            var products = await _db.Products
                .Where(p => p.Stock < threshold)
                .ToListAsync();

            return products.Select(p => new ProductStockDto(p.Id, p.Name, p.Stock));
        }
    }
}
