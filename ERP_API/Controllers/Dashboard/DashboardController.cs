using ERP_API.Common.Constants;
using ERP_API.DTOs.Dashboard;
using ERP_API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP_API.Controllers;

/// <summary>
/// Controlador para métricas y datos del dashboard
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el resumen completo del dashboard
    /// </summary>
    /// <param name="period">Período predefinido (Today, Week, Month, etc.)</param>
    /// <param name="startDate">Fecha inicio personalizada (solo para período Custom)</param>
    /// <param name="endDate">Fecha fin personalizada (solo para período Custom)</param>
    [HttpGet("summary")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboardSummary(
        [FromQuery] DashboardPeriod period = DashboardPeriod.Month,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo resumen del dashboard. Período: {Period}, Usuario: {User}",
            period, User.Identity?.Name);

        if (period == DashboardPeriod.Custom && (!startDate.HasValue || !endDate.HasValue))
        {
            return BadRequest("Para período Custom se requieren startDate y endDate");
        }

        var result = await _dashboardService.GetDashboardSummaryAsync(
            period, startDate, endDate, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene métricas detalladas de ventas
    /// </summary>
    [HttpGet("sales-metrics")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
    [ProducesResponseType(typeof(SalesMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSalesMetrics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo métricas de ventas desde {StartDate} hasta {EndDate}",
            startDate, endDate);

        if (startDate > endDate)
        {
            return BadRequest("La fecha de inicio no puede ser mayor a la fecha de fin");
        }

        var result = await _dashboardService.GetSalesMetricsAsync(
            startDate, endDate, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene alertas de inventario (productos con stock bajo)
    /// </summary>
    [HttpGet("inventory-alerts")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager},{RoleConstants.WarehouseManager}")]
    [ProducesResponseType(typeof(InventoryMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInventoryAlerts(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo alertas de inventario");

        var result = await _dashboardService.GetInventoryAlertsAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene los productos más vendidos
    /// </summary>
    /// <param name="period">Período de análisis</param>
    /// <param name="limit">Cantidad máxima de productos a retornar</param>
    [HttpGet("top-products")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
    [ProducesResponseType(typeof(TopProductsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] DashboardPeriod period = DashboardPeriod.Month,
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo top {Limit} productos. Período: {Period}", limit, period);

        if (period == DashboardPeriod.Custom && (!startDate.HasValue || !endDate.HasValue))
        {
            return BadRequest("Para período Custom se requieren startDate y endDate");
        }

        if (limit < 1 || limit > 100)
        {
            return BadRequest("El límite debe estar entre 1 y 100");
        }

        var result = await _dashboardService.GetTopProductsAsync(
            period, limit, startDate, endDate, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene los mejores clientes
    /// </summary>
    /// <param name="period">Período de análisis</param>
    /// <param name="limit">Cantidad máxima de clientes a retornar</param>
    [HttpGet("top-customers")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
    [ProducesResponseType(typeof(TopCustomersResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopCustomers(
        [FromQuery] DashboardPeriod period = DashboardPeriod.Month,
        [FromQuery] int limit = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo top {Limit} clientes. Período: {Period}", limit, period);

        if (period == DashboardPeriod.Custom && (!startDate.HasValue || !endDate.HasValue))
        {
            return BadRequest("Para período Custom se requieren startDate y endDate");
        }

        if (limit < 1 || limit > 100)
        {
            return BadRequest("El límite debe estar entre 1 y 100");
        }

        var result = await _dashboardService.GetTopCustomersAsync(
            period, limit, startDate, endDate, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene métricas financieras generales
    /// </summary>
    [HttpGet("financial-metrics")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
    [ProducesResponseType(typeof(FinancialMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFinancialMetrics(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo métricas financieras");

        var result = await _dashboardService.GetFinancialMetricsAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene actividades recientes del sistema
    /// </summary>
    /// <param name="limit">Cantidad de actividades a retornar</param>
    [HttpGet("recent-activities")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
    [ProducesResponseType(typeof(List<RecentActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRecentActivities(
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo {Limit} actividades recientes", limit);

        if (limit < 1 || limit > 100)
        {
            return BadRequest("El límite debe estar entre 1 y 100");
        }

        var result = await _dashboardService.GetRecentActivitiesAsync(limit, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene la tendencia de ventas agrupada por período
    /// </summary>
    [HttpGet("sales-trend")]
    [Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Manager}")]
    [ProducesResponseType(typeof(SalesTrendResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSalesTrend(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Obteniendo tendencia de ventas desde {StartDate} hasta {EndDate}",
            startDate, endDate);

        if (startDate > endDate)
        {
            return BadRequest("La fecha de inicio no puede ser mayor a la fecha de fin");
        }

        var result = await _dashboardService.GetSalesTrendAsync(
            startDate, endDate, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}