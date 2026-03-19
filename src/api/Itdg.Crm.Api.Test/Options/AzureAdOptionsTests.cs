namespace Itdg.Crm.Api.Test.Options;

using System.ComponentModel.DataAnnotations;
using Itdg.Crm.Api.Infrastructure.Options;

public class AzureAdOptionsTests
{
    [Fact]
    public void Key_IsAzureAd()
    {
        // Assert
        AzureAdOptions.Key.Should().Be("AzureAd");
    }

    [Fact]
    public void Validation_Succeeds_WhenAllFieldsProvided()
    {
        // Arrange
        var options = new AzureAdOptions
        {
            Instance = "https://login.microsoftonline.com/",
            TenantId = "00000000-0000-0000-0000-000000000000",
            ClientId = "00000000-0000-0000-0000-000000000000",
            Audience = "api://00000000-0000-0000-0000-000000000000"
        };

        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "tenant", "client", "audience", "Instance")]
    [InlineData("instance", null, "client", "audience", "TenantId")]
    [InlineData("instance", "tenant", null, "audience", "ClientId")]
    [InlineData("instance", "tenant", "client", null, "Audience")]
    public void Validation_Fails_WhenRequiredFieldMissing(
        string? instance, string? tenantId, string? clientId, string? audience, string expectedMember)
    {
        // Arrange
        var options = new AzureAdOptions
        {
            Instance = instance!,
            TenantId = tenantId!,
            ClientId = clientId!,
            Audience = audience!
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
