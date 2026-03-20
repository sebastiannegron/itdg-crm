namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class RenderTemplateRequestValidator : AbstractValidator<RenderTemplateRequest>
{
    public RenderTemplateRequestValidator()
    {
        RuleFor(x => x.MergeFields)
            .NotNull().WithMessage("Merge fields are required.");
    }
}
