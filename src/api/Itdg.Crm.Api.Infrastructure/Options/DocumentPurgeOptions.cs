namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class DocumentPurgeOptions
{
    public const string Key = "DocumentPurge";

    public bool Enabled { get; set; } = true;

    [Range(typeof(TimeSpan), "00:01:00", "1.00:00:00",
        ErrorMessage = "TimeBetweenRuns must be between 1 minute and 1 day.")]
    public TimeSpan TimeBetweenRuns { get; set; } = TimeSpan.FromHours(24);

    [Range(1, 365, ErrorMessage = "RetentionDays must be between 1 and 365.")]
    public int RetentionDays { get; set; } = 30;
}
