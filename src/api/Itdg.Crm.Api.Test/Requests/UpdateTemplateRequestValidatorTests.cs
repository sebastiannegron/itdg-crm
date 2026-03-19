namespace Itdg.Crm.Api.Test.Requests;

using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Requests;

public class UpdateTemplateRequestValidatorTests
{
    private readonly UpdateTemplateRequestValidator _validator = new();

    private static UpdateTemplateRequest CreateValidRequest() => new()
    {
        Category = TemplateCategory.General,
        Name = "Updated Template",
        SubjectTemplate = "Updated Subject {{client_name}}",
        BodyTemplate = "Updated body text for {{client_name}}.",
        Language = "es-pr"
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
    public void Validate_ShouldFail_WhenNameIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_ShouldFail_WhenSubjectTemplateIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SubjectTemplate = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SubjectTemplate");
    }

    [Fact]
    public void Validate_ShouldFail_WhenBodyTemplateIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BodyTemplate = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BodyTemplate");
    }

    [Fact]
    public void Validate_ShouldFail_WhenLanguageIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Language = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Language");
    }
}
