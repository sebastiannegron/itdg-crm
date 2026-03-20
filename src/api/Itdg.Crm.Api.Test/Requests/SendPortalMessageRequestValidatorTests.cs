namespace Itdg.Crm.Api.Test.Requests;

using Itdg.Crm.Api.Requests;

public class SendPortalMessageRequestValidatorTests
{
    private readonly SendPortalMessageRequestValidator _validator = new();

    private static SendPortalMessageRequest CreateValidRequest() => new()
    {
        Subject = "Test Subject",
        Body = "Test body content"
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
    public void Validate_ShouldFail_WhenSubjectIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Subject = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Subject");
    }

    [Fact]
    public void Validate_ShouldFail_WhenSubjectExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Subject = new string('a', 501);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Subject");
    }

    [Fact]
    public void Validate_ShouldFail_WhenBodyIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Body = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Body");
    }

    [Fact]
    public void Validate_ShouldFail_WhenBodyExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Body = new string('a', 4001);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Body");
    }
}
