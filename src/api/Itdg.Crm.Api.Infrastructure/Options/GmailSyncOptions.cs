namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class GmailSyncOptions
{
    public const string Key = "GmailSync";

    public bool Enabled { get; set; } = true;

    [Range(typeof(TimeSpan), "00:00:30", "1.00:00:00",
        ErrorMessage = "TimeBetweenRuns must be between 30 seconds and 1 day.")]
    public TimeSpan TimeBetweenRuns { get; set; } = TimeSpan.FromMinutes(5);

    [Range(1, 500, ErrorMessage = "MaxMessagesPerSync must be between 1 and 500.")]
    public int MaxMessagesPerSync { get; set; } = 50;
}
