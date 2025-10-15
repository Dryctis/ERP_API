using ERP_API.DTOs;
using FluentValidation;

namespace ERP_API.Validators
{
    public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
    {
        public ProductCreateDtoValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("El SKU es obligatorio")
                .MaximumLength(50).WithMessage("El SKU no puede tener más de 50 caracteres");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio")
                .MaximumLength(200).WithMessage("El nombre no puede tener más de 200 caracteres");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("El precio debe ser mayor a 0");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("El stock no puede ser negativo");
        }
    }
}
