using AutoMapper;
using ERP_API.Common.Results;
using ERP_API.DTOs;
using ERP_API.Entities;
using ERP_API.Repositories.Interfaces;
using ERP_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly IUnidadDeTrabajo _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IUnidadDeTrabajo unitOfWork,
        IMapper mapper,
        ILogger<InvoiceService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<object> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        string? status = null,
        Guid? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sort = null)
    {
        _logger.LogDebug(
            "Consultando facturas paginadas. Página: {Page}, Tamaño: {PageSize}",
            page, pageSize
        );

        var (items, total) = await _unitOfWork.Invoices.GetPagedAsync(
            page, pageSize, searchTerm, status, customerId, fromDate, toDate, sort);

        var result = items.Select(i => new InvoiceListDto(
            i.Id,
            i.InvoiceNumber,
            i.Customer.Name,
            i.IssueDate,
            i.DueDate,
            i.Status.ToString(),
            i.Total,
            i.PaidAmount,
            i.Balance,
            i.IsOverdue()
        )).ToList();

        _logger.LogInformation(
            "Facturas obtenidas. Total: {Total}, Página: {Page}, Resultados: {Count}",
            total, page, result.Count
        );

        return new { total, page, pageSize, items = result };
    }

    public async Task<Result<InvoiceDto>> GetAsync(Guid id)
    {
        _logger.LogDebug("Consultando factura. InvoiceId: {InvoiceId}", id);

        var invoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(id);

        if (invoice is null)
        {
            _logger.LogWarning("Factura no encontrada. InvoiceId: {InvoiceId}", id);
            return Result<InvoiceDto>.Failure("Invoice not found");
        }

        var dto = MapToInvoiceDto(invoice);

        _logger.LogDebug(
            "Factura encontrada. InvoiceNumber: {Number}, Customer: {Customer}",
            invoice.InvoiceNumber, invoice.Customer.Name
        );

        return Result<InvoiceDto>.Success(dto);
    }

    public async Task<Result<InvoiceDto>> CreateFromOrderAsync(CreateInvoiceFromOrderDto dto)
    {
        _logger.LogInformation(
            "Creando factura desde pedido. OrderId: {OrderId}",
            dto.OrderId
        );

     
        var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
        if (order is null)
        {
            _logger.LogWarning("Pedido no encontrado. OrderId: {OrderId}", dto.OrderId);
            return Result<InvoiceDto>.Failure("Order not found");
        }

       
        if (await _unitOfWork.Invoices.ExistsForOrderAsync(dto.OrderId))
        {
            _logger.LogWarning(
                "Ya existe una factura para este pedido. OrderId: {OrderId}",
                dto.OrderId
            );
            return Result<InvoiceDto>.Failure("Invoice already exists for this order");
        }

        
        var invoiceNumber = await _unitOfWork.Invoices.GenerateInvoiceNumberAsync();

      
        var issueDate = dto.IssueDate ?? DateTime.UtcNow;
        var paymentTermDays = dto.PaymentTermDays.HasValue ? dto.PaymentTermDays.Value : 30;
        var dueDate = issueDate.AddDays(paymentTermDays);

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = order.CustomerId,
            OrderId = order.Id,
            IssueDate = issueDate,
            DueDate = dueDate,
            Status = InvoiceStatus.Draft,
            PaymentTerms = dto.PaymentTerms ?? $"Net {paymentTermDays}",
            Notes = dto.Notes,
            Subtotal = order.Subtotal,
            TaxAmount = order.Tax,
            DiscountAmount = dto.DiscountAmount,
            Total = order.Total - dto.DiscountAmount
        };

        
        int sortOrder = 1;
        foreach (var orderItem in order.Items)
        {
            var invoiceItem = new InvoiceItem
            {
                ProductId = orderItem.ProductId,
                Description = orderItem.Product.Name,
                Quantity = orderItem.Quantity,
                UnitPrice = orderItem.UnitPrice,
                DiscountAmount = 0,
                Subtotal = orderItem.Quantity * orderItem.UnitPrice,
                TaxAmount = 0,
                LineTotal = orderItem.LineTotal,
                SortOrder = sortOrder++
            };

            invoice.Items.Add(invoiceItem);
        }

        await _unitOfWork.Invoices.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Factura creada desde pedido. InvoiceId: {InvoiceId}, InvoiceNumber: {Number}",
            invoice.Id, invoice.InvoiceNumber
        );

        var created = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(invoice.Id);

        if (created is null)
        {
            _logger.LogError("Error: Factura creada pero no recuperada. Id: {Id}", invoice.Id);
            return Result<InvoiceDto>.Failure("Error creating invoice");
        }

        return Result<InvoiceDto>.Success(MapToInvoiceDto(created));
    }

    public async Task<Result<InvoiceDto>> CreateAsync(InvoiceCreateDto dto)
    {
        _logger.LogInformation(
            "Creando factura directa. CustomerId: {CustomerId}",
            dto.CustomerId
        );

        
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
        if (customer is null)
        {
            _logger.LogWarning("Cliente no encontrado. CustomerId: {CustomerId}", dto.CustomerId);
            return Result<InvoiceDto>.Failure("Customer not found");
        }

       
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _unitOfWork.GetDbContext().Set<Product>()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (products.Count != productIds.Count)
        {
            _logger.LogWarning("Algunos productos no encontrados");
            return Result<InvoiceDto>.Failure("One or more products not found");
        }

        
        var invoiceNumber = await _unitOfWork.Invoices.GenerateInvoiceNumberAsync();

        
        var issueDate = dto.IssueDate ?? DateTime.UtcNow;
        var paymentTermDays = dto.PaymentTermDays.HasValue ? dto.PaymentTermDays.Value : 30;
        var dueDate = issueDate.AddDays(paymentTermDays);

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = dto.CustomerId,
            OrderId = null,
            IssueDate = issueDate,
            DueDate = dueDate,
            Status = InvoiceStatus.Draft,
            PaymentTerms = dto.PaymentTerms ?? $"Net {paymentTermDays}",
            Notes = dto.Notes,
            DiscountAmount = dto.DiscountAmount
        };

       
        decimal subtotal = 0;
        decimal taxTotal = 0;
        int sortOrder = 1;

        foreach (var itemDto in dto.Items)
        {
            var product = products[itemDto.ProductId];
            var unitPrice = itemDto.UnitPrice ?? product.Price;
            var lineSubtotal = (itemDto.Quantity * unitPrice) - itemDto.DiscountAmount;
            var lineTax = lineSubtotal * 0.12m; 
            var lineTotal = lineSubtotal + lineTax;

            subtotal += lineSubtotal;
            taxTotal += lineTax;

            var invoiceItem = new InvoiceItem
            {
                ProductId = itemDto.ProductId,
                Description = itemDto.Description ?? product.Name,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                DiscountAmount = itemDto.DiscountAmount,
                Subtotal = lineSubtotal,
                TaxAmount = lineTax,
                LineTotal = lineTotal,
                SortOrder = sortOrder++
            };

            invoice.Items.Add(invoiceItem);
        }

        invoice.Subtotal = subtotal;
        invoice.TaxAmount = taxTotal;
        invoice.Total = (subtotal + taxTotal) - invoice.DiscountAmount;

        await _unitOfWork.Invoices.AddAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Factura directa creada. InvoiceId: {InvoiceId}, InvoiceNumber: {Number}",
            invoice.Id, invoice.InvoiceNumber
        );

        var created = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(invoice.Id);

        if (created is null)
        {
            _logger.LogError("Error: Factura creada pero no recuperada. Id: {Id}", invoice.Id);
            return Result<InvoiceDto>.Failure("Error creating invoice");
        }

        return Result<InvoiceDto>.Success(MapToInvoiceDto(created));
    }

    public async Task<Result<InvoiceDto>> UpdateAsync(Guid id, InvoiceUpdateDto dto)
    {
        _logger.LogInformation("Actualizando factura. InvoiceId: {InvoiceId}", id);

        var invoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(id);

        if (invoice is null)
        {
            _logger.LogWarning("Factura no encontrada. InvoiceId: {InvoiceId}", id);
            return Result<InvoiceDto>.Failure("Invoice not found");
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            _logger.LogWarning(
                "No se puede actualizar factura en estado {Status}. InvoiceId: {InvoiceId}",
                invoice.Status, id
            );
            return Result<InvoiceDto>.Failure($"Cannot update invoice in {invoice.Status} status");
        }

        invoice.IssueDate = dto.IssueDate;
        invoice.DueDate = dto.DueDate;
        invoice.PaymentTerms = dto.PaymentTerms;
        invoice.DiscountAmount = dto.DiscountAmount;
        invoice.Notes = dto.Notes;

        _unitOfWork.GetDbContext().Set<InvoiceItem>().RemoveRange(invoice.Items);

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _unitOfWork.GetDbContext().Set<Product>()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        if (products.Count != productIds.Count)
        {
            return Result<InvoiceDto>.Failure("One or more products not found");
        }

        decimal subtotal = 0;
        decimal taxTotal = 0;
        int sortOrder = 1;

        invoice.Items.Clear();

        foreach (var itemDto in dto.Items)
        {
            var product = products[itemDto.ProductId];
            var unitPrice = itemDto.UnitPrice ?? product.Price;
            var lineSubtotal = (itemDto.Quantity * unitPrice) - itemDto.DiscountAmount;
            var lineTax = lineSubtotal * 0.12m;
            var lineTotal = lineSubtotal + lineTax;

            subtotal += lineSubtotal;
            taxTotal += lineTax;

            var invoiceItem = new InvoiceItem
            {
                ProductId = itemDto.ProductId,
                Description = itemDto.Description ?? product.Name,
                Quantity = itemDto.Quantity,
                UnitPrice = unitPrice,
                DiscountAmount = itemDto.DiscountAmount,
                Subtotal = lineSubtotal,
                TaxAmount = lineTax,
                LineTotal = lineTotal,
                SortOrder = sortOrder++
            };

            invoice.Items.Add(invoiceItem);
        }

        invoice.Subtotal = subtotal;
        invoice.TaxAmount = taxTotal;
        invoice.Total = (subtotal + taxTotal) - invoice.DiscountAmount;

        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Factura actualizada. InvoiceId: {InvoiceId}", id);

        var updated = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(id);

        if (updated is null)
        {
            return Result<InvoiceDto>.Failure("Error updating invoice");
        }

        return Result<InvoiceDto>.Success(MapToInvoiceDto(updated));
    }


    public async Task<Result> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Eliminando factura. InvoiceId: {InvoiceId}", id);

        var invoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(id);

        if (invoice is null)
        {
            _logger.LogWarning("Factura no encontrada. InvoiceId: {InvoiceId}", id);
            return Result.Failure("Invoice not found");
        }

        if (invoice.Payments.Any())
        {
            _logger.LogWarning(
                "No se puede eliminar factura con pagos. InvoiceId: {InvoiceId}, Payments: {Count}",
                id, invoice.Payments.Count
            );
            return Result.Failure("Cannot delete invoice with registered payments");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            _logger.LogWarning("No se puede eliminar factura pagada. InvoiceId: {InvoiceId}", id);
            return Result.Failure("Cannot delete paid invoice");
        }

        await _unitOfWork.Invoices.DeleteAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning(
            "Factura eliminada (soft delete). InvoiceId: {InvoiceId}, Number: {Number}",
            id, invoice.InvoiceNumber
        );

        return Result.Success();
    }

    public async Task<Result<InvoiceDto>> SendInvoiceAsync(Guid id)
    {
        _logger.LogInformation("Enviando factura. InvoiceId: {InvoiceId}", id);

        var invoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(id);

        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure("Invoice not found");
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            return Result<InvoiceDto>.Failure($"Cannot send invoice in {invoice.Status} status");
        }

        invoice.Status = InvoiceStatus.Sent;
        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Factura enviada. InvoiceId: {InvoiceId}, Number: {Number}",
            id, invoice.InvoiceNumber
        );

        return Result<InvoiceDto>.Success(MapToInvoiceDto(invoice));
    }

    public async Task<Result<InvoiceDto>> CancelInvoiceAsync(Guid id)
    {
        _logger.LogInformation("Cancelando factura. InvoiceId: {InvoiceId}", id);

        var invoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(id);

        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure("Invoice not found");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result<InvoiceDto>.Failure("Cannot cancel paid invoice");
        }

        if (invoice.Payments.Any())
        {
            return Result<InvoiceDto>.Failure("Cannot cancel invoice with registered payments");
        }

        invoice.Status = InvoiceStatus.Cancelled;
        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning(
            "Factura cancelada. InvoiceId: {InvoiceId}, Number: {Number}",
            id, invoice.InvoiceNumber
        );

        return Result<InvoiceDto>.Success(MapToInvoiceDto(invoice));
    }

    public async Task<Result<InvoiceDto>> AddPaymentAsync(Guid invoiceId, InvoicePaymentCreateDto dto)
    {
        _logger.LogInformation(
            "Registrando pago. InvoiceId: {InvoiceId}, Amount: {Amount}",
            invoiceId, dto.Amount
        );

        var invoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(invoiceId);

        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure("Invoice not found");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result<InvoiceDto>.Failure("Cannot add payment to cancelled invoice");
        }

        if (invoice.Status == InvoiceStatus.Draft)
        {
            return Result<InvoiceDto>.Failure("Cannot add payment to draft invoice. Send it first.");
        }

        if (dto.Amount > invoice.Balance)
        {
            _logger.LogWarning(
                "Monto de pago excede balance. InvoiceId: {InvoiceId}, Balance: {Balance}, Amount: {Amount}",
                invoiceId, invoice.Balance, dto.Amount
            );
            return Result<InvoiceDto>.Failure($"Payment amount ({dto.Amount}) exceeds balance ({invoice.Balance})");
        }

        var payment = new InvoicePayment
        {
            InvoiceId = invoiceId,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate ?? DateTime.UtcNow,
            PaymentMethod = (PaymentMethod)dto.PaymentMethod,
            Reference = dto.Reference,
            Notes = dto.Notes
        };

        await _unitOfWork.InvoicePayments.AddAsync(payment);

        invoice.PaidAmount += dto.Amount;
        invoice.UpdateStatus();

        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Pago registrado. InvoiceId: {InvoiceId}, PaymentId: {PaymentId}, NewStatus: {Status}",
            invoiceId, payment.Id, invoice.Status
        );

        var updated = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(invoiceId);

        if (updated is null)
        {
            return Result<InvoiceDto>.Failure("Error processing payment");
        }

        return Result<InvoiceDto>.Success(MapToInvoiceDto(updated));
    }

    public async Task<Result<List<InvoicePaymentDto>>> GetPaymentsAsync(Guid invoiceId)
    {
        var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);

        if (invoice is null)
        {
            return Result<List<InvoicePaymentDto>>.Failure("Invoice not found");
        }

        var payments = await _unitOfWork.InvoicePayments.GetByInvoiceIdAsync(invoiceId);

        var result = payments.Select(p => new InvoicePaymentDto(
            p.Id,
            p.InvoiceId,
            p.PaymentDate,
            p.Amount,
            p.PaymentMethod.ToString(),
            p.Reference,
            p.Notes,
            p.CreatedAt
        )).ToList();

        return Result<List<InvoicePaymentDto>>.Success(result);
    }

  
    public async Task<Result> DeletePaymentAsync(Guid invoiceId, Guid paymentId)
    {
        _logger.LogInformation(
            "Eliminando pago. InvoiceId: {InvoiceId}, PaymentId: {PaymentId}",
            invoiceId, paymentId
        );

        var invoice = await _unitOfWork.Invoices.GetByIdWithDetailsAsync(invoiceId);

        if (invoice is null)
        {
            return Result.Failure("Invoice not found");
        }

        var payment = await _unitOfWork.InvoicePayments.GetByIdAsync(paymentId);

        if (payment is null || payment.InvoiceId != invoiceId)
        {
            return Result.Failure("Payment not found");
        }

        invoice.PaidAmount -= payment.Amount;
        invoice.UpdateStatus();

        await _unitOfWork.InvoicePayments.DeleteAsync(payment);
        await _unitOfWork.Invoices.UpdateAsync(invoice);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogWarning(
            "Pago eliminado. InvoiceId: {InvoiceId}, PaymentId: {PaymentId}, NewStatus: {Status}",
            invoiceId, paymentId, invoice.Status
        );

        return Result.Success();
    }

    public async Task<Result<List<OverdueInvoiceDto>>> GetOverdueInvoicesAsync()
    {
        _logger.LogDebug("Consultando facturas vencidas");

        var invoices = await _unitOfWork.Invoices.GetOverdueInvoicesAsync();

        var result = invoices.Select(i => new OverdueInvoiceDto(
            i.Id,
            i.InvoiceNumber,
            i.Customer.Name,
            i.IssueDate,
            i.DueDate,
            (DateTime.UtcNow - i.DueDate).Days,
            i.Total,
            i.Balance
        )).ToList();

        _logger.LogInformation("Facturas vencidas encontradas: {Count}", result.Count);

        return Result<List<OverdueInvoiceDto>>.Success(result);
    }

    public async Task<Result<InvoiceSummaryDto>> GetSummaryAsync()
    {
        _logger.LogDebug("Generando resumen financiero");

        var statusCounts = await _unitOfWork.Invoices.GetInvoiceCountByStatusAsync();

        var allInvoices = await _unitOfWork.GetDbContext().Set<Invoice>()
            .AsNoTracking()
            .ToListAsync();

        var totalAmount = allInvoices
            .Where(i => i.Status != InvoiceStatus.Cancelled)
            .Sum(i => i.Total);

        var totalPaid = allInvoices
            .Where(i => i.Status != InvoiceStatus.Cancelled)
            .Sum(i => i.PaidAmount);

        var totalOutstanding = totalAmount - totalPaid;

        var overdueAmount = allInvoices
            .Where(i => i.IsOverdue())
            .Sum(i => i.Balance);

        var summary = new InvoiceSummaryDto(
            TotalInvoices: allInvoices.Count,
            DraftInvoices: statusCounts.GetValueOrDefault(InvoiceStatus.Draft, 0),
            SentInvoices: statusCounts.GetValueOrDefault(InvoiceStatus.Sent, 0),
            PaidInvoices: statusCounts.GetValueOrDefault(InvoiceStatus.Paid, 0),
            OverdueInvoices: allInvoices.Count(i => i.IsOverdue()),
            TotalAmount: totalAmount,
            TotalPaid: totalPaid,
            TotalOutstanding: totalOutstanding,
            OverdueAmount: overdueAmount
        );

        _logger.LogInformation(
            "Resumen generado. Total: {Total}, Paid: {Paid}, Outstanding: {Outstanding}",
            totalAmount, totalPaid, totalOutstanding
        );

        return Result<InvoiceSummaryDto>.Success(summary);
    }

    private InvoiceDto MapToInvoiceDto(Invoice invoice)
    {
        return new InvoiceDto(
            Id: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            CustomerId: invoice.CustomerId,
            CustomerName: invoice.Customer.Name,
            CustomerEmail: invoice.Customer.Email,
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
            Items: invoice.Items.Select(item => new InvoiceItemDto(
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
            )).OrderBy(i => i.Id).ToList(),
            Payments: invoice.Payments.Select(p => new InvoicePaymentDto(
                Id: p.Id,
                InvoiceId: p.InvoiceId,
                PaymentDate: p.PaymentDate,
                Amount: p.Amount,
                PaymentMethod: p.PaymentMethod.ToString(),
                Reference: p.Reference,
                Notes: p.Notes,
                CreatedAt: p.CreatedAt
            )).OrderByDescending(p => p.PaymentDate).ToList(),
            IsOverdue: invoice.IsOverdue(),
            IsPaid: invoice.IsPaid()
        );
    }
}