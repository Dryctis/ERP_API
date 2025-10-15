using System.ComponentModel.DataAnnotations;

namespace ERP_API.Entities;

public class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(160)]
    public string Name { get; set; } = default!;

    [Required, MaxLength(160)]
    public string Email { get; set; } = default!;

    [MaxLength(40)]
    public string? Phone { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
