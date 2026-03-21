namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class DraftEmailRequestValidator : AbstractValidator<DraftEmailRequest>
{
    private static readonly string[] SupportedLanguages = ["en", "en-pr", "es", "es-pr"];

    public DraftEmailRequestValidator()
    {
        RuleFor(x => x.ClientName)
            .NotEmpty().WithMessage("Client name is required.")
            .MaximumLength(200).WithMessage("Client name must not exceed 200 characters.");

        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required.")
            .MaximumLength(500).WithMessage("Topic must not exceed 500 characters.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .Must(lang => SupportedLanguages.Contains(lang, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Language must be one of: en, en-pr, es, es-pr.");

        RuleFor(x => x.AdditionalContext)
            .MaximumLength(2000).WithMessage("Additional context must not exceed 2000 characters.")
            .When(x => x.AdditionalContext is not null);
    }
}
