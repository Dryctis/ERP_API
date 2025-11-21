using ERP_API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ERP_API.Common.Audit;

/// <summary>
/// Determina la acción de auditoría según el estado de la entidad
/// </summary>
public static class AuditActionResolver
{
    /// <summary>
    /// Obtiene la acción de auditoría según el estado de la entidad
    /// </summary>
    public static AuditAction GetAuditAction(EntityEntry entry)
    {
        return entry.State switch
        {
            EntityState.Added => AuditAction.Create,
            EntityState.Modified => IsSoftDelete(entry) ? AuditAction.Delete : AuditAction.Update,
            EntityState.Deleted => AuditAction.Delete,
            _ => AuditAction.Update
        };
    }

    /// <summary>
    /// Verifica si es un soft delete (IsDeleted cambió a true)
    /// </summary>
    public static bool IsSoftDelete(EntityEntry entry)
    {
        var isDeletedProperty = entry.Properties
            .FirstOrDefault(p => p.Metadata.Name == "IsDeleted");

        if (isDeletedProperty == null)
            return false;

        var currentValue = isDeletedProperty.CurrentValue as bool?;
        var originalValue = isDeletedProperty.OriginalValue as bool?;

        return currentValue == true && originalValue == false;
    }

    /// <summary>
    /// Verifica si es una restauración (IsDeleted cambió a false)
    /// </summary>
    public static bool IsRestore(EntityEntry entry)
    {
        var isDeletedProperty = entry.Properties
            .FirstOrDefault(p => p.Metadata.Name == "IsDeleted");

        if (isDeletedProperty == null)
            return false;

        var currentValue = isDeletedProperty.CurrentValue as bool?;
        var originalValue = isDeletedProperty.OriginalValue as bool?;

        return currentValue == false && originalValue == true;
    }
}