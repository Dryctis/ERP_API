using System.ComponentModel.DataAnnotations;
using ERP_API.Common.Entities;

namespace ERP_API.Entities;


public class Supplier : ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();

   
    [Required, MaxLength(200)]
    public string Name { get; set; } = default!;

   
    [MaxLength(160)]
    public string? ContactName { get; set; }

   
    [Required, MaxLength(160)]
    public string Email { get; set; } = default!;

 
    [MaxLength(40)]
    public string? Phone { get; set; }

   
    [MaxLength(300)]
    public string? Address { get; set; }

   
    [MaxLength(100)]
    public string? City { get; set; }

    
    [MaxLength(100)]
    public string? Country { get; set; }

  
    [MaxLength(50)]
    public string? TaxId { get; set; }

    
    [MaxLength(200)]
    public string? Website { get; set; }

  
    [MaxLength(1000)]
    public string? Notes { get; set; }

    
    public bool IsActive { get; set; } = true;


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
    public DateTime? UpdatedAt { get; set; }

   
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

  
    public ICollection<ProductSupplier> ProductSuppliers { get; set; } = new List<ProductSupplier>();
}


public class ProductSupplier
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = default!;

   
    public decimal SupplierPrice { get; set; }

    
    [MaxLength(100)]
    public string? SupplierSku { get; set; }

    
    public bool IsPreferred { get; set; }

    
    public int? LeadTimeDays { get; set; }

  
    public int? MinimumOrderQuantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}