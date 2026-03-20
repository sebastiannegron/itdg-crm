namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class AssignClientRequestValidator : AbstractValidator<AssignClientRequest>
{
    public AssignClientRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
