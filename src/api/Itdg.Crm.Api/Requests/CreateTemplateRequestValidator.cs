namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class CreateTemplateRequestValidator : AbstractValidator<CreateTemplateRequest>
{
    public CreateTemplateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.SubjectTemplate)
            .NotEmpty().WithMessage("Subject template is required.")
            .MaximumLength(500).WithMessage("Subject template must not exceed 500 characters.");

        RuleFor(x => x.BodyTemplate)
            .NotEmpty().WithMessage("Body template is required.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(10).WithMessage("Language must not exceed 10 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid template category.");
    }
}
