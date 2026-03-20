namespace Itdg.Crm.Api.Test.Requests;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator = new();

    private static UpdateUserRequest CreateValidRequest() => new()
    {
        Role = UserRole.Associate,
        IsActive = true
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
    public void Validate_ShouldPass_WhenRoleIsAdministrator()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Role = UserRole.Administrator;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenIsActiveIsFalse()
    {
        // Arrange
        var request = CreateValidRequest();
        request.IsActive = false;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
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
