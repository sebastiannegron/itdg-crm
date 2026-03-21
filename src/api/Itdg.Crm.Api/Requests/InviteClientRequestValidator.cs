namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class InviteClientRequestValidator : AbstractValidator<InviteClientRequest>
{
    public InviteClientRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}
