namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class ReorderDocumentCategoriesRequestValidator : AbstractValidator<ReorderDocumentCategoriesRequest>
{
    public ReorderDocumentCategoriesRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Items are required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category ID is required.");

            item.RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Sort order must be zero or greater.");
        });
    }
}
