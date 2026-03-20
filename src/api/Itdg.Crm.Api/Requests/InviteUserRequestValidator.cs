namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class InviteUserRequestValidator : AbstractValidator<InviteUserRequest>
{
    public InviteUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MinimumLength(2).WithMessage("Display name must be at least 2 characters.")
            .MaximumLength(200).WithMessage("Display name must not exceed 200 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role.");
    }
}
