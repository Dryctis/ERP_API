using AutoMapper;
using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace ERP_API.Services.Implementations;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PurchaseOrderService> _logger;

    public PurchaseOrderService(
        IUnidadDeTrabajo unitOfWork,
        IMapper mapper,
        ILogger<PurchaseOrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<object> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? status = null,
        Guid? supplierId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sort = null)
    {
        _logger.LogDebug(
            "Consultando órdenes de compra paginadas. Página: {Page}, Tamaño: {PageSize}",
            page, pageSize
        );

        var (items, total) = await _unitOfWork.PurchaseOrders.GetPagedAsync(
            page, pageSize, searchTerm, status, supplierId, fromDate, toDate, sort);

        var result = items.Select(po => new PurchaseOrderListDto(
            po.Id,
            po.OrderNumber,
            po.Supplier.Name,
            po.Status.ToString(),
            po.OrderDate,
            po.ExpectedDeliveryDate,
            po.Total,
            po.Balance,
            po.Items.Count,
            po.IsFullyReceived()
        )).ToList();

        _logger.LogInformation(
            "Órdenes de compra obtenidas. Total: {Total}, Página: {Page}, Resultados: {Count}",
            total, page, result.Count
        );

        return new { total, page, pageSize, items = result };
    }

    public async Task<Result<PurchaseOrderDto>> GetAsync(Guid id)
    {
        _logger.LogDebug("Consultando orden de compra. PurchaseOrderId: {Id}", id);

        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(id);

        if (purchaseOrder is null)
        {
            _logger.LogWarning("Orden de compra no encontrada. PurchaseOrderId: {Id}", id);
            return Result<PurchaseOrderDto>.Failure("Purchase order not found");
        }

        var dto = MapToPurchaseOrderDto(purchaseOrder);

        _logger.LogDebug(
            "Orden de compra encontrada. OrderNumber: {Number}, Supplier: {Supplier}",
            purchaseOrder.OrderNumber, purchaseOrder.Supplier.Name
        );

        return Result<PurchaseOrderDto>.Success(dto);
    }

    public async Task<Result<PurchaseOrderSummaryDto>> GetSummaryAsync()
    {
        _logger.LogDebug("Generando resumen de órdenes de compra");

        var statusCounts = await _unitOfWork.PurchaseOrders.GetOrderCountByStatusAsync();

        var allOrders = await _unitOfWork.GetDbContext().Set<PurchaseOrder>()
            .AsNoTracking()
            .ToListAsync();

        var totalAmount = allOrders
            .Where(po => po.Status != PurchaseOrderStatus.Cancelled)
            .Sum(po => po.Total);

        var totalPaid = allOrders
            .Where(po => po.Status != PurchaseOrderStatus.Cancelled)
            .Sum(po => po.PaidAmount);

        var totalOutstanding = totalAmount - totalPaid;

        var overdueCount = allOrders.Count(po =>
            po.ExpectedDeliveryDate < DateTime.UtcNow &&
            po.Status != PurchaseOrderStatus.Received &&
            po.Status != PurchaseOrderStatus.Cancelled);

        var summary = new PurchaseOrderSummaryDto(
            TotalOrders: allOrders.Count,
            DraftOrders: statusCounts.GetValueOrDefault(PurchaseOrderStatus.Draft, 0),
            SentOrders: statusCounts.GetValueOrDefault(PurchaseOrderStatus.Sent, 0),
            ConfirmedOrders: statusCounts.GetValueOrDefault(PurchaseOrderStatus.Confirmed, 0),
            PartiallyReceivedOrders: statusCounts.GetValueOrDefault(PurchaseOrderStatus.PartiallyReceived, 0),
            ReceivedOrders: statusCounts.GetValueOrDefault(PurchaseOrderStatus.Received, 0),
            CancelledOrders: statusCounts.GetValueOrDefault(PurchaseOrderStatus.Cancelled, 0),
            TotalAmount: totalAmount,
            TotalPaid: totalPaid,
            TotalOutstanding: totalOutstanding,
            OverdueOrders: overdueCount
        );

        _logger.LogInformation(
            "Resumen generado. Total: {Total}, Outstanding: {Outstanding}",
            totalAmount, totalOutstanding
        );

        return Result<PurchaseOrderSummaryDto>.Success(summary);
    }

    public async Task<Result<List<OverduePurchaseOrderDto>>> GetOverdueOrdersAsync()
    {
        _logger.LogDebug("Consultando órdenes de compra vencidas");

        var orders = await _unitOfWork.PurchaseOrders.GetOverdueOrdersAsync();

        var result = orders.Select(po => new OverduePurchaseOrderDto(
            po.Id,
            po.OrderNumber,
            po.Supplier.Name,
            po.OrderDate,
            po.ExpectedDeliveryDate,
            (DateTime.UtcNow - po.ExpectedDeliveryDate).Days,
            po.Total,
            po.Balance,
            po.Status.ToString()
        )).ToList();

        _logger.LogInformation("Órdenes vencidas encontradas: {Count}", result.Count);

        return Result<List<OverduePurchaseOrderDto>>.Success(result);
    }

    public async Task<Result<List<ReorderSuggestionDto>>> GetReorderSuggestionsAsync(int threshold = 10)
    {
        _logger.LogInformation(
            "Generando sugerencias de reorden. Umbral: {Threshold}",
            threshold
        );

        var lowStockProducts = await _unitOfWork.GetDbContext().Products
            .Include(p => p.ProductSuppliers)
                .ThenInclude(ps => ps.Supplier)
            .Where(p => p.Stock < threshold)
            .AsNoTracking()
            .ToListAsync();

        var suggestions = new List<ReorderSuggestionDto>();

        foreach (var product in lowStockProducts)
        {
            var minStock = threshold;
            var suggestedQuantity = (minStock * 2) - product.Stock;

            var preferredSupplier = product.ProductSuppliers
                .FirstOrDefault(ps => ps.IsPreferred);

            suggestions.Add(new ReorderSuggestionDto(
                product.Id,
                product.Name,
                product.Sku,
                product.Stock,
                minStock,
                suggestedQuantity,
                preferredSupplier?.SupplierId,
                preferredSupplier?.Supplier.Name,
                preferredSupplier != null ? preferredSupplier.SupplierPrice * suggestedQuantity : null
            ));
        }

        _logger.LogInformation(
            "Sugerencias generadas: {Count} productos necesitan reorden",
            suggestions.Count
        );

        return Result<List<ReorderSuggestionDto>>.Success(suggestions);
    }


    public async Task<Result<PurchaseOrderDto>> CreateAsync(PurchaseOrderCreateDto dto)
    {
        _logger.LogInformation(
            "Creando orden de compra. SupplierId: {SupplierId}, Items: {ItemCount}",
            dto.SupplierId, dto.Items?.Count ?? 0
        );

        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(dto.SupplierId);
        if (supplier is null)
        {
            _logger.LogWarning("Proveedor no encontrado. SupplierId: {SupplierId}", dto.SupplierId);
            return Result<PurchaseOrderDto>.Failure("Supplier not found");
        }

        if (dto.Items is null || dto.Items.Count == 0)
        {
            _logger.LogWarning("Orden sin items. SupplierId: {SupplierId}", dto.SupplierId);
            return Result<PurchaseOrderDto>.Failure("Purchase order must have at least one item");
        }

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await GetProductsDictionaryAsync(productIds);

        if (products.Count != productIds.Count)
        {
            _logger.LogWarning(
                "Productos no encontrados. SupplierId: {SupplierId}",
                dto.SupplierId
            );
            return Result<PurchaseOrderDto>.Failure("One or more products not found");
        }

        var orderNumber = await _unitOfWork.PurchaseOrders.GenerateOrderNumberAsync();

        var orderDate = dto.OrderDate ?? DateTime.UtcNow;
        var expectedDeliveryDate = dto.ExpectedDeliveryDate ?? orderDate.AddDays(7);

        var purchaseOrder = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            SupplierId = dto.SupplierId,
            Status = PurchaseOrderStatus.Draft,
            OrderDate = orderDate,
            ExpectedDeliveryDate = expectedDeliveryDate,
            DiscountAmount = dto.DiscountAmount,
            ShippingCost = dto.ShippingCost,
            PaymentTerms = dto.PaymentTerms,
            Notes = dto.Notes,
            SupplierReference = dto.SupplierReference
        };

        BuildPurchaseOrderItems(purchaseOrder, dto.Items, products);
        CalculatePurchaseOrderTotals(purchaseOrder);

        await _unitOfWork.PurchaseOrders.AddAsync(purchaseOrder);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Orden de compra creada. PurchaseOrderId: {Id}, OrderNumber: {Number}",
            purchaseOrder.Id, purchaseOrder.OrderNumber
        );

        var created = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(purchaseOrder.Id);
        return Result<PurchaseOrderDto>.Success(MapToPurchaseOrderDto(created!));
    }

    public async Task<Result<PurchaseOrderDto>> UpdateAsync(Guid id, PurchaseOrderUpdateDto dto)
    {
        _logger.LogInformation("Actualizando orden de compra. PurchaseOrderId: {Id}", id);

        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(id);

        if (purchaseOrder is null)
        {
            _logger.LogWarning("Orden de compra no encontrada. PurchaseOrderId: {Id}", id);
            return Result<PurchaseOrderDto>.Failure("Purchase order not found");
        }

        if (!purchaseOrder.CanBeEdited())
        {
            _logger.LogWarning(
                "No se puede editar orden en estado {Status}. PurchaseOrderId: {Id}",
                purchaseOrder.Status, id
            );
            return Result<PurchaseOrderDto>.Failure($"Cannot edit purchase order in {purchaseOrder.Status} status");
        }

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await GetProductsDictionaryAsync(productIds);

        if (products.Count != productIds.Count)
        {
            return Result<PurchaseOrderDto>.Failure("One or more products not found");
        }

        purchaseOrder.OrderDate = dto.OrderDate;
        purchaseOrder.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
        purchaseOrder.DiscountAmount = dto.DiscountAmount;
        purchaseOrder.ShippingCost = dto.ShippingCost;
        purchaseOrder.PaymentTerms = dto.PaymentTerms;
        purchaseOrder.Notes = dto.Notes;
        purchaseOrder.SupplierReference = dto.SupplierReference;

        _unitOfWork.GetDbContext().Set<PurchaseOrderItem>().RemoveRange(purchaseOrder.Items);
        purchaseOrder.Items.Clear();

        BuildPurchaseOrderItems(purchaseOrder, dto.Items, products);
        CalculatePurchaseOrderTotals(purchaseOrder);

        await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Orden de compra actualizada. PurchaseOrderId: {Id}", id);

        var updated = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(id);
        return Result<PurchaseOrderDto>.Success(MapToPurchaseOrderDto(updated!));
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Eliminando orden de compra. PurchaseOrderId: {Id}", id);

        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(id);

        if (purchaseOrder is null)
        {
            _logger.LogWarning("Orden de compra no encontrada. PurchaseOrderId: {Id}", id);
            return Result.Failure("Purchase order not found");
        }

        if (purchaseOrder.Status != PurchaseOrderStatus.Draft)
        {
            _logger.LogWarning(
                "No se puede eliminar orden en estado {Status}. PurchaseOrderId: {Id}",
                purchaseOrder.Status, id
            );
            return Result.Failure($"Cannot delete purchase order in {purchaseOrder.Status} status");
        }

        await _unitOfWork.PurchaseOrders.DeleteAsync(purchaseOrder);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning(
            "Orden de compra eliminada. PurchaseOrderId: {Id}, OrderNumber: {Number}",
            id, purchaseOrder.OrderNumber
        );

        return Result.Success();
    }


    public async Task<Result<PurchaseOrderDto>> SendOrderAsync(Guid id)
    {
        _logger.LogInformation("Enviando orden de compra. PurchaseOrderId: {Id}", id);

        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(id);

        if (purchaseOrder is null)
        {
            _logger.LogWarning("Orden de compra no encontrada. PurchaseOrderId: {Id}", id);
            return Result<PurchaseOrderDto>.Failure("Purchase order not found");
        }

        if (!purchaseOrder.CanBeSent())
        {
            _logger.LogWarning(
                "No se puede enviar orden en estado {Status}. PurchaseOrderId: {Id}",
                purchaseOrder.Status, id
            );
            return Result<PurchaseOrderDto>.Failure($"Cannot send purchase order in {purchaseOrder.Status} status");
        }

        purchaseOrder.Status = PurchaseOrderStatus.Sent;
        await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Orden de compra enviada. PurchaseOrderId: {Id}, OrderNumber: {Number}",
            id, purchaseOrder.OrderNumber
        );

        return Result<PurchaseOrderDto>.Success(MapToPurchaseOrderDto(purchaseOrder));
    }

    public async Task<Result<PurchaseOrderDto>> ConfirmOrderAsync(Guid id, ConfirmPurchaseOrderDto dto)
    {
        _logger.LogInformation("Confirmando orden de compra. PurchaseOrderId: {Id}", id);

        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(id);

        if (purchaseOrder is null)
        {
            _logger.LogWarning("Orden de compra no encontrada. PurchaseOrderId: {Id}", id);
            return Result<PurchaseOrderDto>.Failure("Purchase order not found");
        }

        if (purchaseOrder.Status != PurchaseOrderStatus.Sent)
        {
            _logger.LogWarning(
                "No se puede confirmar orden en estado {Status}. PurchaseOrderId: {Id}",
                purchaseOrder.Status, id
            );
            return Result<PurchaseOrderDto>.Failure($"Can only confirm orders in Sent status");
        }

        purchaseOrder.Status = PurchaseOrderStatus.Confirmed;

        if (!string.IsNullOrWhiteSpace(dto.SupplierReference))
        {
            purchaseOrder.SupplierReference = dto.SupplierReference;
        }

        if (dto.ExpectedDeliveryDate.HasValue)
        {
            purchaseOrder.ExpectedDeliveryDate = dto.ExpectedDeliveryDate.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            purchaseOrder.Notes = string.IsNullOrWhiteSpace(purchaseOrder.Notes)
                ? dto.Notes
                : $"{purchaseOrder.Notes}\n[Confirmación]: {dto.Notes}";
        }

        await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Orden de compra confirmada. PurchaseOrderId: {Id}, OrderNumber: {Number}",
            id, purchaseOrder.OrderNumber
        );

        return Result<PurchaseOrderDto>.Success(MapToPurchaseOrderDto(purchaseOrder));
    }

    public async Task<Result<PurchaseOrderDto>> ReceiveItemsAsync(Guid id, ReceiveItemsDto dto)
    {
        _logger.LogInformation(
            "Recibiendo items de orden de compra. PurchaseOrderId: {Id}, ItemsCount: {Count}",
            id, dto.Items.Count
        );

        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(id);

        if (purchaseOrder is null)
        {
            _logger.LogWarning("Orden de compra no encontrada. PurchaseOrderId: {Id}", id);
            return Result<PurchaseOrderDto>.Failure("Purchase order not found");
        }

        if (!purchaseOrder.CanReceiveItems())
        {
            _logger.LogWarning(
                "No se puede recibir items en estado {Status}. PurchaseOrderId: {Id}",
                purchaseOrder.Status, id
            );
            return Result<PurchaseOrderDto>.Failure($"Cannot receive items in {purchaseOrder.Status} status");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            foreach (var receiveDto in dto.Items)
            {
                var item = purchaseOrder.Items.FirstOrDefault(i => i.Id == receiveDto.PurchaseOrderItemId);

                if (item is null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning(
                        "Item no encontrado. ItemId: {ItemId}, PurchaseOrderId: {Id}",
                        receiveDto.PurchaseOrderItemId, id
                    );
                    return Result<PurchaseOrderDto>.Failure($"Item {receiveDto.PurchaseOrderItemId} not found");
                }

                var newReceivedQuantity = item.ReceivedQuantity + receiveDto.ReceivedQuantity;
                if (newReceivedQuantity > item.OrderedQuantity)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogWarning(
                        "Cantidad recibida excede ordenada. ItemId: {ItemId}, Ordered: {Ordered}, Receiving: {Receiving}",
                        item.Id, item.OrderedQuantity, newReceivedQuantity
                    );
                    return Result<PurchaseOrderDto>.Failure(
                        $"Cannot receive {receiveDto.ReceivedQuantity} units. Only {item.PendingQuantity} pending");
                }

                item.ReceivedQuantity = newReceivedQuantity;

                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product is not null)
                {
                    var previousStock = product.Stock;
                    product.Stock += receiveDto.ReceivedQuantity;

                    await _unitOfWork.Products.UpdateAsync(product);

                    await _unitOfWork.Inventory.AddAsync(new InventoryMovement
                    {
                        ProductId = product.Id,
                        Quantity = receiveDto.ReceivedQuantity,
                        MovementType = MovementType.Increase,
                        Reason = $"Purchase Order {purchaseOrder.OrderNumber} - Item received"
                    });

                    _logger.LogInformation(
                        "Stock actualizado. ProductId: {ProductId}, Previous: {Previous}, Added: {Added}, New: {New}",
                        product.Id, previousStock, receiveDto.ReceivedQuantity, product.Stock
                    );
                }
            }

            purchaseOrder.UpdateStatusBasedOnReceipts();

            if (!string.IsNullOrWhiteSpace(dto.Notes))
            {
                purchaseOrder.Notes = string.IsNullOrWhiteSpace(purchaseOrder.Notes)
                    ? $"[Recepción]: {dto.Notes}"
                    : $"{purchaseOrder.Notes}\n[Recepción]: {dto.Notes}";
            }

            await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder);
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Items recibidos exitosamente. PurchaseOrderId: {Id}, NewStatus: {Status}",
                id, purchaseOrder.Status
            );

            var updated = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(id);
            return Result<PurchaseOrderDto>.Success(MapToPurchaseOrderDto(updated!));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(
                ex,
                "Error al recibir items. PurchaseOrderId: {Id}",
                id
            );
            return Result<PurchaseOrderDto>.Failure($"Error receiving items: {ex.Message}");
        }
    }

    public async Task<Result<PurchaseOrderDto>> CancelOrderAsync(Guid id, string? reason = null)
    {
        _logger.LogInformation("Cancelando orden de compra. PurchaseOrderId: {Id}", id);

        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdWithDetailsAsync(id);

        if (purchaseOrder is null)
        {
            _logger.LogWarning("Orden de compra no encontrada. PurchaseOrderId: {Id}", id);
            return Result<PurchaseOrderDto>.Failure("Purchase order not found");
        }

        if (!purchaseOrder.CanBeCancelled())
        {
            _logger.LogWarning(
                "No se puede cancelar orden en estado {Status}. PurchaseOrderId: {Id}",
                purchaseOrder.Status, id
            );
            return Result<PurchaseOrderDto>.Failure($"Cannot cancel purchase order in {purchaseOrder.Status} status");
        }

        if (purchaseOrder.Items.Any(i => i.ReceivedQuantity > 0))
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var item in purchaseOrder.Items.Where(i => i.ReceivedQuantity > 0))
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product is not null)
                    {
                        product.Stock -= item.ReceivedQuantity;
                        await _unitOfWork.Products.UpdateAsync(product);

                        await _unitOfWork.Inventory.AddAsync(new InventoryMovement
                        {
                            ProductId = product.Id,
                            Quantity = item.ReceivedQuantity,
                            MovementType = MovementType.Decrease,
                            Reason = $"Purchase Order {purchaseOrder.OrderNumber} - Cancelled (reversal)"
                        });

                        _logger.LogWarning(
                            "Stock revertido por cancelación. ProductId: {ProductId}, Quantity: {Quantity}",
                            product.Id, item.ReceivedQuantity
                        );
                    }
                }

                purchaseOrder.Status = PurchaseOrderStatus.Cancelled;

                if (!string.IsNullOrWhiteSpace(reason))
                {
                    purchaseOrder.Notes = string.IsNullOrWhiteSpace(purchaseOrder.Notes)
                        ? $"[Cancelación]: {reason}"
                        : $"{purchaseOrder.Notes}\n[Cancelación]: {reason}";
                }

                await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error al cancelar orden. PurchaseOrderId: {Id}", id);
                return Result<PurchaseOrderDto>.Failure($"Error cancelling order: {ex.Message}");
            }
        }
        else
        {
            purchaseOrder.Status = PurchaseOrderStatus.Cancelled;

            if (!string.IsNullOrWhiteSpace(reason))
            {
                purchaseOrder.Notes = string.IsNullOrWhiteSpace(purchaseOrder.Notes)
                    ? $"[Cancelación]: {reason}"
                    : $"{purchaseOrder.Notes}\n[Cancelación]: {reason}";
            }

            await _unitOfWork.PurchaseOrders.UpdateAsync(purchaseOrder);
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogWarning(
            "Orden de compra cancelada. PurchaseOrderId: {Id}, OrderNumber: {Number}",
            id, purchaseOrder.OrderNumber
        );

        return Result<PurchaseOrderDto>.Success(MapToPurchaseOrderDto(purchaseOrder));
    }



    private async Task<Dictionary<Guid, Product>> GetProductsDictionaryAsync(List<Guid> productIds)
    {
        return await _unitOfWork.GetDbContext().Products
            .Include(p => p.ProductSuppliers)
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);
    }

    private void BuildPurchaseOrderItems(
        PurchaseOrder purchaseOrder,
        List<PurchaseOrderItemCreateDto> itemDtos,
        Dictionary<Guid, Product> products)
    {
        int sortOrder = 1;

        foreach (var itemDto in itemDtos)
        {
            var product = products[itemDto.ProductId];

            var unitCost = itemDto.UnitCost ?? GetSupplierCostForProduct(product, purchaseOrder.SupplierId);

            var lineSubtotal = (itemDto.OrderedQuantity * unitCost) - itemDto.DiscountAmount;
            var lineTax = lineSubtotal * 0.12m;
            var lineTotal = lineSubtotal + lineTax;

            var item = new PurchaseOrderItem
            {
                ProductId = itemDto.ProductId,
                Description = itemDto.Description ?? product.Name,
                SupplierSku = itemDto.SupplierSku,
                OrderedQuantity = itemDto.OrderedQuantity,
                ReceivedQuantity = 0,
                UnitCost = unitCost,
                DiscountAmount = itemDto.DiscountAmount,
                Subtotal = lineSubtotal,
                TaxAmount = lineTax,
                LineTotal = lineTotal,
                SortOrder = sortOrder++,
                Notes = itemDto.Notes
            };

            purchaseOrder.Items.Add(item);
        }
    }

    private decimal GetSupplierCostForProduct(Product product, Guid supplierId)
    {
        var supplierRelation = product.ProductSuppliers
            .FirstOrDefault(ps => ps.SupplierId == supplierId);

        return supplierRelation?.SupplierPrice ?? product.Price;
    }

    private void CalculatePurchaseOrderTotals(PurchaseOrder purchaseOrder)
    {
        purchaseOrder.Subtotal = purchaseOrder.Items.Sum(i => i.Subtotal);
        purchaseOrder.TaxAmount = purchaseOrder.Items.Sum(i => i.TaxAmount);
        purchaseOrder.Total = (purchaseOrder.Subtotal + purchaseOrder.TaxAmount + purchaseOrder.ShippingCost)
                              - purchaseOrder.DiscountAmount;
    }

    private PurchaseOrderDto MapToPurchaseOrderDto(PurchaseOrder po)
    {
        return new PurchaseOrderDto(
            Id: po.Id,
            OrderNumber: po.OrderNumber,
            SupplierId: po.SupplierId,
            SupplierName: po.Supplier.Name,
            SupplierEmail: po.Supplier.Email,
            Status: po.Status.ToString(),
            OrderDate: po.OrderDate,
            ExpectedDeliveryDate: po.ExpectedDeliveryDate,
            ActualDeliveryDate: po.ActualDeliveryDate,
            Subtotal: po.Subtotal,
            TaxAmount: po.TaxAmount,
            DiscountAmount: po.DiscountAmount,
            ShippingCost: po.ShippingCost,
            Total: po.Total,
            PaidAmount: po.PaidAmount,
            Balance: po.Balance,
            PaymentTerms: po.PaymentTerms,
            Notes: po.Notes,
            SupplierReference: po.SupplierReference,
            CreatedAt: po.CreatedAt,
            UpdatedAt: po.UpdatedAt,
            Items: po.Items.Select(item => new PurchaseOrderItemDto(
                Id: item.Id,
                ProductId: item.ProductId,
                ProductName: item.Product?.Name ?? "N/A",
                ProductSku: item.Product?.Sku ?? "N/A",
                Description: item.Description,
                SupplierSku: item.SupplierSku,
                OrderedQuantity: item.OrderedQuantity,
                ReceivedQuantity: item.ReceivedQuantity,
                PendingQuantity: item.PendingQuantity,
                UnitCost: item.UnitCost,
                DiscountAmount: item.DiscountAmount,
                Subtotal: item.Subtotal,
                TaxAmount: item.TaxAmount,
                LineTotal: item.LineTotal,
                ReceivedPercentage: item.GetReceivedPercentage()
            )).OrderBy(i => i.Id).ToList(),
            CanBeEdited: po.CanBeEdited(),
            CanBeSent: po.CanBeSent(),
            CanBeCancelled: po.CanBeCancelled(),
            CanReceiveItems: po.CanReceiveItems(),
            IsFullyReceived: po.IsFullyReceived()
        );
    }
}