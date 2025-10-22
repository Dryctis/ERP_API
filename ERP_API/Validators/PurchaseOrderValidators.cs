using ERP_API.DTOs;
using FluentValidation;

namespace ERP_API.Validators;

public class PurchaseOrderCreateValidator : AbstractValidator<PurchaseOrderCreateDto>
{
    public PurchaseOrderCreateValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("El proveedor es obligatorio");

        RuleFor(x => x.OrderDate)
            .Must(date => !date.HasValue || date.Value <= DateTime.UtcNow.AddDays(1))
            .WithMessage("La fecha de orden no puede ser futura");

        RuleFor(x => x.ExpectedDeliveryDate)
            .Must((dto, date) => !date.HasValue || date.Value >= (dto.OrderDate ?? DateTime.UtcNow))
            .WithMessage("La fecha esperada de entrega debe ser posterior a la fecha de orden");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo");

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("El costo de envío no puede ser negativo");

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(500).WithMessage("Los términos de pago no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.SupplierReference)
            .MaximumLength(100).WithMessage("La referencia del proveedor no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.SupplierReference));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La orden debe tener al menos un item")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("La orden debe tener al menos un item");

        RuleForEach(x => x.Items)
            .SetValidator(new PurchaseOrderItemCreateValidator());
    }
}

public class PurchaseOrderUpdateValidator : AbstractValidator<PurchaseOrderUpdateDto>
{
    public PurchaseOrderUpdateValidator()
    {
        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("La fecha de orden es obligatoria")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("La fecha de orden no puede ser futura");

        RuleFor(x => x.ExpectedDeliveryDate)
            .NotEmpty().WithMessage("La fecha esperada de entrega es obligatoria")
            .GreaterThanOrEqualTo(x => x.OrderDate)
            .WithMessage("La fecha esperada debe ser posterior a la fecha de orden");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo");

        RuleFor(x => x.ShippingCost)
            .GreaterThanOrEqualTo(0).WithMessage("El costo de envío no puede ser negativo");

        RuleFor(x => x.PaymentTerms)
            .MaximumLength(500).WithMessage("Los términos de pago no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.PaymentTerms));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.SupplierReference)
            .MaximumLength(100).WithMessage("La referencia del proveedor no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.SupplierReference));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La orden debe tener al menos un item");

        RuleForEach(x => x.Items)
            .SetValidator(new PurchaseOrderItemCreateValidator());
    }
}

public class PurchaseOrderItemCreateValidator : AbstractValidator<PurchaseOrderItemCreateDto>
{
    public PurchaseOrderItemCreateValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("El producto es obligatorio");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.SupplierSku)
            .MaximumLength(100).WithMessage("El SKU del proveedor no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.SupplierSku));

        RuleFor(x => x.OrderedQuantity)
            .GreaterThan(0).WithMessage("La cantidad ordenada debe ser mayor a 0")
            .LessThanOrEqualTo(100000).WithMessage("La cantidad ordenada es excesiva");

        RuleFor(x => x.UnitCost)
            .GreaterThan(0).WithMessage("El costo unitario debe ser mayor a 0")
            .LessThan(1000000).WithMessage("El costo unitario es excesivo")
            .When(x => x.UnitCost.HasValue);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("El descuento no puede ser negativo");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class ReceiveItemsValidator : AbstractValidator<ReceiveItemsDto>
{
    public ReceiveItemsValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Debe especificar al menos un item para recibir");

        RuleForEach(x => x.Items)
            .SetValidator(new ReceiveItemValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class ReceiveItemValidator : AbstractValidator<ReceiveItemDto>
{
    public ReceiveItemValidator()
    {
        RuleFor(x => x.PurchaseOrderItemId)
            .NotEmpty().WithMessage("El ID del item es obligatorio");

        RuleFor(x => x.ReceivedQuantity)
            .GreaterThan(0).WithMessage("La cantidad recibida debe ser mayor a 0")
            .LessThanOrEqualTo(100000).WithMessage("La cantidad recibida es excesiva");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public class ConfirmPurchaseOrderValidator : AbstractValidator<ConfirmPurchaseOrderDto>
{
    public ConfirmPurchaseOrderValidator()
    {
        RuleFor(x => x.SupplierReference)
            .MaximumLength(100).WithMessage("La referencia del proveedor no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.SupplierReference));

        RuleFor(x => x.ExpectedDeliveryDate)
            .Must(date => !date.HasValue || date.Value >= DateTime.UtcNow.Date)
            .WithMessage("La fecha esperada de entrega no puede ser pasada");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}