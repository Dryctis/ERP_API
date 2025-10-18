using System.ComponentModel.DataAnnotations;
using ERP_API.Common.Entities;

namespace ERP_API.Entities;

public class Product : ISoftDeletable
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    
    public bool IsDeleted { get; set; }

    
    public DateTime? DeletedAt { get; set; }

    
    public Guid? DeletedBy { get; set; }
}