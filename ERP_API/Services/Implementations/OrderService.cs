using AutoMapper;
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

    public OrderService(IUnidadDeTrabajo unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrderDto?> GetAsync(Guid id)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id);
        return order is null ? null : _mapper.Map<OrderDto>(order);
    }

    
    public async Task<(bool ok, string? error, OrderDto? dto)> CreateAsync(OrderCreateDto dto)
    {
        
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
        if (customer is null)
            return (false, "CustomerNotFound", null);

       
        if (dto.Items is null || dto.Items.Count == 0)
            return (false, "NoItems", null);

      
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await GetProductsDictionaryAsync(productIds);

        if (products.Count != productIds.Count)
            return (false, "ProductNotFound", null);

        
        foreach (var item in dto.Items)
        {
            var product = products[item.ProductId];
            if (product.Stock < item.Quantity)
                return (false, $"InsufficientStock:{product.Id}", null);
        }

       
        var order = BuildOrder(dto, products);

       
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            
            foreach (var item in dto.Items)
            {
                var product = products[item.ProductId];
                product.Stock -= item.Quantity;

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
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return (false, $"Error: {ex.Message}", null);
        }

        
        var createdOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id);
        return (true, null, _mapper.Map<OrderDto>(createdOrder!));
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
        var order = new Order
        {
            CustomerId = dto.CustomerId
        };

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