namespace Itdg.Crm.Api.Test.Requests;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public class InviteUserRequestValidatorTests
{
    private readonly InviteUserRequestValidator _validator = new();

    private static InviteUserRequest CreateValidRequest() => new()
    {
        Email = "newuser@example.com",
        DisplayName = "New User",
        Role = UserRole.Associate
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
    public void Validate_ShouldFail_WhenEmailIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_ShouldFail_WhenEmailIsInvalid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = "not-an-email";

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_ShouldFail_WhenEmailExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = new string('a', 192) + "@test.com";

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDisplayNameIsEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DisplayName = string.Empty;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDisplayNameIsTooShort()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DisplayName = "A";

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDisplayNameExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DisplayName = new string('a', 201);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public void Validate_ShouldFail_WhenRoleIsInvalid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Role = (UserRole)999;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Role");
    }
}
