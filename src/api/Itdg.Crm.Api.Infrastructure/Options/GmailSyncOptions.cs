namespace Itdg.Crm.Api.Infrastructure.Options;

public class GmailSyncOptions
{
    public const string Key = "GmailSync";

    public bool Enabled { get; set; } = true;

    public TimeSpan TimeBetweenRuns { get; set; } = TimeSpan.FromMinutes(5);
}
