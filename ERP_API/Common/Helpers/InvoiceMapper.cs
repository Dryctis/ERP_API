using ERP_API.DTOs;
using ERP_API.Entities;

namespace ERP_API.Common.Helpers;

public static class InvoiceMapper
{

    public static InvoiceDto ToDto(Invoice invoice)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        return new InvoiceDto(
            Id: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            CustomerId: invoice.CustomerId,
            CustomerName: invoice.Customer?.Name ?? "N/A",
            CustomerEmail: invoice.Customer?.Email ?? "N/A",
            OrderId: invoice.OrderId,
            OrderReference: invoice.Order?.Id.ToString(),
            IssueDate: invoice.IssueDate,
            DueDate: invoice.DueDate,
            Status: invoice.Status.ToString(),
            Subtotal: invoice.Subtotal,
            TaxAmount: invoice.TaxAmount,
            DiscountAmount: invoice.DiscountAmount,
            Total: invoice.Total,
            PaidAmount: invoice.PaidAmount,
            Balance: invoice.Balance,
            PaymentTerms: invoice.PaymentTerms,
            Notes: invoice.Notes,
            CreatedAt: invoice.CreatedAt,
            UpdatedAt: invoice.UpdatedAt,
            Items: invoice.Items?.Select(ToItemDto).OrderBy(i => i.Id).ToList() ?? new List<InvoiceItemDto>(),
            Payments: invoice.Payments?.Select(ToPaymentDto).OrderByDescending(p => p.PaymentDate).ToList() ?? new List<InvoicePaymentDto>(),
            IsOverdue: invoice.IsOverdue(),
            IsPaid: invoice.IsPaid()
        );
    }

    private static InvoiceItemDto ToItemDto(InvoiceItem item)
    {
        return new InvoiceItemDto(
            Id: item.Id,
            ProductId: item.ProductId,
            ProductName: item.Product?.Name ?? "N/A",
            ProductSku: item.Product?.Sku ?? "N/A",
            Description: item.Description,
            Quantity: item.Quantity,
            UnitPrice: item.UnitPrice,
            DiscountAmount: item.DiscountAmount,
            Subtotal: item.Subtotal,
            TaxAmount: item.TaxAmount,
            LineTotal: item.LineTotal
        );
    }

    
    private static InvoicePaymentDto ToPaymentDto(InvoicePayment payment)
    {
        return new InvoicePaymentDto(
            Id: payment.Id,
            InvoiceId: payment.InvoiceId,
            PaymentDate: payment.PaymentDate,
            Amount: payment.Amount,
            PaymentMethod: payment.PaymentMethod.ToString(),
            Reference: payment.Reference,
            Notes: payment.Notes,
            CreatedAt: payment.CreatedAt
        );
    }
}