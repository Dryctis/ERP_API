using AutoMapper;
using ERP_API.Common.Exceptions;
using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Services.Implementations;


public class OrderService : IOrderService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnidadDeTrabajo unitOfWork, IMapper mapper, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<OrderDto>> GetAsync(Guid id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);

        if (order is null)
            return Result<OrderDto>.Failure("Order not found");

        return Result<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }

    public async Task<Result<OrderDto>> CreateAsync(OrderCreateDto dto)
    {
        _logger.LogInformation(
            "Iniciando creación de orden. CustomerId: {CustomerId}, Items: {ItemCount}",
            dto.CustomerId, dto.Items?.Count ?? 0
        );

       
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
        if (customer is null)
        {
            _logger.LogWarning("Cliente no encontrado: {CustomerId}", dto.CustomerId);
            return Result<OrderDto>.Failure("Customer not found");
        }

        
        if (dto.Items is null || dto.Items.Count == 0)
        {
            _logger.LogWarning("Orden sin items. CustomerId: {CustomerId}", dto.CustomerId);
            return Result<OrderDto>.Failure("Order must have at least one item");
        }

        
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await GetProductsDictionaryAsync(productIds);

        if (products.Count != productIds.Count)
        {
            _logger.LogWarning(
                "Productos no encontrados. CustomerId: {CustomerId}, ProductIds solicitados: {RequestedCount}, Encontrados: {FoundCount}",
                dto.CustomerId, productIds.Count, products.Count
            );
            return Result<OrderDto>.Failure("One or more products not found");
        }

        
        foreach (var item in dto.Items)
        {
            var product = products[item.ProductId];
            if (product.Stock < item.Quantity)
            {
                _logger.LogWarning(
                    "Stock insuficiente al crear orden. ProductId: {ProductId}, Disponible: {Disponible}, Requerido: {Requerido}",
                    product.Id, product.Stock, item.Quantity
                );
                return Result<OrderDto>.Failure($"Insufficient stock for product {product.Name}");
            }
        }

      
        var order = BuildOrder(dto, products);

        _logger.LogInformation(
            "Orden construida. CustomerId: {CustomerId}, Subtotal: {Subtotal}, Total: {Total}, Items: {ItemCount}",
            dto.CustomerId, order.Subtotal, order.Total, order.Items.Count
        );

        
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            
            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];
                var stockAnterior = product.Stock;

                product.Stock -= item.Quantity;

                _logger.LogDebug(
                    "Actualizando stock en orden. ProductId: {ProductId}, Anterior: {StockAnterior}, Decremento: {Quantity}, Nuevo: {StockNuevo}",
                    product.Id, stockAnterior, item.Quantity, product.Stock
                );

                await _unitOfWork.Products.UpdateAsync(product);

                await _unitOfWork.Inventory.AddAsync(new InventoryMovement
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    MovementType = MovementType.Decrease,
                    Reason = $"Order {order.Id}"
                });
            }

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Orden creada exitosamente. OrderId: {OrderId}, CustomerId: {CustomerId}, Total: {Total}, Items: {ItemCount}",
                order.Id, customer.Id, order.Total, order.Items.Count
            );
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();

            _logger.LogError(ex, "Conflicto de concurrencia al crear orden. CustomerId: {CustomerId}", dto.CustomerId);
            return Result<OrderDto>.Failure("Concurrency conflict: One or more products were modified. Please try again.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();

            _logger.LogError(ex, "Error al crear orden. CustomerId: {CustomerId}", dto.CustomerId);
            return Result<OrderDto>.Failure($"Error creating order: {ex.Message}");
        }

        var createdOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id);
        return Result<OrderDto>.Success(_mapper.Map<OrderDto>(createdOrder!));
    }

    private async Task<Dictionary<Guid, Product>> GetProductsDictionaryAsync(List<Guid> productIds)
    {
        return await _unitOfWork.GetDbContext().Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);
    }

    private Order BuildOrder(OrderCreateDto dto, Dictionary<Guid, Product> products)
    {
        decimal subtotal = 0m;
        var order = new Order { CustomerId = dto.CustomerId };

        foreach (var itemDto in dto.Items)
        {
            var product = products[itemDto.ProductId];
            var unitPrice = product.Price;
            var lineTotal = unitPrice * itemDto.Quantity;

            subtotal += lineTotal;

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                UnitPrice = unitPrice,
                Quantity = itemDto.Quantity,
                LineTotal = lineTotal
            });
        }

        const decimal taxRate = 0.12m;
        order.Subtotal = subtotal;
        order.Tax = Math.Round(order.Subtotal * taxRate, 2, MidpointRounding.AwayFromZero);
        order.Total = order.Subtotal + order.Tax;

        return order;
    }
}