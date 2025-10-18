using AutoMapper;
using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnidadDeTrabajo unitOfWork, IMapper mapper, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<object> GetPagedAsync(int page, int pageSize, string? q, string? sort)
    {
        _logger.LogDebug(
            "Consultando productos paginados. Página: {Page}, Tamaño: {PageSize}, Búsqueda: {Query}, Orden: {Sort}",
            page, pageSize, q ?? "ninguna", sort ?? "name:asc"
        );

        var (items, total) = await _unitOfWork.Products.GetPagedAsync(page, pageSize, q, sort);
        var result = _mapper.Map<List<ProductDto>>(items);

        _logger.LogInformation(
            "Productos obtenidos. Total: {Total}, Página: {Page}, Resultados: {Count}",
            total, page, result.Count
        );

        return new { total, page, pageSize, items = result };
    }

    public async Task<Result<ProductDto>> GetAsync(Guid id)
    {
        _logger.LogDebug("Consultando producto. ProductId: {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id);

        if (product is null)
        {
            _logger.LogWarning("Producto no encontrado. ProductId: {ProductId}", id);
            return Result<ProductDto>.Failure("Product not found");
        }

        _logger.LogDebug(
            "Producto encontrado. ProductId: {ProductId}, SKU: {Sku}, Nombre: {Name}, Stock: {Stock}",
            id, product.Sku, product.Name, product.Stock
        );

        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result<ProductDto>> CreateAsync(ProductCreateDto dto)
    {
        _logger.LogInformation(
            "Iniciando creación de producto. SKU: {Sku}, Nombre: {Name}, Precio: {Price}, Stock: {Stock}",
            dto.Sku, dto.Name, dto.Price, dto.Stock
        );

        var product = _mapper.Map<Product>(dto);

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Producto creado exitosamente. ProductId: {ProductId}, SKU: {Sku}, Nombre: {Name}, Precio: {Price}, Stock: {Stock}",
            product.Id, product.Sku, product.Name, product.Price, product.Stock
        );

        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result<ProductDto>> UpdateAsync(Guid id, ProductUpdateDto dto)
    {
        _logger.LogInformation(
            "Iniciando actualización de producto. ProductId: {ProductId}, SKU: {Sku}",
            id, dto.Sku
        );

        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Producto no encontrado para actualizar. ProductId: {ProductId}", id);
            return Result<ProductDto>.Failure("Product not found");
        }

        
        var skuAnterior = product.Sku;
        var nombreAnterior = product.Name;
        var precioAnterior = product.Price;
        var stockAnterior = product.Stock;

        
        _mapper.Map(dto, product);

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.SaveChangesAsync();

        
        if (precioAnterior != product.Price)
        {
            _logger.LogWarning(
                "Precio de producto actualizado. ProductId: {ProductId}, SKU: {Sku}, Precio: {PrecioAnterior} → {PrecioNuevo}",
                product.Id, product.Sku, precioAnterior, product.Price
            );
        }

        if (stockAnterior != product.Stock)
        {
            _logger.LogWarning(
                "Stock de producto actualizado manualmente. ProductId: {ProductId}, SKU: {Sku}, Stock: {StockAnterior} → {StockNuevo}",
                product.Id, product.Sku, stockAnterior, product.Stock
            );
        }

        _logger.LogInformation(
            "Producto actualizado exitosamente. ProductId: {ProductId}, SKU: {SkuAnterior} → {SkuNuevo}, Nombre: {NombreAnterior} → {NombreNuevo}",
            product.Id, skuAnterior, product.Sku, nombreAnterior, product.Name
        );

        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Iniciando eliminación de producto. ProductId: {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Producto no encontrado para eliminar. ProductId: {ProductId}", id);
            return Result.Failure("Product not found");
        }

        var sku = product.Sku;
        var nombre = product.Name;
        var stock = product.Stock;

        if (stock > 0)
        {
            _logger.LogWarning(
                "Eliminando producto con stock disponible. ProductId: {ProductId}, SKU: {Sku}, Stock: {Stock}",
                id, sku, stock
            );
        }

        
        await _unitOfWork.Products.DeleteAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning(
            "Producto eliminado (soft delete). ProductId: {ProductId}, SKU: {Sku}, Nombre: {Name}, Stock: {Stock}",
            id, sku, nombre, stock
        );

        return Result.Success();
    }

    
    public async Task<Result<ProductDto>> RestoreAsync(Guid id)
    {
        _logger.LogInformation("Iniciando restauración de producto. ProductId: {ProductId}", id);

       
        var product = await _unitOfWork.GetDbContext().Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            _logger.LogWarning("Producto no encontrado. ProductId: {ProductId}", id);
            return Result<ProductDto>.Failure("Product not found");
        }

        if (!product.IsDeleted)
        {
            _logger.LogWarning("Intento de restaurar producto no eliminado. ProductId: {ProductId}", id);
            return Result<ProductDto>.Failure("Product is not deleted");
        }

        
        product.IsDeleted = false;
        product.DeletedAt = null;
        product.DeletedBy = null;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Producto restaurado exitosamente. ProductId: {ProductId}, SKU: {Sku}, Nombre: {Name}",
            product.Id, product.Sku, product.Name
        );

        return Result<ProductDto>.Success(_mapper.Map<ProductDto>(product));
    }

   
    public async Task<object> GetDeletedAsync(int page, int pageSize)
    {
        _logger.LogDebug("Consultando productos eliminados. Página: {Page}, Tamaño: {PageSize}", page, pageSize);

        var query = _unitOfWork.GetDbContext().Products
            .IgnoreQueryFilters()
            .Where(p => p.IsDeleted)
            .OrderByDescending(p => p.DeletedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = _mapper.Map<List<ProductDto>>(items);

        _logger.LogInformation("Productos eliminados obtenidos. Total: {Total}, Resultados: {Count}", total, result.Count);

        return new { total, page, pageSize, items = result };
    }
}