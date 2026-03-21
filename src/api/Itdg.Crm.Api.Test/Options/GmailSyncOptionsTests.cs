namespace Itdg.Crm.Api.Test.Options;

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
}
