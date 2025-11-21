using System.ComponentModel.DataAnnotations;

namespace ERP_API.Entities;

/// <summary>
/// Registro de auditoría de cambios en el sistema
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ID del usuario que realizó la acción
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Nombre/Email del usuario que realizó la acción
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de acción realizada
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// Tipo de entidad afectada (ej: "Product", "Order")
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID de la entidad afectada
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Nombre o descripción legible de la entidad
    /// </summary>
    [MaxLength(500)]
    public string? EntityName { get; set; }

    /// <summary>
    /// Lista de campos que fueron modificados (separados por coma)
    /// Ejemplo: "Price,Stock,Name"
    /// </summary>
    [MaxLength(1000)]
    public string? ChangedFields { get; set; }

    /// <summary>
    /// Valores anteriores en formato JSON
    /// Solo campos que cambiaron
    /// </summary>
    [MaxLength(4000)]
    public string? OldValues { get; set; }

    /// <summary>
    /// Valores nuevos en formato JSON
    /// Solo campos que cambiaron
    /// </summary>
    [MaxLength(4000)]
    public string? NewValues { get; set; }

    /// <summary>
    /// Dirección IP del usuario
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User Agent del navegador
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Endpoint HTTP llamado
    /// </summary>
    [MaxLength(500)]
    public string? Endpoint { get; set; }

    /// <summary>
    /// Fecha y hora en que ocurrió la acción
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si este registro ya fue archivado (más de 1 año)
    /// </summary>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// Fecha en que fue archivado
    /// </summary>
    public DateTime? ArchivedAt { get; set; }
}

/// <summary>
/// Tipos de acciones auditables
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Creación de un nuevo registro
    /// </summary>
    Create = 1,

    /// <summary>
    /// Actualización de un registro existente
    /// </summary>
    Update = 2,

    /// <summary>
    /// Eliminación (soft delete) de un registro
    /// </summary>
    Delete = 3,

    /// <summary>
    /// Restauración de un registro eliminado
    /// </summary>
    Restore = 4,

    /// <summary>
    /// Inicio de sesión
    /// </summary>
    Login = 5,

    /// <summary>
    /// Cierre de sesión
    /// </summary>
    Logout = 6,

    /// <summary>
    /// Cambio de contraseña
    /// </summary>
    PasswordChange = 7,

    /// <summary>
    /// Cambio de roles de usuario
    /// </summary>
    RoleChange = 8,

    /// <summary>
    /// Activación de registro
    /// </summary>
    Activate = 9,

    /// <summary>
    /// Desactivación de registro
    /// </summary>
    Deactivate = 10
}