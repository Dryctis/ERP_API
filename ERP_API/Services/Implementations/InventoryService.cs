using AutoMapper;
using ERP_API.Common.Exceptions;
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
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IUnidadDeTrabajo unitOfWork, IMapper mapper, ILogger<InventoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
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
        _logger.LogInformation(
            "Iniciando movimiento de inventario. ProductId: {ProductId}, Tipo: {MovementType}, Cantidad: {Quantity}",
            dto.ProductId,
            dto.MovementType,
            dto.Quantity
        );

    
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product == null)
        {
            _logger.LogWarning("Producto no encontrado: {ProductId}", dto.ProductId);
            throw new ProductoNoEncontradoException(dto.ProductId);
        }

        var stockAnterior = product.Stock;

       
        var movement = _mapper.Map<InventoryMovement>(dto);

        if (movement.MovementType == MovementType.Increase)
        {
            product.Stock += movement.Quantity;

            _logger.LogInformation(
                "Incrementando stock. ProductId: {ProductId}, Anterior: {StockAnterior}, Incremento: {Quantity}, Nuevo: {StockNuevo}",
                product.Id,
                stockAnterior,
                movement.Quantity,
                product.Stock
            );
        }
        else if (movement.MovementType == MovementType.Decrease)
        {
            if (product.Stock < movement.Quantity)
            {
                _logger.LogWarning(
                    "Stock insuficiente. ProductId: {ProductId}, Disponible: {Disponible}, Requerido: {Requerido}",
                    product.Id,
                    product.Stock,
                    movement.Quantity
                );

                throw new StockInsuficienteException(
                    product.Id,
                    product.Name,
                    product.Stock,
                    movement.Quantity
                );
            }

            product.Stock -= movement.Quantity;

            _logger.LogInformation(
                "Disminuyendo stock. ProductId: {ProductId}, Anterior: {StockAnterior}, Decremento: {Quantity}, Nuevo: {StockNuevo}",
                product.Id,
                stockAnterior,
                movement.Quantity,
                product.Stock
            );
        }

        
        await _unitOfWork.Inventory.AddAsync(movement);
        await _unitOfWork.Products.UpdateAsync(product);

      
        try
        {
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Movimiento de inventario completado. MovementId: {MovementId}, ProductId: {ProductId}, Stock final: {Stock}",
                movement.Id,
                product.Id,
                product.Stock
            );
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(
                ex,
                "Conflicto de concurrencia al actualizar stock. ProductId: {ProductId}",
                product.Id
            );

            throw new ConcurrencyException("Producto", product.Id);
        }

        
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
        _logger.LogInformation("Consultando productos con stock bajo. Umbral: {Threshold}", threshold);

        var products = await _unitOfWork.GetDbContext().Products
            .AsNoTracking()
            .Where(p => p.Stock < threshold)
            .OrderBy(p => p.Stock)
            .ToListAsync();

        _logger.LogInformation(
            "Productos con stock bajo encontrados: {Count}",
            products.Count
        );

        return products.Select(p => new ProductStockDto(p.Id, p.Name, p.Stock));
    }
}