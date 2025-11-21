using ERP_API.Entities;

namespace ERP_API.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task AddAsync(Order order);

    /// <summary>
    /// Obtiene los items de órdenes en un período específico
    /// </summary>
    Task<IEnumerable<OrderItem>> GetOrderItemsInPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}