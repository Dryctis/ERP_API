using AutoMapper;
using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Services.Implementations;

public class SupplierService : ISupplierService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        IUnidadDeTrabajo unitOfWork,
        IMapper mapper,
        ILogger<SupplierService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<object> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm,
        string? sort,
        bool? isActive = null)
    {
        _logger.LogDebug(
            "Consultando proveedores paginados. Página: {Page}, Tamaño: {PageSize}, Búsqueda: {Search}, IsActive: {IsActive}",
            page, pageSize, searchTerm ?? "ninguna", isActive?.ToString() ?? "todos"
        );

        var (items, total) = await _unitOfWork.Suppliers.GetPagedAsync(
            page, pageSize, searchTerm, sort, isActive);

        var result = items.Select(s => new SupplierListDto(
            s.Id,
            s.Name,
            s.Email,
            s.Phone,
            s.City,
            s.IsActive,
            s.ProductSuppliers.Count
        )).ToList();

        _logger.LogInformation(
            "Proveedores obtenidos. Total: {Total}, Página: {Page}, Resultados: {Count}",
            total, page, result.Count
        );

        return new { total, page, pageSize, items = result };
    }

    public async Task<Result<SupplierDto>> GetAsync(Guid id)
    {
        _logger.LogDebug("Consultando proveedor. SupplierId: {SupplierId}", id);

        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);

        if (supplier is null)
        {
            _logger.LogWarning("Proveedor no encontrado. SupplierId: {SupplierId}", id);
            return Result<SupplierDto>.Failure("Supplier not found");
        }

        var dto = _mapper.Map<SupplierDto>(supplier);

        _logger.LogDebug(
            "Proveedor encontrado. SupplierId: {SupplierId}, Nombre: {Name}",
            id, supplier.Name
        );

        return Result<SupplierDto>.Success(dto);
    }

    public async Task<Result<SupplierDto>> GetWithProductsAsync(Guid id)
    {
        _logger.LogDebug("Consultando proveedor con productos. SupplierId: {SupplierId}", id);

        var supplier = await _unitOfWork.Suppliers.GetByIdWithProductsAsync(id);

        if (supplier is null)
        {
            _logger.LogWarning("Proveedor no encontrado. SupplierId: {SupplierId}", id);
            return Result<SupplierDto>.Failure("Supplier not found");
        }

        var dto = _mapper.Map<SupplierDto>(supplier);

        _logger.LogDebug(
            "Proveedor encontrado con {ProductCount} productos. SupplierId: {SupplierId}",
            supplier.ProductSuppliers.Count, id
        );

        return Result<SupplierDto>.Success(dto);
    }

    public async Task<Result<SupplierDto>> CreateAsync(SupplierCreateDto dto)
    {
        _logger.LogInformation(
            "Iniciando creación de proveedor. Email: {Email}, Nombre: {Name}",
            dto.Email, dto.Name
        );

        
        if (await _unitOfWork.Suppliers.ExistsByEmailAsync(dto.Email))
        {
            _logger.LogWarning(
                "Intento de crear proveedor con email duplicado. Email: {Email}",
                dto.Email
            );
            return Result<SupplierDto>.Failure("Email already exists");
        }

       
        if (!string.IsNullOrWhiteSpace(dto.TaxId) &&
            await _unitOfWork.Suppliers.ExistsByTaxIdAsync(dto.TaxId))
        {
            _logger.LogWarning(
                "Intento de crear proveedor con TaxId duplicado. TaxId: {TaxId}",
                dto.TaxId
            );
            return Result<SupplierDto>.Failure("Tax ID already exists");
        }

        var supplier = _mapper.Map<Supplier>(dto);
        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Proveedor creado exitosamente. SupplierId: {SupplierId}, Nombre: {Name}",
            supplier.Id, supplier.Name
        );

        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
    }

    public async Task<Result<SupplierDto>> UpdateAsync(Guid id, SupplierUpdateDto dto)
    {
        _logger.LogInformation(
            "Iniciando actualización de proveedor. SupplierId: {SupplierId}",
            id
        );

        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);

        if (supplier is null)
        {
            _logger.LogWarning(
                "Proveedor no encontrado para actualizar. SupplierId: {SupplierId}",
                id
            );
            return Result<SupplierDto>.Failure("Supplier not found");
        }

        var nombreAnterior = supplier.Name;

        
        if (await _unitOfWork.Suppliers.ExistsByEmailAsync(dto.Email, id))
        {
            _logger.LogWarning(
                "Intento de actualizar con email duplicado. SupplierId: {SupplierId}, Email: {Email}",
                id, dto.Email
            );
            return Result<SupplierDto>.Failure("Email already exists");
        }

       
        if (!string.IsNullOrWhiteSpace(dto.TaxId) &&
            await _unitOfWork.Suppliers.ExistsByTaxIdAsync(dto.TaxId, id))
        {
            _logger.LogWarning(
                "Intento de actualizar con TaxId duplicado. SupplierId: {SupplierId}, TaxId: {TaxId}",
                id, dto.TaxId
            );
            return Result<SupplierDto>.Failure("Tax ID already exists");
        }

        _mapper.Map(dto, supplier);
        await _unitOfWork.Suppliers.UpdateAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Proveedor actualizado exitosamente. SupplierId: {SupplierId}, Nombre: {NombreAnterior} → {NombreNuevo}",
            id, nombreAnterior, supplier.Name
        );

        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        _logger.LogInformation(
            "Iniciando eliminación de proveedor. SupplierId: {SupplierId}",
            id
        );

        var supplier = await _unitOfWork.Suppliers.GetByIdWithProductsAsync(id);

        if (supplier is null)
        {
            _logger.LogWarning(
                "Proveedor no encontrado para eliminar. SupplierId: {SupplierId}",
                id
            );
            return Result.Failure("Supplier not found");
        }

       
        if (supplier.ProductSuppliers.Any())
        {
            _logger.LogWarning(
                "Intento de eliminar proveedor con productos asociados. SupplierId: {SupplierId}, ProductCount: {Count}",
                id, supplier.ProductSuppliers.Count
            );
            return Result.Failure(
                $"Cannot delete supplier with {supplier.ProductSuppliers.Count} associated products. Remove associations first."
            );
        }

        await _unitOfWork.Suppliers.DeleteAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning(
            "Proveedor eliminado (soft delete). SupplierId: {SupplierId}, Nombre: {Name}",
            id, supplier.Name
        );

        return Result.Success();
    }

    public async Task<Result<SupplierDto>> RestoreAsync(Guid id)
    {
        _logger.LogInformation(
            "Iniciando restauración de proveedor. SupplierId: {SupplierId}",
            id
        );

        var supplier = await _unitOfWork.GetDbContext().Set<Supplier>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier is null)
        {
            _logger.LogWarning("Proveedor no encontrado. SupplierId: {SupplierId}", id);
            return Result<SupplierDto>.Failure("Supplier not found");
        }

        if (!supplier.IsDeleted)
        {
            _logger.LogWarning(
                "Intento de restaurar proveedor no eliminado. SupplierId: {SupplierId}",
                id
            );
            return Result<SupplierDto>.Failure("Supplier is not deleted");
        }

        supplier.IsDeleted = false;
        supplier.DeletedAt = null;
        supplier.DeletedBy = null;

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Proveedor restaurado exitosamente. SupplierId: {SupplierId}, Nombre: {Name}",
            id, supplier.Name
        );

        return Result<SupplierDto>.Success(_mapper.Map<SupplierDto>(supplier));
    }

    public async Task<object> GetDeletedAsync(int page, int pageSize)
    {
        _logger.LogDebug(
            "Consultando proveedores eliminados. Página: {Page}, Tamaño: {PageSize}",
            page, pageSize
        );

        var (items, total) = await _unitOfWork.Suppliers.GetDeletedPagedAsync(page, pageSize);

        var result = _mapper.Map<List<SupplierDto>>(items);

        _logger.LogInformation(
            "Proveedores eliminados obtenidos. Total: {Total}, Resultados: {Count}",
            total, result.Count
        );

        return new { total, page, pageSize, items = result };
    }



    public async Task<Result<ProductSupplierDto>> AssignToProductAsync(ProductSupplierCreateDto dto)
    {
        _logger.LogInformation(
            "Asignando proveedor a producto. ProductId: {ProductId}, SupplierId: {SupplierId}",
            dto.ProductId, dto.SupplierId
        );

       
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product is null)
        {
            _logger.LogWarning("Producto no encontrado. ProductId: {ProductId}", dto.ProductId);
            return Result<ProductSupplierDto>.Failure("Product not found");
        }

        
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId);
        if (supplier is null)
        {
            _logger.LogWarning("Proveedor no encontrado. SupplierId: {SupplierId}", dto.SupplierId);
            return Result<ProductSupplierDto>.Failure("Supplier not found");
        }

       
        if (await _unitOfWork.ProductSuppliers.ExistsAsync(dto.ProductId, dto.SupplierId))
        {
            _logger.LogWarning(
                "La relación producto-proveedor ya existe. ProductId: {ProductId}, SupplierId: {SupplierId}",
                dto.ProductId, dto.SupplierId
            );
            return Result<ProductSupplierDto>.Failure("Product-Supplier relationship already exists");
        }

        
        if (dto.IsPreferred)
        {
            await RemovePreferredStatusFromOthers(dto.ProductId);
        }

        var productSupplier = _mapper.Map<ProductSupplier>(dto);
        await _unitOfWork.ProductSuppliers.AddAsync(productSupplier);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Proveedor asignado exitosamente. ProductSupplierId: {Id}, ProductId: {ProductId}, SupplierId: {SupplierId}",
            productSupplier.Id, dto.ProductId, dto.SupplierId
        );

        var created = await _unitOfWork.ProductSuppliers.GetByIdAsync(productSupplier.Id);
        return Result<ProductSupplierDto>.Success(_mapper.Map<ProductSupplierDto>(created!));
    }

    public async Task<Result<ProductSupplierDto>> UpdateProductSupplierAsync(
        Guid id,
        ProductSupplierUpdateDto dto)
    {
        _logger.LogInformation(
            "Actualizando relación producto-proveedor. ProductSupplierId: {Id}",
            id
        );

        var productSupplier = await _unitOfWork.ProductSuppliers.GetByIdAsync(id);

        if (productSupplier is null)
        {
            _logger.LogWarning(
                "Relación producto-proveedor no encontrada. ProductSupplierId: {Id}",
                id
            );
            return Result<ProductSupplierDto>.Failure("Product-Supplier relationship not found");
        }

       
        if (dto.IsPreferred && !productSupplier.IsPreferred)
        {
            await RemovePreferredStatusFromOthers(productSupplier.ProductId, id);
        }

        _mapper.Map(dto, productSupplier);
        await _unitOfWork.ProductSuppliers.UpdateAsync(productSupplier);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Relación actualizada exitosamente. ProductSupplierId: {Id}",
            id
        );

        var updated = await _unitOfWork.ProductSuppliers.GetByIdAsync(id);
        return Result<ProductSupplierDto>.Success(_mapper.Map<ProductSupplierDto>(updated!));
    }

    public async Task<Result> RemoveFromProductAsync(Guid productSupplierId)
    {
        _logger.LogInformation(
            "Eliminando relación producto-proveedor. ProductSupplierId: {Id}",
            productSupplierId
        );

        var productSupplier = await _unitOfWork.ProductSuppliers.GetByIdAsync(productSupplierId);

        if (productSupplier is null)
        {
            _logger.LogWarning(
                "Relación producto-proveedor no encontrada. ProductSupplierId: {Id}",
                productSupplierId
            );
            return Result.Failure("Product-Supplier relationship not found");
        }

        await _unitOfWork.ProductSuppliers.DeleteAsync(productSupplier);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Relación eliminada exitosamente. ProductSupplierId: {Id}",
            productSupplierId
        );

        return Result.Success();
    }

    public async Task<Result<List<SupplierForProductDto>>> GetSuppliersByProductAsync(Guid productId)
    {
        _logger.LogDebug(
            "Consultando proveedores de producto. ProductId: {ProductId}",
            productId
        );

        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product is null)
        {
            _logger.LogWarning("Producto no encontrado. ProductId: {ProductId}", productId);
            return Result<List<SupplierForProductDto>>.Failure("Product not found");
        }

        var productSuppliers = await _unitOfWork.ProductSuppliers.GetByProductIdAsync(productId);

        var result = productSuppliers.Select(ps => new SupplierForProductDto(
            ps.SupplierId,
            ps.Supplier.Name,
            ps.Supplier.Email,
            ps.SupplierPrice,
            ps.SupplierSku,
            ps.IsPreferred,
            ps.LeadTimeDays,
            ps.MinimumOrderQuantity
        )).ToList();

        _logger.LogDebug(
            "Proveedores obtenidos. ProductId: {ProductId}, Count: {Count}",
            productId, result.Count
        );

        return Result<List<SupplierForProductDto>>.Success(result);
    }

    public async Task<Result<List<ProductSupplierDto>>> GetProductsBySupplierId(Guid supplierId)
    {
        _logger.LogDebug(
            "Consultando productos de proveedor. SupplierId: {SupplierId}",
            supplierId
        );

        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(supplierId);
        if (supplier is null)
        {
            _logger.LogWarning("Proveedor no encontrado. SupplierId: {SupplierId}", supplierId);
            return Result<List<ProductSupplierDto>>.Failure("Supplier not found");
        }

        var productSuppliers = await _unitOfWork.ProductSuppliers.GetBySupplierId(supplierId);

        var result = _mapper.Map<List<ProductSupplierDto>>(productSuppliers);

        _logger.LogDebug(
            "Productos obtenidos. SupplierId: {SupplierId}, Count: {Count}",
            supplierId, result.Count
        );

        return Result<List<ProductSupplierDto>>.Success(result);
    }

    

    private async Task RemovePreferredStatusFromOthers(Guid productId, Guid? excludeId = null)
    {
        var existingPreferred = await _unitOfWork.GetDbContext().Set<ProductSupplier>()
            .Where(ps => ps.ProductId == productId && ps.IsPreferred)
            .ToListAsync();

        if (excludeId.HasValue)
        {
            existingPreferred = existingPreferred.Where(ps => ps.Id != excludeId.Value).ToList();
        }

        foreach (var ps in existingPreferred)
        {
            ps.IsPreferred = false;
            await _unitOfWork.ProductSuppliers.UpdateAsync(ps);
        }
    }

    public async Task<Result<ProductSupplierDto>> GetProductSupplierByIdAsync(Guid id)
    {
        var productSupplier = await _unitOfWork.ProductSuppliers.GetByIdAsync(id);

        if (productSupplier is null)
            return Result<ProductSupplierDto>.Failure("Product-Supplier relationship not found");

        return Result<ProductSupplierDto>.Success(_mapper.Map<ProductSupplierDto>(productSupplier));
    }
}