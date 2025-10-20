using ERP_API.Common.Entities;
using System.ComponentModel.DataAnnotations;

namespace ERP_API.Entities;



public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Paid = 3,
    Cancelled = 4
}

public enum PaymentMethod
{
    Cash = 1,
    CreditCard = 2,
    BankTransfer = 3,
    Check = 4,
    Other = 5
}


public class Invoice : ISoftDeletable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }

    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal Balance => Total - PaidAmount;

    [MaxLength(500)]
    public string? PaymentTerms { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

 
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

 
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();

    public bool IsOverdue() => DueDate < DateTime.UtcNow && Status != InvoiceStatus.Paid && Status != InvoiceStatus.Cancelled;

    public bool IsPaid() => Status == InvoiceStatus.Paid;

    public void UpdateStatus()
    {
        if (Status == InvoiceStatus.Draft || Status == InvoiceStatus.Cancelled)
            return;

        if (PaidAmount >= Total)
            Status = InvoiceStatus.Paid;
        else if (PaidAmount > 0)
            Status = InvoiceStatus.Sent;
    }
}



public class InvoiceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public int SortOrder { get; set; }
}



public class InvoicePayment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;

    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    public PaymentMethod PaymentMethod { get; set; }

    [MaxLength(100)]
    public string? Reference { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}