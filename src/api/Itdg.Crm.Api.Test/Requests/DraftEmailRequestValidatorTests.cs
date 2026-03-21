namespace Itdg.Crm.Api.Test.Requests;

using Itdg.Crm.Api.Requests;
using FluentValidation.TestHelper;

public class DraftEmailRequestValidatorTests
{
    private readonly DraftEmailRequestValidator _validator = new();

    [Fact]
    public void Validate_ValidRequest_HasNoErrors()
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = "John Doe",
            Topic = "Tax return filing deadline",
            Language = "en"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ValidRequest_WithAdditionalContext_HasNoErrors()
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = "María García",
            Topic = "Fecha límite de radicación",
            Language = "es-pr",
            AdditionalContext = "Client has outstanding balance of $5,000"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("en")]
    [InlineData("en-pr")]
    [InlineData("es")]
    [InlineData("es-pr")]
    public void Validate_SupportedLanguages_HasNoErrors(string language)
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = "Test Client",
            Topic = "Test Topic",
            Language = language
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Language);
    }

    [Fact]
    public void Validate_EmptyClientName_HasError()
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = "",
            Topic = "Test Topic",
            Language = "en"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientName);
    }

    [Fact]
    public void Validate_EmptyTopic_HasError()
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = "John Doe",
            Topic = "",
            Language = "en"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Topic);
    }

    [Fact]
    public void Validate_EmptyLanguage_HasError()
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = "John Doe",
            Topic = "Test Topic",
            Language = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Language);
    }

    [Fact]
    public void Validate_UnsupportedLanguage_HasError()
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = "John Doe",
            Topic = "Test Topic",
            Language = "fr"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Language);
    }

    [Fact]
    public void Validate_ClientNameTooLong_HasError()
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = new string('A', 201),
            Topic = "Test Topic",
            Language = "en"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ClientName);
    }

    [Fact]
    public void Validate_TopicTooLong_HasError()
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = "John Doe",
            Topic = new string('A', 501),
            Language = "en"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Topic);
    }

    [Fact]
    public void Validate_AdditionalContextTooLong_HasError()
    {
        // Arrange
        var request = new DraftEmailRequest
        {
            ClientName = "John Doe",
            Topic = "Test Topic",
            Language = "en",
            AdditionalContext = new string('A', 2001)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AdditionalContext);
    }
}
