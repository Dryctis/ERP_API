using ERP_API.Common.Results;
using ERP_API.DTOs.Dashboard;

namespace ERP_API.Services.Interfaces;

/// <summary>
/// Servicio para obtener métricas y datos del dashboard
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Obtiene el resumen completo del dashboard para el período especificado
    /// </summary>
    Task<Result<DashboardSummaryDto>> GetDashboardSummaryAsync(
        DashboardPeriod period,
        DateTime? customStartDate = null,
        DateTime? customEndDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene métricas detalladas de ventas
    /// </summary>
    Task<Result<SalesMetricsDto>> GetSalesMetricsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene métricas de inventario
    /// </summary>
    Task<Result<InventoryMetricsDto>> GetInventoryMetricsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene métricas financieras
    /// </summary>
    Task<Result<FinancialMetricsDto>> GetFinancialMetricsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los productos más vendidos
    /// </summary>
    Task<Result<TopProductsResponseDto>> GetTopProductsAsync(
        DashboardPeriod period,
        int limit = 10,
        DateTime? customStartDate = null,
        DateTime? customEndDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene los mejores clientes
    /// </summary>
    Task<Result<TopCustomersResponseDto>> GetTopCustomersAsync(
        DashboardPeriod period,
        int limit = 10,
        DateTime? customStartDate = null,
        DateTime? customEndDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene alertas de inventario (stock bajo, sin movimiento, etc.)
    /// </summary>
    Task<Result<InventoryMetricsDto>> GetInventoryAlertsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene actividades recientes del sistema
    /// </summary>
    Task<Result<List<RecentActivityDto>>> GetRecentActivitiesAsync(
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la tendencia de ventas agrupada por período
    /// </summary>
    Task<Result<SalesTrendResponseDto>> GetSalesTrendAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}