using ERP_API.Common.Helpers;
using ERP_API.Common.Results;
using ERP_API.DTOs.Dashboard;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace ERP_API.Services.Implementations;

/// <summary>
/// Servicio para obtener métricas y datos del dashboard
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IUnidadDeTrabajo unitOfWork,
        ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DashboardSummaryDto>> GetDashboardSummaryAsync(
        DashboardPeriod period,
        DateTime? customStartDate = null,
        DateTime? customEndDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo resumen del dashboard para período: {Period}", period);

            var (startDate, endDate) = DatePeriodHelper.GetDateRange(period, customStartDate, customEndDate);

            var salesTask = GetSalesMetricsAsync(startDate, endDate, cancellationToken);
            var inventoryTask = GetInventoryMetricsAsync(cancellationToken);
            var financialTask = GetFinancialMetricsAsync(cancellationToken);
            var activitiesTask = GetRecentActivitiesAsync(20, cancellationToken);

            await Task.WhenAll(salesTask, inventoryTask, financialTask, activitiesTask);

            if (!salesTask.Result.IsSuccess || !inventoryTask.Result.IsSuccess ||
                !financialTask.Result.IsSuccess || !activitiesTask.Result.IsSuccess)
            {
                _logger.LogWarning("Error al obtener algunas métricas del dashboard");
                return Result<DashboardSummaryDto>.Failure("Error al obtener métricas del dashboard");
            }

            var summary = new DashboardSummaryDto
            {
                Sales = salesTask.Result.Value!,
                Inventory = inventoryTask.Result.Value!,
                Financial = financialTask.Result.Value!,
                RecentActivities = activitiesTask.Result.Value!,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Resumen del dashboard generado exitosamente");
            return Result<DashboardSummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen del dashboard");
            return Result<DashboardSummaryDto>.Failure("Error al obtener resumen del dashboard");
        }
    }

    public async Task<Result<SalesMetricsDto>> GetSalesMetricsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo métricas de ventas desde {StartDate} hasta {EndDate}", startDate, endDate);

            var (prevStartDate, prevEndDate) = DatePeriodHelper.GetPreviousPeriodRange(
                DashboardPeriod.Custom, startDate, endDate);

            var dbContext = _unitOfWork.GetDbContext();

            var currentOrders = await dbContext.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToListAsync(cancellationToken);

            var currentInvoices = await dbContext.Invoices
                .AsNoTracking()
                .Where(i => !i.IsDeleted && i.IssueDate >= startDate && i.IssueDate <= endDate)
                .ToListAsync(cancellationToken);

            var previousOrders = await dbContext.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.CreatedAt >= prevStartDate && o.CreatedAt <= prevEndDate)
                .CountAsync(cancellationToken);

            var previousInvoices = await dbContext.Invoices
                .AsNoTracking()
                .Where(i => !i.IsDeleted && i.IssueDate >= prevStartDate && i.IssueDate <= prevEndDate)
                .ToListAsync(cancellationToken);

            var totalSales = currentInvoices.Sum(i => i.Total);
            var totalOrders = currentOrders.Count;
            var paidAmount = currentInvoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.Total);
            var pendingAmount = currentInvoices
                .Where(i => i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Draft)
                .Sum(i => i.Balance);

            var previousPeriodSales = previousInvoices.Sum(i => i.Total);

            var metrics = new SalesMetricsDto
            {
                TotalSales = totalSales,
                PreviousPeriodSales = previousPeriodSales,
                SalesGrowthPercentage = DatePeriodHelper.CalculateGrowthPercentage(totalSales, previousPeriodSales),
                TotalOrders = totalOrders,
                PreviousPeriodOrders = previousOrders,
                AverageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0,
                TotalInvoices = currentInvoices.Count,
                PaidAmount = paidAmount,
                PendingAmount = pendingAmount
            };

            _logger.LogInformation("Métricas de ventas obtenidas: TotalSales={TotalSales}, Orders={Orders}, Growth={Growth}%",
                metrics.TotalSales, metrics.TotalOrders, metrics.SalesGrowthPercentage);

            return Result<SalesMetricsDto>.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas de ventas");
            return Result<SalesMetricsDto>.Failure("Error al obtener métricas de ventas");
        }
    }

    public async Task<Result<InventoryMetricsDto>> GetInventoryMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo métricas de inventario");

            var dbContext = _unitOfWork.GetDbContext();

            var products = await dbContext.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .ToListAsync(cancellationToken);

            // Como Product no tiene MinimumStock, usamos un threshold fijo
            const int lowStockThreshold = 10;

            var lowStockProducts = products
                .Where(p => p.Stock <= lowStockThreshold && p.Stock > 0)
                .Select(p => new LowStockProductDto
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Sku = p.Sku,
                    CurrentStock = p.Stock,
                    MinimumStock = lowStockThreshold,
                    ReorderLevel = lowStockThreshold * 2,
                    UnitPrice = p.Price
                })
                .OrderBy(p => p.CurrentStock)
                .Take(10)
                .ToList();

            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentMovements = await _unitOfWork.Inventory.GetAllAsync();
            var recentMovementsList = recentMovements
                .Where(m => m.CreatedAt >= thirtyDaysAgo)
                .ToList();

            var productsWithMovement = recentMovementsList
                .Select(m => m.ProductId)
                .Distinct()
                .ToHashSet();

            var productsWithoutMovement = products.Count(p => !productsWithMovement.Contains(p.Id));

            var metrics = new InventoryMetricsDto
            {
                TotalProducts = products.Count,
                LowStockProducts = products.Count(p => p.Stock <= lowStockThreshold && p.Stock > 0),
                OutOfStockProducts = products.Count(p => p.Stock == 0),
                TotalInventoryValue = products.Sum(p => p.Stock * p.Price),
                ProductsWithoutMovement = productsWithoutMovement,
                LowStockAlerts = lowStockProducts
            };

            _logger.LogInformation("Métricas de inventario obtenidas: Total={Total}, LowStock={LowStock}, OutOfStock={OutOfStock}",
                metrics.TotalProducts, metrics.LowStockProducts, metrics.OutOfStockProducts);

            return Result<InventoryMetricsDto>.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas de inventario");
            return Result<InventoryMetricsDto>.Failure("Error al obtener métricas de inventario");
        }
    }

    public async Task<Result<FinancialMetricsDto>> GetFinancialMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo métricas financieras");

            var dbContext = _unitOfWork.GetDbContext();
            var today = DateTime.UtcNow.Date;

            var pendingInvoices = await dbContext.Invoices
                .AsNoTracking()
                .Where(i => !i.IsDeleted && (i.Status == InvoiceStatus.Sent || i.Status == InvoiceStatus.Draft))
                .ToListAsync(cancellationToken);

            var accountsReceivable = pendingInvoices.Sum(i => i.Balance);

            var overdueInvoices = pendingInvoices.Where(i => i.DueDate < today).ToList();
            var overdueAmount = overdueInvoices.Sum(i => i.Balance);

            var thirtyDaysFromNow = today.AddDays(30);
            var expectedCollections = pendingInvoices
                .Where(i => i.DueDate >= today && i.DueDate <= thirtyDaysFromNow)
                .Sum(i => i.Balance);

            var pendingPurchaseOrders = await dbContext.PurchaseOrders
                .AsNoTracking()
                .Where(po => !po.IsDeleted &&
                    (po.Status == PurchaseOrderStatus.Draft ||
                     po.Status == PurchaseOrderStatus.Sent ||
                     po.Status == PurchaseOrderStatus.Confirmed))
                .ToListAsync(cancellationToken);

            var accountsPayable = pendingPurchaseOrders.Sum(po => po.Total);

            var metrics = new FinancialMetricsDto
            {
                AccountsReceivable = accountsReceivable,
                OverdueInvoices = overdueAmount,
                OverdueInvoicesCount = overdueInvoices.Count,
                ExpectedCollections30Days = expectedCollections,
                AccountsPayable = accountsPayable,
                PendingPurchaseOrders = accountsPayable,
                PendingPurchaseOrdersCount = pendingPurchaseOrders.Count,
                NetCashFlow = accountsReceivable - accountsPayable
            };

            _logger.LogInformation("Métricas financieras obtenidas: AR={AR}, AP={AP}, NetCashFlow={NetCashFlow}",
                metrics.AccountsReceivable, metrics.AccountsPayable, metrics.NetCashFlow);

            return Result<FinancialMetricsDto>.Success(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener métricas financieras");
            return Result<FinancialMetricsDto>.Failure("Error al obtener métricas financieras");
        }
    }

    public async Task<Result<TopProductsResponseDto>> GetTopProductsAsync(
        DashboardPeriod period,
        int limit = 10,
        DateTime? customStartDate = null,
        DateTime? customEndDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo top {Limit} productos para período: {Period}", limit, period);

            var (startDate, endDate) = DatePeriodHelper.GetDateRange(period, customStartDate, customEndDate);
            var dbContext = _unitOfWork.GetDbContext();

            // Query optimizada: se ejecuta completamente en SQL con GROUP BY
            // Evita traer todos los OrderItems a memoria
            var topProducts = await dbContext.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .Where(oi => !oi.Order.IsDeleted &&
                             oi.Order.CreatedAt >= startDate &&
                             oi.Order.CreatedAt <= endDate)
                .GroupBy(oi => new
                {
                    oi.ProductId,
                    oi.Product.Name,
                    oi.Product.Sku
                })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    Sku = g.Key.Sku,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.LineTotal),
                    OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
                })
                .OrderByDescending(p => p.TotalRevenue)
                .Take(limit)
                .ToListAsync(cancellationToken);

            var response = new TopProductsResponseDto
            {
                Products = topProducts,
                Period = DatePeriodHelper.GetPeriodDescription(period, startDate, endDate),
                StartDate = startDate,
                EndDate = endDate
            };

            _logger.LogInformation("Top productos obtenidos: {Count} productos", topProducts.Count);
            return Result<TopProductsResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top productos");
            return Result<TopProductsResponseDto>.Failure("Error al obtener top productos");
        }
    }

    public async Task<Result<TopCustomersResponseDto>> GetTopCustomersAsync(
        DashboardPeriod period,
        int limit = 10,
        DateTime? customStartDate = null,
        DateTime? customEndDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo top {Limit} clientes para período: {Period}", limit, period);

            var (startDate, endDate) = DatePeriodHelper.GetDateRange(period, customStartDate, customEndDate);

            var dbContext = _unitOfWork.GetDbContext();

            var orders = await dbContext.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Where(o => !o.IsDeleted && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToListAsync(cancellationToken);

            var topCustomers = orders
                .GroupBy(o => new { o.CustomerId, o.Customer.Name, o.Customer.Email })
                .Select(g => new TopCustomerDto
                {
                    CustomerId = g.Key.CustomerId,
                    Name = g.Key.Name,
                    Email = g.Key.Email,
                    TotalPurchases = g.Sum(o => o.Total),
                    OrderCount = g.Count(),
                    LastPurchaseDate = g.Max(o => o.CreatedAt)
                })
                .OrderByDescending(c => c.TotalPurchases)
                .Take(limit)
                .ToList();

            var response = new TopCustomersResponseDto
            {
                Customers = topCustomers,
                Period = DatePeriodHelper.GetPeriodDescription(period, startDate, endDate),
                StartDate = startDate,
                EndDate = endDate
            };

            _logger.LogInformation("Top clientes obtenidos: {Count} clientes", topCustomers.Count);
            return Result<TopCustomersResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener top clientes");
            return Result<TopCustomersResponseDto>.Failure("Error al obtener top clientes");
        }
    }

    public async Task<Result<InventoryMetricsDto>> GetInventoryAlertsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetInventoryMetricsAsync(cancellationToken);
    }

    public async Task<Result<List<RecentActivityDto>>> GetRecentActivitiesAsync(
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo {Limit} actividades recientes", limit);

            var dbContext = _unitOfWork.GetDbContext();
            var activities = new List<RecentActivityDto>();

            var recentOrders = await dbContext.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.CreatedAt)
                .Take(limit / 3)
                .ToListAsync(cancellationToken);

            activities.AddRange(recentOrders.Select(o => new RecentActivityDto
            {
                Type = "Order",
                EntityId = o.Id,
                Description = $"Nueva orden de {o.Customer.Name}",
                Amount = o.Total,
                CreatedAt = o.CreatedAt,
                CreatedBy = o.Customer.Name
            }));

            var recentInvoices = await dbContext.Invoices
                .AsNoTracking()
                .Include(i => i.Customer)
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.IssueDate)
                .Take(limit / 3)
                .ToListAsync(cancellationToken);

            activities.AddRange(recentInvoices.Select(i => new RecentActivityDto
            {
                Type = "Invoice",
                EntityId = i.Id,
                Description = $"Factura {i.InvoiceNumber} emitida a {i.Customer.Name}",
                Amount = i.Total,
                CreatedAt = i.IssueDate,
                CreatedBy = "Sistema"
            }));

            var recentPurchaseOrders = await dbContext.PurchaseOrders
                .AsNoTracking()
                .Include(po => po.Supplier)
                .Where(po => !po.IsDeleted)
                .OrderByDescending(po => po.OrderDate)
                .Take(limit / 3)
                .ToListAsync(cancellationToken);

            activities.AddRange(recentPurchaseOrders.Select(po => new RecentActivityDto
            {
                Type = "PurchaseOrder",
                EntityId = po.Id,
                Description = $"Orden de compra {po.OrderNumber} a {po.Supplier.Name}",
                Amount = po.Total,
                CreatedAt = po.OrderDate,
                CreatedBy = "Sistema"
            }));

            var sortedActivities = activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToList();

            _logger.LogInformation("Actividades recientes obtenidas: {Count} actividades", sortedActivities.Count);
            return Result<List<RecentActivityDto>>.Success(sortedActivities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener actividades recientes");
            return Result<List<RecentActivityDto>>.Failure("Error al obtener actividades recientes");
        }
    }

    public async Task<Result<SalesTrendResponseDto>> GetSalesTrendAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obteniendo tendencia de ventas desde {StartDate} hasta {EndDate}", startDate, endDate);

            var groupingType = DatePeriodHelper.GetGroupingType(startDate, endDate);
            var dbContext = _unitOfWork.GetDbContext();

            var orders = await dbContext.Orders
                .AsNoTracking()
                .Where(o => !o.IsDeleted && o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .ToListAsync(cancellationToken);

            List<SalesTrendDto> trends;

            if (groupingType == "Day")
            {
                trends = orders
                    .GroupBy(o => o.CreatedAt.Date)
                    .Select(g => new SalesTrendDto
                    {
                        Date = g.Key,
                        TotalSales = g.Sum(o => o.Total),
                        OrderCount = g.Count(),
                        AverageOrderValue = g.Average(o => o.Total)
                    })
                    .OrderBy(t => t.Date)
                    .ToList();
            }
            else if (groupingType == "Week")
            {
                trends = orders
                    .GroupBy(o => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                        o.CreatedAt, CalendarWeekRule.FirstDay, DayOfWeek.Monday))
                    .Select(g => new SalesTrendDto
                    {
                        Date = g.First().CreatedAt.Date,
                        TotalSales = g.Sum(o => o.Total),
                        OrderCount = g.Count(),
                        AverageOrderValue = g.Average(o => o.Total)
                    })
                    .OrderBy(t => t.Date)
                    .ToList();
            }
            else
            {
                trends = orders
                    .GroupBy(o => new DateTime(o.CreatedAt.Year, o.CreatedAt.Month, 1))
                    .Select(g => new SalesTrendDto
                    {
                        Date = g.Key,
                        TotalSales = g.Sum(o => o.Total),
                        OrderCount = g.Count(),
                        AverageOrderValue = g.Average(o => o.Total)
                    })
                    .OrderBy(t => t.Date)
                    .ToList();
            }

            var response = new SalesTrendResponseDto
            {
                Trends = trends,
                Period = DatePeriodHelper.GetPeriodDescription(DashboardPeriod.Custom, startDate, endDate),
                GroupBy = groupingType,
                StartDate = startDate,
                EndDate = endDate
            };

            _logger.LogInformation("Tendencia de ventas obtenida: {Count} períodos agrupados por {GroupBy}",
                trends.Count, groupingType);

            return Result<SalesTrendResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tendencia de ventas");
            return Result<SalesTrendResponseDto>.Failure("Error al obtener tendencia de ventas");
        }
    }
}