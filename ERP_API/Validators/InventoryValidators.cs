using ERP_API.DTOs;
using FluentValidation;

namespace ERP_API.Validators
{
    public class InventoryMovementCreateValidator : AbstractValidator<InventoryMovementCreateDto>
    {
        public InventoryMovementCreateValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("El producto es obligatorio.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a 0.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Debe indicar una razón para el movimiento.");
        }
    }
}
