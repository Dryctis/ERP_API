namespace ERP_API.DTOs;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    Guid? OrderId,
    string? OrderReference,
    DateTime IssueDate,
    DateTime DueDate,
    string Status, 
    decimal Subtotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal Total,
    decimal PaidAmount,
    decimal Balance,
    string? PaymentTerms,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<InvoiceItemDto> Items,
    List<InvoicePaymentDto> Payments,
    bool IsOverdue,
    bool IsPaid
);

public record InvoiceListDto(
    Guid Id,
    string InvoiceNumber,
    string CustomerName,
    DateTime IssueDate,
    DateTime DueDate,
    string Status,
    decimal Total,
    decimal PaidAmount,
    decimal Balance,
    bool IsOverdue
);

public record CreateInvoiceFromOrderDto(
    Guid OrderId,
    DateTime? IssueDate = null, 
    int? PaymentTermDays = 30,  
    string? PaymentTerms = "Net 30",
    decimal DiscountAmount = 0,
    string? Notes = null
);


public record InvoiceCreateDto(
    Guid CustomerId,
    DateTime? IssueDate = null,
    int? PaymentTermDays = 30,
    string? PaymentTerms = "Net 30",
    decimal DiscountAmount = 0,
    string? Notes = null,
    List<InvoiceItemCreateDto> Items = default!
);

public record InvoiceUpdateDto(
    DateTime IssueDate,
    DateTime DueDate,
    string? PaymentTerms,
    decimal DiscountAmount,
    string? Notes,
    List<InvoiceItemCreateDto> Items
);

public record InvoiceItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal Subtotal,
    decimal TaxAmount,
    decimal LineTotal
);


public record InvoiceItemCreateDto(
    Guid ProductId,
    string? Description = null, 
    int Quantity = 1,
    decimal? UnitPrice = null, 
    decimal DiscountAmount = 0
);


public record InvoicePaymentDto(
    Guid Id,
    Guid InvoiceId,
    DateTime PaymentDate,
    decimal Amount,
    string PaymentMethod, 
    string? Reference,
    string? Notes,
    DateTime CreatedAt
);


public record InvoicePaymentCreateDto(
    decimal Amount,
    DateTime? PaymentDate = null, 
    int PaymentMethod = 0, 
    string? Reference = null,
    string? Notes = null
);


public record InvoiceSummaryDto(
    int TotalInvoices,
    int DraftInvoices,
    int SentInvoices,
    int PaidInvoices,
    int OverdueInvoices,
    decimal TotalAmount,
    decimal TotalPaid,
    decimal TotalOutstanding,
    decimal OverdueAmount
);

public record OverdueInvoiceDto(
    Guid Id,
    string InvoiceNumber,
    string CustomerName,
    DateTime IssueDate,
    DateTime DueDate,
    int DaysOverdue,
    decimal Total,
    decimal Balance
);