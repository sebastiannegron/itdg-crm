namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class SearchDocumentsRequestValidator : AbstractValidator<SearchDocumentsRequest>
{
    public SearchDocumentsRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Search query is required.")
            .MaximumLength(200).WithMessage("Search query must not exceed 200 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(200).WithMessage("Category must not exceed 200 characters.");

        RuleFor(x => x.DateTo)
            .GreaterThanOrEqualTo(x => x.DateFrom)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}
