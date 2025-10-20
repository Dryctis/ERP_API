using ERP_API.DTOs;
using FluentValidation;

namespace ERP_API.Validators;

public class SupplierCreateValidator : AbstractValidator<SupplierCreateDto>
{
    public SupplierCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del proveedor es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres");

        RuleFor(x => x.ContactName)
            .MaximumLength(160).WithMessage("El nombre de contacto no puede exceder 160 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ContactName));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("Debe ser un email válido")
            .MaximumLength(160).WithMessage("El email no puede exceder 160 caracteres");

        RuleFor(x => x.Phone)
            .MaximumLength(40).WithMessage("El teléfono no puede exceder 40 caracteres")
            .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("El teléfono contiene caracteres inválidos")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("La dirección no puede exceder 300 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("La ciudad no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("El país no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.TaxId)
            .MaximumLength(50).WithMessage("El NIT/RUC no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.TaxId));

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("El sitio web no puede exceder 200 caracteres")
            .Must(BeAValidUrl).WithMessage("Debe ser una URL válida")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}

public class SupplierUpdateValidator : AbstractValidator<SupplierUpdateDto>
{
    public SupplierUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del proveedor es obligatorio")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres")
            .MinimumLength(2).WithMessage("El nombre debe tener al menos 2 caracteres");

        RuleFor(x => x.ContactName)
            .MaximumLength(160).WithMessage("El nombre de contacto no puede exceder 160 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ContactName));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio")
            .EmailAddress().WithMessage("Debe ser un email válido")
            .MaximumLength(160).WithMessage("El email no puede exceder 160 caracteres");

        RuleFor(x => x.Phone)
            .MaximumLength(40).WithMessage("El teléfono no puede exceder 40 caracteres")
            .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("El teléfono contiene caracteres inválidos")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.Address)
            .MaximumLength(300).WithMessage("La dirección no puede exceder 300 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Address));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("La ciudad no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("El país no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.TaxId)
            .MaximumLength(50).WithMessage("El NIT/RUC no puede exceder 50 caracteres")
            .When(x => !string.IsNullOrEmpty(x.TaxId));

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("El sitio web no puede exceder 200 caracteres")
            .Must(BeAValidUrl).WithMessage("Debe ser una URL válida")
            .When(x => !string.IsNullOrEmpty(x.Website));

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Las notas no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}


public class ProductSupplierCreateValidator : AbstractValidator<ProductSupplierCreateDto>
{
    public ProductSupplierCreateValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("El ID del producto es obligatorio");

        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("El ID del proveedor es obligatorio");

        RuleFor(x => x.SupplierPrice)
            .GreaterThan(0).WithMessage("El precio del proveedor debe ser mayor a cero")
            .LessThan(999999.99m).WithMessage("El precio es demasiado alto");

        RuleFor(x => x.SupplierSku)
            .MaximumLength(100).WithMessage("El SKU del proveedor no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.SupplierSku));

        RuleFor(x => x.LeadTimeDays)
            .GreaterThanOrEqualTo(0).WithMessage("El tiempo de entrega no puede ser negativo")
            .LessThanOrEqualTo(365).WithMessage("El tiempo de entrega no puede exceder 365 días")
            .When(x => x.LeadTimeDays.HasValue);

        RuleFor(x => x.MinimumOrderQuantity)
            .GreaterThan(0).WithMessage("La cantidad mínima debe ser mayor a cero")
            .When(x => x.MinimumOrderQuantity.HasValue);
    }
}


public class ProductSupplierUpdateValidator : AbstractValidator<ProductSupplierUpdateDto>
{
    public ProductSupplierUpdateValidator()
    {
        RuleFor(x => x.SupplierPrice)
            .GreaterThan(0).WithMessage("El precio del proveedor debe ser mayor a cero")
            .LessThan(999999.99m).WithMessage("El precio es demasiado alto");

        RuleFor(x => x.SupplierSku)
            .MaximumLength(100).WithMessage("El SKU del proveedor no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.SupplierSku));

        RuleFor(x => x.LeadTimeDays)
            .GreaterThanOrEqualTo(0).WithMessage("El tiempo de entrega no puede ser negativo")
            .LessThanOrEqualTo(365).WithMessage("El tiempo de entrega no puede exceder 365 días")
            .When(x => x.LeadTimeDays.HasValue);

        RuleFor(x => x.MinimumOrderQuantity)
            .GreaterThan(0).WithMessage("La cantidad mínima debe ser mayor a cero")
            .When(x => x.MinimumOrderQuantity.HasValue);
    }
}