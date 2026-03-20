namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.ContactEmail)
            .MaximumLength(200).WithMessage("Contact email must not exceed 200 characters.")
            .EmailAddress().WithMessage("Contact email must be a valid email address.")
            .When(x => !string.IsNullOrEmpty(x.ContactEmail));

        RuleFor(x => x.Phone)
            .MaximumLength(50).WithMessage("Phone must not exceed 50 characters.");

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Address must not exceed 500 characters.");

        RuleFor(x => x.IndustryTag)
            .MaximumLength(100).WithMessage("Industry tag must not exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.");

        RuleFor(x => x.CustomFields)
            .MaximumLength(4000).WithMessage("Custom fields must not exceed 4000 characters.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid client status.");
    }
}
