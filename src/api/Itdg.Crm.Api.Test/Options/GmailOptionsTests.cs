namespace Itdg.Crm.Api.Test.Options;

using System.ComponentModel.DataAnnotations;
using Itdg.Crm.Api.Infrastructure.Options;

public class GmailOptionsTests
{
    [Fact]
    public void Key_IsGmail()
    {
        // Assert
        GmailOptions.Key.Should().Be("Gmail");
    }

    [Fact]
    public void Validation_Succeeds_WhenAllRequiredFieldsProvided()
    {
        // Arrange
        var options = new GmailOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret"
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Fact]
    public void ApplicationName_DefaultsToItdgCrm()
    {
        // Arrange
        var options = new GmailOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret"
        };

        // Assert
        options.ApplicationName.Should().Be("ITDG-CRM");
    }

    [Fact]
    public void ApplicationName_CanBeOverridden()
    {
        // Arrange
        var options = new GmailOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            ApplicationName = "Custom App"
        };

        // Assert
        options.ApplicationName.Should().Be("Custom App");
    }

    [Theory]
    [InlineData(null, "secret", "ClientId")]
    [InlineData("client", null, "ClientSecret")]
    public void Validation_Fails_WhenRequiredFieldMissing(
        string? clientId, string? clientSecret, string expectedMember)
    {
        // Arrange
        var options = new GmailOptions
        {
            ClientId = clientId!,
            ClientSecret = clientSecret!
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains(expectedMember));
    }
}
