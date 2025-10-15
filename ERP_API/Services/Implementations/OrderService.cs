using AutoMapper;
using ERP_API.Data;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IOrderRepository _orders;
    private readonly IMapper _mapper;

    public OrderService(AppDbContext db, IOrderRepository orders, IMapper mapper)
    {
        _db = db; _orders = orders; _mapper = mapper;
    }

    public async Task<OrderDto?> GetAsync(Guid id)
    {
        var o = await _orders.GetByIdAsync(id);
        return o is null ? null : _mapper.Map<OrderDto>(o);
    }

    public async Task<(bool ok, string? error, OrderDto? dto)> CreateAsync(OrderCreateDto dto)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == dto.CustomerId);
        if (customer is null) return (false, "CustomerNotFound", null);
        if (dto.Items is null || dto.Items.Count == 0) return (false, "NoItems", null);

        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        if (products.Count != productIds.Distinct().Count()) return (false, "ProductNotFound", null);

        foreach (var it in dto.Items)
        {
            var p = products[it.ProductId];
            if (p.Stock < it.Quantity) return (false, $"InsufficientStock:{p.Id}", null);
        }

        decimal subtotal = 0m;
        var order = new Order { CustomerId = customer.Id };

        foreach (var it in dto.Items)
        {
            var p = products[it.ProductId];
            var unit = p.Price;
            var line = unit * it.Quantity;
            subtotal += line;

            order.Items.Add(new OrderItem
            {
                ProductId = p.Id,
                UnitPrice = unit,
                Quantity = it.Quantity,
                LineTotal = line
            });
        }

        var taxRate = 0.12m;
        order.Subtotal = subtotal;
        order.Tax = Math.Round(order.Subtotal * taxRate, 2, MidpointRounding.AwayFromZero);
        order.Total = order.Subtotal + order.Tax;

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var it in dto.Items)
            {
                var p = products[it.ProductId];
                p.Stock -= it.Quantity;
                _db.InventoryMovements.Add(new InventoryMovement
                {
                    ProductId = p.Id,
                    Quantity = it.Quantity,
                    MovementType = MovementType.Decrease,
                    Reason = "Order"
                });
            }

            await _orders.AddAsync(order);
            await _orders.SaveAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            return (false, "Error", null);
        }

        var created = await _orders.GetByIdAsync(order.Id);
        return (true, null, _mapper.Map<OrderDto>(created!));
    }
}
