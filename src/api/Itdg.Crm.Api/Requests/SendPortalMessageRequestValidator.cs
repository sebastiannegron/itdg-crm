namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class SendPortalMessageRequestValidator : AbstractValidator<SendPortalMessageRequest>
{
    public SendPortalMessageRequestValidator()
    {
        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required.")
            .MaximumLength(500).WithMessage("Subject must not exceed 500 characters.");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Body is required.")
            .MaximumLength(4000).WithMessage("Body must not exceed 4000 characters.");
    }
}
