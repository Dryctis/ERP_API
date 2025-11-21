using ERP_API.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ERP_API.Common.Audit;

/// <summary>
/// Resuelve nombres legibles para entidades auditadas
/// </summary>
public static class EntityNameResolver
{
    /// <summary>
    /// Obtiene un nombre legible de la entidad para mostrar en logs
    /// </summary>
    public static string GetEntityName(EntityEntry entry)
    {
        var entityType = entry.Entity.GetType().Name;

        return entityType switch
        {
            nameof(Product) => GetProductName(entry),
            nameof(Order) => GetOrderName(entry),
            nameof(Invoice) => GetInvoiceName(entry),
            nameof(PurchaseOrder) => GetPurchaseOrderName(entry),
            nameof(User) => GetUserName(entry),
            _ => entityType
        };
    }

    /// <summary>
    /// Obtiene el ID de la entidad
    /// </summary>
    public static Guid? GetEntityId(EntityEntry entry)
    {
        try
        {
            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            return idProperty?.CurrentValue as Guid?;
        }
        catch
        {
            return null;
        }
    }

    private static string GetProductName(EntityEntry entry)
    {
        var name = GetPropertyValue<string>(entry, "Name");
        var sku = GetPropertyValue<string>(entry, "Sku");

        return !string.IsNullOrEmpty(name) ? name :
               !string.IsNullOrEmpty(sku) ? $"SKU: {sku}" :
               "Producto";
    }

    private static string GetOrderName(EntityEntry entry)
    {
        var id = GetPropertyValue<Guid>(entry, "Id");
        return $"Orden #{id.ToString().Substring(0, 8)}";
    }

    private static string GetInvoiceName(EntityEntry entry)
    {
        var invoiceNumber = GetPropertyValue<string>(entry, "InvoiceNumber");
        return !string.IsNullOrEmpty(invoiceNumber) ? $"Factura {invoiceNumber}" : "Factura";
    }

    private static string GetPurchaseOrderName(EntityEntry entry)
    {
        var orderNumber = GetPropertyValue<string>(entry, "OrderNumber");
        return !string.IsNullOrEmpty(orderNumber) ? $"OC {orderNumber}" : "Orden de Compra";
    }

    private static string GetUserName(EntityEntry entry)
    {
        var email = GetPropertyValue<string>(entry, "Email");
        var fullName = GetPropertyValue<string>(entry, "FullName");

        return !string.IsNullOrEmpty(fullName) ? fullName :
               !string.IsNullOrEmpty(email) ? email :
               "Usuario";
    }

    /// <summary>
    /// Obtiene el valor de una propiedad de forma segura
    /// </summary>
    private static T? GetPropertyValue<T>(EntityEntry entry, string propertyName)
    {
        try
        {
            var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);
            return property != null ? (T?)property.CurrentValue : default;
        }
        catch
        {
            return default;
        }
    }
}