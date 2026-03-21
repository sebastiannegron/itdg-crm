namespace Itdg.Crm.Api.Test.Options;

using System.ComponentModel.DataAnnotations;
using Itdg.Crm.Api.Infrastructure.Options;

public class GmailSyncOptionsTests
{
    [Fact]
    public void Key_IsGmailSync()
    {
        // Assert
        GmailSyncOptions.Key.Should().Be("GmailSync");
    }

    [Fact]
    public void Enabled_DefaultsToTrue()
    {
        // Arrange
        var options = new GmailSyncOptions();

        // Assert
        options.Enabled.Should().BeTrue();
    }

    [Fact]
    public void TimeBetweenRuns_DefaultsToFiveMinutes()
    {
        // Arrange
        var options = new GmailSyncOptions();

        // Assert
        options.TimeBetweenRuns.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void MaxMessagesPerSync_DefaultsTo50()
    {
        // Arrange
        var options = new GmailSyncOptions();

        // Assert
        options.MaxMessagesPerSync.Should().Be(50);
    }

    [Fact]
    public void Enabled_CanBeSetToFalse()
    {
        // Arrange
        var options = new GmailSyncOptions { Enabled = false };

        // Assert
        options.Enabled.Should().BeFalse();
    }

    [Fact]
    public void TimeBetweenRuns_CanBeOverridden()
    {
        // Arrange
        var options = new GmailSyncOptions { TimeBetweenRuns = TimeSpan.FromMinutes(10) };

        // Assert
        options.TimeBetweenRuns.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public void MaxMessagesPerSync_CanBeOverridden()
    {
        // Arrange
        var options = new GmailSyncOptions { MaxMessagesPerSync = 100 };

        // Assert
        options.MaxMessagesPerSync.Should().Be(100);
    }

    [Fact]
    public void Validation_Succeeds_WithDefaults()
    {
        // Arrange
        var options = new GmailSyncOptions();
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(501)]
    public void Validation_Fails_WhenMaxMessagesPerSyncOutOfRange(int value)
    {
        // Arrange
        var options = new GmailSyncOptions { MaxMessagesPerSync = value };
        var context = new ValidationContext(options);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(GmailSyncOptions.MaxMessagesPerSync)));
    }
}
