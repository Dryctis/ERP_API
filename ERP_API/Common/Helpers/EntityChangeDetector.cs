using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ERP_API.Common.Audit;

/// <summary>
/// Detecta cambios en entidades y captura valores anteriores/nuevos
/// </summary>
public static class EntityChangeDetector
{
    /// <summary>
    /// Propiedades que NO deben ser auditadas (técnicas o sensibles)
    /// </summary>
    private static readonly HashSet<string> ExcludedProperties = new()
    {
        "PasswordHash",
        "RowVersion",
        "UpdatedAt",
        "CreatedAt",
        "DeletedAt",
        "DeletedBy"
    };

    /// <summary>
    /// Captura los campos que fueron modificados
    /// </summary>
    public static (string changedFields, Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues)
        GetChangedFields(EntityEntry entry)
    {
        var oldValuesDict = new Dictionary<string, object?>();
        var newValuesDict = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;

            // Excluir propiedades técnicas o sensibles
            if (ShouldExcludeProperty(propertyName))
                continue;

            // Solo incluir si el valor cambió
            if (entry.State == EntityState.Modified && !property.IsModified)
                continue;

            // Capturar valores
            var oldValue = entry.State == EntityState.Added ? null : property.OriginalValue;
            var newValue = entry.State == EntityState.Deleted ? null : property.CurrentValue;

            // Solo agregar si realmente cambió
            if (entry.State == EntityState.Modified && Equals(oldValue, newValue))
                continue;

            oldValuesDict[propertyName] = oldValue;
            newValuesDict[propertyName] = newValue;
        }

        // Si no hay cambios, retornar vacío
        if (oldValuesDict.Count == 0 && newValuesDict.Count == 0)
            return (string.Empty, new Dictionary<string, object?>(), new Dictionary<string, object?>());

        var changedFields = string.Join(", ", oldValuesDict.Keys);

        return (changedFields, oldValuesDict, newValuesDict);
    }

    /// <summary>
    /// Verifica si una propiedad debe ser excluida de la auditoría
    /// </summary>
    public static bool ShouldExcludeProperty(string propertyName)
    {
        return ExcludedProperties.Contains(propertyName);
    }

    /// <summary>
    /// Agrega una propiedad a la lista de exclusión
    /// </summary>
    public static void AddExcludedProperty(string propertyName)
    {
        ExcludedProperties.Add(propertyName);
    }
}