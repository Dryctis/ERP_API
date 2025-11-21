using ERP_API.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ERP_API.Common.Audit;

/// <summary>
/// Determina si una entidad debe ser auditada
/// </summary>
public static class AuditableEntityDetector
{
    /// <summary>
    /// Entidades críticas que deben ser auditadas
    /// </summary>
    private static readonly HashSet<string> AuditableEntities = new()
    {
        nameof(Product),
        nameof(Order),
        nameof(Invoice),
        nameof(PurchaseOrder),
        nameof(User)
    };

    /// <summary>
    /// Verifica si una entidad debe ser auditada
    /// </summary>
    public static bool IsAuditable(EntityEntry entry)
    {
        if (entry?.Entity == null)
            return false;

        var entityTypeName = entry.Entity.GetType().Name;
        return AuditableEntities.Contains(entityTypeName);
    }

    /// <summary>
    /// Verifica si una entidad es de un tipo específico auditable
    /// </summary>
    public static bool IsAuditableType(string entityTypeName)
    {
        return AuditableEntities.Contains(entityTypeName);
    }

    /// <summary>
    /// Obtiene la lista de entidades auditables
    /// </summary>
    public static IReadOnlySet<string> GetAuditableEntities()
    {
        return AuditableEntities;
    }
}