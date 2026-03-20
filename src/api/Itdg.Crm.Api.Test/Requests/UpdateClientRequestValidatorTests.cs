namespace Itdg.Crm.Api.Test.Requests;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public class UpdateClientRequestValidatorTests
{
    private readonly UpdateClientRequestValidator _validator = new();

    private static UpdateClientRequest CreateValidRequest() => new()
    {
        Name = "Updated Client",
        ContactEmail = "updated@example.com",
        Phone = "787-555-5678",
        Address = "456 Oak Ave, San Juan, PR",
        Status = ClientStatus.Active,
        IndustryTag = "Finance",
        Notes = "Updated notes",
        CustomFields = "{\"key\":\"updated\"}"
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
    public void Validate_ShouldPass_WhenOptionalFieldsAreNull()
    {
        // Arrange
        var request = new UpdateClientRequest
        {
            Name = "Updated Client",
            Status = ClientStatus.Inactive
        };

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
    public void Validate_ShouldFail_WhenContactEmailIsInvalid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ContactEmail = "not-an-email";

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContactEmail");
    }

    [Fact]
    public void Validate_ShouldFail_WhenContactEmailExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ContactEmail = new string('a', 192) + "@test.com";

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContactEmail");
    }

    [Fact]
    public void Validate_ShouldFail_WhenPhoneExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Phone = new string('1', 51);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Phone");
    }

    [Fact]
    public void Validate_ShouldFail_WhenAddressExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Address = new string('a', 501);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Address");
    }

    [Fact]
    public void Validate_ShouldFail_WhenNotesExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Notes = new string('a', 2001);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes");
    }

    [Fact]
    public void Validate_ShouldFail_WhenIndustryTagExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.IndustryTag = new string('a', 101);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "IndustryTag");
    }

    [Fact]
    public void Validate_ShouldFail_WhenCustomFieldsExceedsMaxLength()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CustomFields = new string('a', 4001);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomFields");
    }

    [Fact]
    public void Validate_ShouldFail_WhenStatusIsInvalid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Status = (ClientStatus)999;

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }
}
