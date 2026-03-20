namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid user role.");
    }
}
