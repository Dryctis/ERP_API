using System.Text.Json;

namespace ERP_API.Common.Audit;

/// <summary>
/// Formatea valores para auditoría y serialización
/// </summary>
public static class AuditValueFormatter
{
    private const int MaxJsonLength = 4000;

    /// <summary>
    /// Formatea un valor para serialización (manejo de tipos especiales)
    /// </summary>
    public static object? FormatValue(object? value)
    {
        if (value == null)
            return null;

        // Convertir Guid a string para mejor legibilidad
        if (value is Guid guid)
            return guid.ToString();

        // Convertir DateTime a formato ISO
        if (value is DateTime dateTime)
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");

        // Para enums, obtener el nombre
        if (value.GetType().IsEnum)
            return value.ToString();

        // Para decimales, mantener precisión
        if (value is decimal)
            return value;

        return value;
    }

    /// <summary>
    /// Serializa un diccionario a JSON con límite de tamaño
    /// </summary>
    public static string SerializeToJson(Dictionary<string, object?> data)
    {
        if (data == null || data.Count == 0)
            return string.Empty;

        try
        {
            // Formatear valores antes de serializar
            var formattedData = data.ToDictionary(
                kvp => kvp.Key,
                kvp => FormatValue(kvp.Value)
            );

            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(formattedData, options);

            // Limitar a MaxJsonLength caracteres (límite de la columna)
            if (json.Length > MaxJsonLength)
            {
                json = json.Substring(0, MaxJsonLength - 3) + "...";
            }

            return json;
        }
        catch (Exception ex)
        {
            // Log error pero no fallar
            return $"{{\"error\":\"Serialization failed: {ex.Message}\"}}";
        }
    }

    /// <summary>
    /// Crea una representación legible de los cambios
    /// </summary>
    public static string CreateChangeDescription(
        string changedFields,
        Dictionary<string, object?> oldValues,
        Dictionary<string, object?> newValues)
    {
        if (string.IsNullOrEmpty(changedFields))
            return string.Empty;

        var changes = new List<string>();

        foreach (var field in changedFields.Split(", "))
        {
            if (oldValues.TryGetValue(field, out var oldValue) &&
                newValues.TryGetValue(field, out var newValue))
            {
                changes.Add($"{field}: {FormatValue(oldValue)} → {FormatValue(newValue)}");
            }
        }

        return string.Join("; ", changes);
    }
}