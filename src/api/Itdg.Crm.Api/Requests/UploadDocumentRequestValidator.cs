namespace Itdg.Crm.Api.Requests;

using FluentValidation;

public class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
{
    public UploadDocumentRequestValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.GoogleDriveParentFolderId)
            .MaximumLength(200).WithMessage("Google Drive parent folder ID must not exceed 200 characters.");
    }
}
