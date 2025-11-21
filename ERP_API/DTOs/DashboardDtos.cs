namespace ERP_API.DTOs.Dashboard;

/// <summary>
/// Resumen general del dashboard
/// </summary>
public record DashboardSummaryDto
{
    public SalesMetricsDto Sales { get; init; } = null!;
    public InventoryMetricsDto Inventory { get; init; } = null!;
    public FinancialMetricsDto Financial { get; init; } = null!;
    public List<RecentActivityDto> RecentActivities { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Métricas de ventas
/// </summary>
public record SalesMetricsDto
{
    public decimal TotalSales { get; init; }
    public decimal PreviousPeriodSales { get; init; }
    public decimal SalesGrowthPercentage { get; init; }
    public int TotalOrders { get; init; }
    public int PreviousPeriodOrders { get; init; }
    public decimal AverageOrderValue { get; init; }
    public int TotalInvoices { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal PendingAmount { get; init; }
}

/// <summary>
/// Métricas de inventario
/// </summary>
public record InventoryMetricsDto
{
    public int TotalProducts { get; init; }
    public int LowStockProducts { get; init; }
    public int OutOfStockProducts { get; init; }
    public decimal TotalInventoryValue { get; init; }
    public int ProductsWithoutMovement { get; init; }
    public List<LowStockProductDto> LowStockAlerts { get; init; } = new();
}

/// <summary>
/// Producto con stock bajo
/// </summary>
public record LowStockProductDto
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public int CurrentStock { get; init; }
    public int MinimumStock { get; init; }
    public int ReorderLevel { get; init; }
    public decimal UnitPrice { get; init; }
}

/// <summary>
/// Métricas financieras
/// </summary>
public record FinancialMetricsDto
{
    public decimal AccountsReceivable { get; init; }
    public decimal OverdueInvoices { get; init; }
    public int OverdueInvoicesCount { get; init; }
    public decimal ExpectedCollections30Days { get; init; }
    public decimal AccountsPayable { get; init; }
    public decimal PendingPurchaseOrders { get; init; }
    public int PendingPurchaseOrdersCount { get; init; }
    public decimal NetCashFlow { get; init; }
}

/// <summary>
/// Actividad reciente en el sistema
/// </summary>
public record RecentActivityDto
{
    public string Type { get; init; } = string.Empty; // "Order", "Invoice", "Payment", "PurchaseOrder"
    public Guid EntityId { get; init; }
    public string Description { get; init; } = string.Empty;
    public decimal? Amount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}

/// <summary>
/// Producto más vendido
/// </summary>
public record TopProductDto
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public int QuantitySold { get; init; }
    public decimal TotalRevenue { get; init; }
    public int OrderCount { get; init; }
}

/// <summary>
/// Cliente principal
/// </summary>
public record TopCustomerDto
{
    public Guid CustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public decimal TotalPurchases { get; init; }
    public int OrderCount { get; init; }
    public DateTime LastPurchaseDate { get; init; }
}

/// <summary>
/// Respuesta de productos top
/// </summary>
public record TopProductsResponseDto
{
    public List<TopProductDto> Products { get; init; } = new();
    public string Period { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

/// <summary>
/// Respuesta de clientes top
/// </summary>
public record TopCustomersResponseDto
{
    public List<TopCustomerDto> Customers { get; init; } = new();
    public string Period { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

/// <summary>
/// Tendencia de ventas por período
/// </summary>
public record SalesTrendDto
{
    public DateTime Date { get; init; }
    public decimal TotalSales { get; init; }
    public int OrderCount { get; init; }
    public decimal AverageOrderValue { get; init; }
}

/// <summary>
/// Respuesta de tendencia de ventas
/// </summary>
public record SalesTrendResponseDto
{
    public List<SalesTrendDto> Trends { get; init; } = new();
    public string Period { get; init; } = string.Empty;
    public string GroupBy { get; init; } = string.Empty; // "Day", "Week", "Month"
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

/// <summary>
/// Enumeración de períodos predefinidos
/// </summary>
public enum DashboardPeriod
{
    Today,
    Yesterday,
    Week,
    Month,
    Quarter,
    Year,
    Custom
}