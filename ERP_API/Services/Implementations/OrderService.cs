using AutoMapper;
using ERP_API.Common.Configuration;
using ERP_API.Common.Exceptions;
using ERP_API.Common.Helpers;
using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Helpers;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly ITaxCalculator _taxCalculator;
    private readonly OrderCalculationHelper _calculationHelper;
    private readonly OrderMapper _orderMapper;

    public OrderService(
        IUnidadDeTrabajo unitOfWork,
        IMapper mapper,
        ILogger<OrderService> logger,
        ITaxCalculator taxCalculator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _taxCalculator = taxCalculator;
        _calculationHelper = new OrderCalculationHelper(taxCalculator);
        _orderMapper = new OrderMapper();
    }

    public async Task<Result<OrderDto>> GetAsync(Guid id)
    {
        _logger.LogDebug("Consultando orden. OrderId: {OrderId}", id);

        var order = await _unitOfWork.Orders.GetByIdAsync(id);

        if (order is null)
        {
            _logger.LogWarning("Orden no encontrada. OrderId: {OrderId}", id);
            return Result<OrderDto>.Failure("Order not found");
        }

        var dto = _orderMapper.MapToDto(order);

        _logger.LogDebug(
            "Orden encontrada. OrderId: {OrderId}, Customer: {CustomerName}, Total: {Total}",
            id, order.Customer.Name, order.Total
        );

        return Result<OrderDto>.Success(dto);
    }

    public async Task<Result<OrderDto>> CreateAsync(OrderCreateDto dto)
    {
        _logger.LogInformation(
            "Iniciando creación de orden. CustomerId: {CustomerId}, Items: {ItemCount}",
            dto.CustomerId, dto.Items?.Count ?? 0
        );

       
        var customerValidation = await ValidateCustomerAsync(dto.CustomerId);
        var itemsValidation = ValidateOrderItems(dto.Items);

        var validationResult = Result.Combine(customerValidation, itemsValidation);
        if (validationResult.IsFailure)
            return Result<OrderDto>.Failure(validationResult.Error!);

       
        var productsResult = await GetAndValidateProductsAsync(dto.Items!);
        if (productsResult.IsFailure)
            return Result<OrderDto>.Failure(productsResult.Error!);

        var products = productsResult.Value!;

    
        var stockValidation = ValidateStock(dto.Items!, products);
        if (stockValidation.IsFailure)
            return Result<OrderDto>.Failure(stockValidation.Error!);

     
        var order = BuildOrder(dto, products);

        _logger.LogInformation(
            "Orden construida. CustomerId: {CustomerId}, Subtotal: {Subtotal}, Total: {Total}, Items: {ItemCount}",
            dto.CustomerId, order.Subtotal, order.Total, order.Items.Count
        );

       
        return await ExecuteCreateOrderTransactionAsync(order, dto.Items!, products);
    }

    #region Private Methods - Validation

    private async Task<Result> ValidateCustomerAsync(Guid customerId)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);

        if (customer is null)
        {
            _logger.LogWarning("Cliente no encontrado: {CustomerId}", customerId);
            return Result.Failure("Customer not found");
        }

        return Result.Success();
    }

    private Result ValidateOrderItems(List<OrderItemCreateDto>? items)
    {
        if (items is null || items.Count == 0)
        {
            _logger.LogWarning("Orden sin items");
            return Result.Failure("Order must have at least one item");
        }
        if (items.Count > BusinessConstants.Orders.MaxOrderItems)
        {
            _logger.LogWarning("Orden excede máximo de items: {Count}", items.Count);
            return Result.Failure($"Order cannot have more than {BusinessConstants.Orders.MaxOrderItems} items");
        }
        foreach (var item in items)
        {
            if (item.Quantity <= 0)
            {
                _logger.LogWarning("Cantidad inválida: {Quantity}", item.Quantity);
                return Result.Failure("Item quantity must be greater than zero");
            }
            if (item.Quantity > BusinessConstants.Orders.MaxProductQuantityPerItem)
            {
                _logger.LogWarning("Cantidad excede máximo permitido: {Quantity}", item.Quantity);
                return Result.Failure($"Item quantity cannot exceed {BusinessConstants.Orders.MaxProductQuantityPerItem}");
            }
        }
        return Result.Success();
    }

    private async Task<Result<Dictionary<Guid, Product>>> GetAndValidateProductsAsync(
        List<OrderItemCreateDto> items)
    {
        var productIds = items.Select(i => i.ProductId).Distinct().ToList();
        var products = await GetProductsDictionaryAsync(productIds);

        if (products.Count != productIds.Count)
        {
            var missingIds = productIds.Except(products.Keys).ToList();
            _logger.LogWarning(
                "Productos no encontrados. ProductIds solicitados: {RequestedCount}, Encontrados: {FoundCount}, Faltantes: {MissingIds}",
                productIds.Count, products.Count, string.Join(", ", missingIds)
            );
            return Result<Dictionary<Guid, Product>>.Failure("One or more products not found");
        }

        return Result<Dictionary<Guid, Product>>.Success(products);
    }

    private Result ValidateStock(List<OrderItemCreateDto> items, Dictionary<Guid, Product> products)
    {
        foreach (var item in items)
        {
            var product = products[item.ProductId];

            if (product.Stock < item.Quantity)
            {
                _logger.LogWarning(
                    "Stock insuficiente. ProductId: {ProductId}, ProductName: {ProductName}, Disponible: {Stock}, Requerido: {Quantity}",
                    product.Id, product.Name, product.Stock, item.Quantity
                );

                
                throw new StockInsuficienteException(
                    product.Id,
                    product.Name,
                    product.Stock,
                    item.Quantity
                );
            }
        }

        return Result.Success();
    }

    #endregion

    #region Private Methods - Data Access

    private async Task<Dictionary<Guid, Product>> GetProductsDictionaryAsync(List<Guid> productIds)
    {
        return await _unitOfWork.GetDbContext().Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);
    }

    #endregion

    #region Private Methods - Order Building

    private Order BuildOrder(OrderCreateDto dto, Dictionary<Guid, Product> products)
    {
        var order = new Order { CustomerId = dto.CustomerId };

      
        var calculations = _calculationHelper.CalculateOrderTotals(dto.Items!, products);

    
        foreach (var itemDto in dto.Items!)
        {
            var product = products[itemDto.ProductId];
            var itemCalculation = calculations.ItemCalculations
                .First(ic => ic.ProductId == itemDto.ProductId);

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                UnitPrice = itemCalculation.UnitPrice,
                Quantity = itemCalculation.Quantity,
                LineTotal = itemCalculation.LineTotal
            });
        }

        
        order.Subtotal = calculations.Subtotal;
        order.Tax = calculations.TaxAmount;
        order.Total = calculations.Total;

        _logger.LogDebug(
            "Orden construida con cálculos. Subtotal: {Subtotal}, IVA: {Tax}, Total: {Total}, Items: {ItemCount}",
            order.Subtotal, order.Tax, order.Total, order.Items.Count
        );

        return order;
    }

    #endregion

    #region Private Methods - Transaction

    private async Task<Result<OrderDto>> ExecuteCreateOrderTransactionAsync(
        Order order,
        List<OrderItemCreateDto> items,
        Dictionary<Guid, Product> products)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
           
            await ProcessStockReductionAsync(items, products, order.Id);

           
            await _unitOfWork.Orders.AddAsync(order);

          
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation(
                "Orden creada exitosamente. OrderId: {OrderId}, CustomerId: {CustomerId}, Total: {Total}, Items: {ItemCount}",
                order.Id, order.CustomerId, order.Total, order.Items.Count
            );

         
            var createdOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id);
            return Result<OrderDto>.Success(_orderMapper.MapToDto(createdOrder!));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();

            _logger.LogError(
                ex,
                "Conflicto de concurrencia al crear orden. CustomerId: {CustomerId}",
                order.CustomerId
            );

            return Result<OrderDto>.Failure(
                "Concurrency conflict: One or more products were modified. Please try again."
            );
        }
        catch (StockInsuficienteException)
        {
            
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();

            _logger.LogError(ex, "Error al crear orden. CustomerId: {CustomerId}", order.CustomerId);

            return Result<OrderDto>.Failure($"Error creating order: {ex.Message}");
        }
    }

    private async Task ProcessStockReductionAsync(
        List<OrderItemCreateDto> items,
        Dictionary<Guid, Product> products,
        Guid orderId)
    {
        foreach (var item in items)
        {
            var product = products[item.ProductId];
            var stockAnterior = product.Stock;

           
            product.Stock -= item.Quantity;

            _logger.LogDebug(
                "Actualizando stock. ProductId: {ProductId}, ProductName: {ProductName}, Anterior: {StockAnterior}, Decremento: {Quantity}, Nuevo: {StockNuevo}",
                product.Id, product.Name, stockAnterior, item.Quantity, product.Stock
            );

            await _unitOfWork.Products.UpdateAsync(product);

            
            await _unitOfWork.Inventory.AddAsync(new InventoryMovement
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                MovementType = MovementType.Decrease,
                Reason = $"Order {orderId}"
            });
        }
    }

    #endregion
}