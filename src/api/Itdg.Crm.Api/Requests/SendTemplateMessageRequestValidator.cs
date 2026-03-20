namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class SendTemplateMessageRequestValidator : AbstractValidator<SendTemplateMessageRequest>
{
    public SendTemplateMessageRequestValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("Template ID is required.");

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required.");

        RuleFor(x => x.MergeFields)
            .NotNull().WithMessage("Merge fields are required.");

        RuleFor(x => x)
            .Must(x => x.SendViaPortal || x.SendViaEmail)
            .WithMessage("At least one delivery channel (portal or email) must be selected.");

        RuleFor(x => x.RecipientEmail)
            .NotEmpty().WithMessage("Recipient email is required when sending via email.")
            .When(x => x.SendViaEmail);
    }
}
