using ERP_API.DTOs;
using FluentValidation;

namespace ERP_API.Validators;

public class CreateInvoiceFromOrderValidator : AbstractValidator<CreateInvoiceFromOrderDto>
{
    public CreateInvoiceFromOrderValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("El ID del pedido es obligatorio");

        RuleFor(x => x.IssueDate)
            .Must(date => !date.HasValue || date.Value <= DateTime.UtcNow.AddDays(1))
            .WithMessage("La fecha de emisión no puede ser futura");

        RuleFor(x => x.PaymentTermDays)
            .GreaterThan(0).WithMessage("Los días de plazo deben ser mayores a 0")
            .LessThanOrEqualTo(365).WithMessage("Los días de plazo no pueden exceder 365")
            .When(x => x.PaymentTermDays.HasValue);

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(100).WithMessage("Los términos de pago no pueden exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo")
            .LessThan(1000000).WithMessage("El descuento es excesivo");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class InvoiceCreateValidator : AbstractValidator<InvoiceCreateDto>
{
    public InvoiceCreateValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("El ID del cliente es obligatorio");

        RuleFor(x => x.IssueDate)
            .Must(date => !date.HasValue || date.Value <= DateTime.UtcNow.AddDays(1))
            .WithMessage("La fecha de emisión no puede ser futura");

        RuleFor(x => x.PaymentTermDays)
            .GreaterThan(0).WithMessage("Los días de plazo deben ser mayores a 0")
            .LessThanOrEqualTo(365).WithMessage("Los días de plazo no pueden exceder 365")
            .When(x => x.PaymentTermDays.HasValue);

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(100).WithMessage("Los términos de pago no pueden exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La factura debe tener al menos un item")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("La factura debe tener al menos un item");

        RuleForEach(x => x.Items)
            .SetValidator(new InvoiceItemCreateValidator());
    }
}

public class InvoiceUpdateValidator : AbstractValidator<InvoiceUpdateDto>
{
    public InvoiceUpdateValidator()
    {
        RuleFor(x => x.IssueDate)
            .NotEmpty().WithMessage("La fecha de emisión es obligatoria")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("La fecha de emisión no puede ser futura");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("La fecha de vencimiento es obligatoria")
            .GreaterThanOrEqualTo(x => x.IssueDate)
            .WithMessage("La fecha de vencimiento debe ser posterior o igual a la fecha de emisión");

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(100).WithMessage("Los términos de pago no pueden exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La factura debe tener al menos un item");

        RuleForEach(x => x.Items)
            .SetValidator(new InvoiceItemCreateValidator());
    }
}

public class InvoiceItemCreateValidator : AbstractValidator<InvoiceItemCreateDto>
{
    public InvoiceItemCreateValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("El ID del producto es obligatorio");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0")
            .LessThanOrEqualTo(10000).WithMessage("La cantidad es excesiva");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a 0")
            .LessThan(1000000).WithMessage("El precio unitario es excesivo")
            .When(x => x.UnitPrice.HasValue);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo");
    }
}

public class InvoicePaymentCreateValidator : AbstractValidator<InvoicePaymentCreateDto>
{
    public InvoicePaymentCreateValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto del pago debe ser mayor a 0")
            .LessThan(10000000).WithMessage("El monto del pago es excesivo");

        RuleFor(x => x.PaymentDate)
            .Must(date => !date.HasValue || date.Value <= DateTime.UtcNow.AddDays(1))
            .WithMessage("La fecha de pago no puede ser futura");

        RuleFor(x => x.PaymentMethod)
            .InclusiveBetween(0, 99).WithMessage("Método de pago inválido");

        RuleFor(x => x.Reference)
            .MaximumLength(200).WithMessage("La referencia no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Reference));

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}