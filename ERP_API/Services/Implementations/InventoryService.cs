using AutoMapper;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Services.Implementations;


public class InventoryService : IInventoryService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryService(IUnidadDeTrabajo unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InventoryMovementDto>> GetAllAsync()
    {
        var movements = await _unitOfWork.Inventory.GetAllAsync();
        return _mapper.Map<IEnumerable<InventoryMovementDto>>(movements);
    }

    public async Task<InventoryMovementDto?> GetByIdAsync(Guid id)
    {
        var movement = await _unitOfWork.Inventory.GetByIdAsync(id);
        return movement == null ? null : _mapper.Map<InventoryMovementDto>(movement);
    }

    
    public async Task<InventoryMovementDto> CreateAsync(InventoryMovementCreateDto dto)
    {
        
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new InvalidOperationException($"Producto con ID {dto.ProductId} no encontrado");

       
        var movement = _mapper.Map<InventoryMovement>(dto);

        
        if (movement.MovementType == MovementType.Increase)
        {
            product.Stock += movement.Quantity;
        }
        else if (movement.MovementType == MovementType.Decrease)
        {
            if (product.Stock < movement.Quantity)
                throw new InvalidOperationException(
                    $"Stock insuficiente. Disponible: {product.Stock}, Requerido: {movement.Quantity}");

            product.Stock -= movement.Quantity;
        }

        
        await _unitOfWork.Inventory.AddAsync(movement);
        await _unitOfWork.Products.UpdateAsync(product);

        
        await _unitOfWork.SaveChangesAsync();

        
        var createdMovement = await _unitOfWork.Inventory.GetByIdAsync(movement.Id);
        return _mapper.Map<InventoryMovementDto>(createdMovement!);
    }

    
    public async Task<ProductStockDto?> GetStockAsync(Guid productId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        return product == null ? null : new ProductStockDto(product.Id, product.Name, product.Stock);
    }

    
    public async Task<IEnumerable<InventoryMovementDto>> GetFilteredAsync(
        Guid? productId, DateTime? from, DateTime? to)
    {
        
        var query = _unitOfWork.GetDbContext().InventoryMovements
            .Include(m => m.Product)
            .AsNoTracking()
            .AsQueryable();

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
       
        var products = await _unitOfWork.GetDbContext().Products
            .AsNoTracking()
            .Where(p => p.Stock < threshold)
            .OrderBy(p => p.Stock) 
            .ToListAsync();

        return products.Select(p => new ProductStockDto(p.Id, p.Name, p.Stock));
    }
}