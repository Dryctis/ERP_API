using ERP_API.DTOs.Dashboard;

namespace ERP_API.Common.Helpers;

/// <summary>
/// Helper para cálculo de rangos de fechas según períodos
/// </summary>
public static class DatePeriodHelper
{
    /// <summary>
    /// Obtiene el rango de fechas según el período especificado
    /// </summary>
    public static (DateTime StartDate, DateTime EndDate) GetDateRange(DashboardPeriod period, DateTime? customStartDate = null, DateTime? customEndDate = null)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        return period switch
        {
            DashboardPeriod.Today => (today, now),
            DashboardPeriod.Yesterday => (today.AddDays(-1), today.AddSeconds(-1)),
            DashboardPeriod.Week => (today.AddDays(-7), now),
            DashboardPeriod.Month => (today.AddMonths(-1), now),
            DashboardPeriod.Quarter => (today.AddMonths(-3), now),
            DashboardPeriod.Year => (today.AddYears(-1), now),
            DashboardPeriod.Custom when customStartDate.HasValue && customEndDate.HasValue
                => (customStartDate.Value, customEndDate.Value),
            _ => throw new ArgumentException($"Período inválido o fechas personalizadas faltantes: {period}")
        };
    }

    /// <summary>
    /// Obtiene el rango de fechas del período anterior para comparación
    /// </summary>
    public static (DateTime StartDate, DateTime EndDate) GetPreviousPeriodRange(DashboardPeriod period, DateTime startDate, DateTime endDate)
    {
        var periodLength = endDate - startDate;

        return period switch
        {
            DashboardPeriod.Today => (startDate.AddDays(-1), startDate.AddSeconds(-1)),
            DashboardPeriod.Yesterday => (startDate.AddDays(-1), startDate.AddSeconds(-1)),
            DashboardPeriod.Week => (startDate.AddDays(-7), endDate.AddDays(-7)),
            DashboardPeriod.Month => (startDate.AddMonths(-1), endDate.AddMonths(-1)),
            DashboardPeriod.Quarter => (startDate.AddMonths(-3), endDate.AddMonths(-3)),
            DashboardPeriod.Year => (startDate.AddYears(-1), endDate.AddYears(-1)),
            DashboardPeriod.Custom => (startDate.Add(-periodLength), startDate.AddSeconds(-1)),
            _ => throw new ArgumentException($"Período inválido: {period}")
        };
    }

    /// <summary>
    /// Calcula el porcentaje de crecimiento entre dos valores
    /// </summary>
    public static decimal CalculateGrowthPercentage(decimal current, decimal previous)
    {
        if (previous == 0)
            return current > 0 ? 100m : 0m;

        return Math.Round(((current - previous) / previous) * 100, 2);
    }

    /// <summary>
    /// Obtiene el agrupamiento apropiado según el rango de fechas
    /// </summary>
    public static string GetGroupingType(DateTime startDate, DateTime endDate)
    {
        var daysDifference = (endDate - startDate).Days;

        return daysDifference switch
        {
            <= 1 => "Hour",
            <= 31 => "Day",
            <= 90 => "Week",
            _ => "Month"
        };
    }

    /// <summary>
    /// Obtiene una descripción amigable del período
    /// </summary>
    public static string GetPeriodDescription(DashboardPeriod period, DateTime? startDate = null, DateTime? endDate = null)
    {
        return period switch
        {
            DashboardPeriod.Today => "Hoy",
            DashboardPeriod.Yesterday => "Ayer",
            DashboardPeriod.Week => "Últimos 7 días",
            DashboardPeriod.Month => "Último mes",
            DashboardPeriod.Quarter => "Último trimestre",
            DashboardPeriod.Year => "Último año",
            DashboardPeriod.Custom when startDate.HasValue && endDate.HasValue
                => $"{startDate.Value:dd/MM/yyyy} - {endDate.Value:dd/MM/yyyy}",
            _ => "Período personalizado"
        };
    }

    /// <summary>
    /// Valida que el rango de fechas sea válido
    /// </summary>
    public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
    {
        return startDate <= endDate && endDate <= DateTime.UtcNow;
    }

    /// <summary>
    /// Obtiene el inicio del día
    /// </summary>
    public static DateTime StartOfDay(DateTime date)
    {
        return date.Date;
    }

    /// <summary>
    /// Obtiene el fin del día
    /// </summary>
    public static DateTime EndOfDay(DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }
}