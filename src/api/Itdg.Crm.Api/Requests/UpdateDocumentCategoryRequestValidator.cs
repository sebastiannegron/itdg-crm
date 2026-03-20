namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class UpdateDocumentCategoryRequestValidator : AbstractValidator<UpdateDocumentCategoryRequest>
{
    public UpdateDocumentCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.NamingConvention)
            .MaximumLength(200).WithMessage("Naming convention must not exceed 200 characters.");

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Sort order must be zero or greater.");
    }
}
