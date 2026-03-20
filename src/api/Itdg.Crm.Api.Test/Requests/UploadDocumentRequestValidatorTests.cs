namespace Itdg.Crm.Api.Test.Requests;

using Itdg.Crm.Api.Requests;

public class UploadDocumentRequestValidatorTests
{
    private readonly UploadDocumentRequestValidator _validator = new();

    private static UploadDocumentRequest CreateValidRequest() => new()
    {
        CategoryId = Guid.NewGuid(),
        GoogleDriveParentFolderId = "parent-folder-123"
    };

    [Fact]
    public void Validate_ShouldPass_WhenRequestIsValid()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenGoogleDriveParentFolderIdIsNull()
    {
        // Arrange
        var request = new UploadDocumentRequest
        {
            CategoryId = Guid.NewGuid(),
            GoogleDriveParentFolderId = null
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenCategoryIdIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CategoryId = Guid.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }

    [Fact]
    public void Validate_ShouldFail_WhenGoogleDriveParentFolderIdExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoogleDriveParentFolderId = new string('a', 201);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "GoogleDriveParentFolderId");
    }
}
