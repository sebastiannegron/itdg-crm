namespace Itdg.Crm.Api.Test.Requests;

using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Requests;

public class CreateTemplateRequestValidatorTests
{
    private readonly CreateTemplateRequestValidator _validator = new();

    private static CreateTemplateRequest CreateValidRequest() => new()
    {
        Category = TemplateCategory.Onboarding,
        Name = "Welcome Template",
        SubjectTemplate = "Welcome {{client_name}}",
        BodyTemplate = "Hello {{client_name}}, welcome to our service!",
        Language = "en-pr"
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
    public void Validate_ShouldFail_WhenNameExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Name = new string('a', 201);

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

    [Fact]
    public void Validate_ShouldFail_WhenLanguageExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Language = new string('a', 11);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Language");
    }
}
