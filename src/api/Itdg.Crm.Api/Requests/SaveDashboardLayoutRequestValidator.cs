namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class SaveDashboardLayoutRequestValidator : AbstractValidator<SaveDashboardLayoutRequest>
{
    public SaveDashboardLayoutRequestValidator()
    {
        RuleFor(x => x.WidgetConfigurations)
            .NotEmpty().WithMessage("Widget configurations are required.")
            .MaximumLength(8000).WithMessage("Widget configurations must not exceed 8000 characters.");
    }
}
