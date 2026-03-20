namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class UpdateTierRequestValidator : AbstractValidator<UpdateTierRequest>
{
    public UpdateTierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be zero or greater.");
    }
}
