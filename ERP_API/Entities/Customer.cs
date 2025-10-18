using System.ComponentModel.DataAnnotations;
using ERP_API.Common.Entities;

namespace ERP_API.Entities;

public class Customer : ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(160)]
    public string Name { get; set; } = default!;

    [Required, MaxLength(160)]
    public string Email { get; set; } = default!;

    [MaxLength(40)]
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
    public bool IsDeleted { get; set; }

    
    public DateTime? DeletedAt { get; set; }

    
    public Guid? DeletedBy { get; set; }
}