using ERP_API.DTOs;
using FluentValidation;

namespace ERP_API.Validators;

public class CustomerCreateValidator : AbstractValidator<CustomerCreateDto>
{
    public CustomerCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(160);
        RuleFor(x => x.Phone).MaximumLength(40).When(x => x.Phone != null);
    }
}

public class CustomerUpdateValidator : AbstractValidator<CustomerUpdateDto>
{
    public CustomerUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(160);
        RuleFor(x => x.Phone).MaximumLength(40).When(x => x.Phone != null);
    }
}
