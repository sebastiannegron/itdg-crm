namespace Itdg.Crm.Api.Test.Requests;

using Itdg.Crm.Api.Requests;

public class AssignClientRequestValidatorTests
{
    private readonly AssignClientRequestValidator _validator;

    public AssignClientRequestValidatorTests()
    {
        _validator = new AssignClientRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_Passes()
    {
        // Arrange
        var request = new AssignClientRequest
        {
            UserId = Guid.NewGuid()
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyUserId_Fails()
    {
        // Arrange
        var request = new AssignClientRequest
        {
            UserId = Guid.Empty
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}
