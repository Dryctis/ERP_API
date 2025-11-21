using ERP_API.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ERP_API.Common.Audit;

/// <summary>
/// Factory para crear registros de auditoría
/// Responsabilidad: Orquestar las demás clases para crear un AuditLog completo
/// </summary>
public static class AuditLogFactory
{
    /// <summary>
    /// Crea un registro de auditoría a partir de un EntityEntry
    /// </summary>
    public static AuditLog CreateAuditLog(
        EntityEntry entry,
        string userName,
        Guid? userId,
        string? ipAddress = null,
        string? userAgent = null,
        string? endpoint = null)
    {
        // Detectar cambios
        var (changedFields, oldValues, newValues) = EntityChangeDetector.GetChangedFields(entry);

        // Crear el registro de auditoría
        return new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = AuditActionResolver.GetAuditAction(entry),
            EntityType = entry.Entity.GetType().Name,
            EntityId = EntityNameResolver.GetEntityId(entry),
            EntityName = EntityNameResolver.GetEntityName(entry),
            ChangedFields = changedFields,
            OldValues = AuditValueFormatter.SerializeToJson(oldValues),
            NewValues = AuditValueFormatter.SerializeToJson(newValues),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Endpoint = endpoint,
            Timestamp = DateTime.UtcNow,
            IsArchived = false
        };
    }

    /// <summary>
    /// Crea múltiples registros de auditoría para un conjunto de cambios
    /// </summary>
    public static List<AuditLog> CreateAuditLogs(
        IEnumerable<EntityEntry> entries,
        string userName,
        Guid? userId,
        string? ipAddress = null,
        string? userAgent = null,
        string? endpoint = null)
    {
        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (!AuditableEntityDetector.IsAuditable(entry))
                continue;

            var auditLog = CreateAuditLog(entry, userName, userId, ipAddress, userAgent, endpoint);
            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }

    /// <summary>
    /// Crea un registro de auditoría manual para acciones específicas
    /// </summary>
    public static AuditLog CreateManualAuditLog(
        AuditAction action,
        string entityType,
        Guid? entityId,
        string entityName,
        string userName,
        Guid? userId,
        Dictionary<string, object?>? oldValues = null,
        Dictionary<string, object?>? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? endpoint = null)
    {
        var changedFields = string.Empty;
        var oldValuesJson = string.Empty;
        var newValuesJson = string.Empty;

        if (oldValues != null && newValues != null)
        {
            changedFields = string.Join(", ", newValues.Keys);
            oldValuesJson = AuditValueFormatter.SerializeToJson(oldValues);
            newValuesJson = AuditValueFormatter.SerializeToJson(newValues);
        }

        return new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            ChangedFields = changedFields,
            OldValues = oldValuesJson,
            NewValues = newValuesJson,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Endpoint = endpoint,
            Timestamp = DateTime.UtcNow,
            IsArchived = false
        };
    }
}