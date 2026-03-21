namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Diagnostics;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GmailSyncBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly GmailSyncOptions _options;
    private readonly ILogger<GmailSyncBackgroundService> _logger;

    public GmailSyncBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<GmailSyncOptions> options,
        ILogger<GmailSyncBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Gmail sync is disabled. Background service will not run");
            return;
        }

        _logger.LogInformation(
            "Gmail sync background service started with interval {Interval}",
            _options.TimeBetweenRuns);

        using var timer = new PeriodicTimer(_options.TimeBetweenRuns);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SyncEmailsAsync(stoppingToken);
        }
    }

    internal async Task SyncEmailsAsync(CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Gmail Sync Emails");

        try
        {
            _logger.LogInformation("Starting Gmail email sync cycle");

            await using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
            var gmailService = scope.ServiceProvider.GetRequiredService<IGmailService>();

            var clients = await dbContext.Set<Client>()
                .Where(c => c.ContactEmail != null)
                .Select(c => new { c.Id, c.TenantId, c.ContactEmail })
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {ClientCount} clients with email addresses to sync", clients.Count);

            foreach (var client in clients)
            {
                try
                {
                    await SyncClientEmailsAsync(
                        dbContext,
                        gmailService,
                        client.Id,
                        client.TenantId,
                        client.ContactEmail!,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to sync emails for client {ClientId} ({Email}). Skipping",
                        client.Id,
                        client.ContactEmail);
                }
            }

            _logger.LogInformation("Gmail email sync cycle completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gmail email sync cycle failed");
        }
    }

    private async Task SyncClientEmailsAsync(
        DbContext dbContext,
        IGmailService gmailService,
        Guid clientId,
        Guid tenantId,
        string clientEmail,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Gmail Sync Client Emails");
        activity?.SetTag("ClientId", clientId);
        activity?.SetTag("ClientEmail", clientEmail);

        var existingMessageIds = await dbContext.Set<EmailMirror>()
            .Where(e => e.ClientId == clientId)
            .Select(e => e.GmailMessageId)
            .ToListAsync(cancellationToken);

        var existingMessageIdSet = new HashSet<string>(existingMessageIds);

        var query = $"from:{clientEmail} OR to:{clientEmail}";
        var messageList = await gmailService.ListMessagesAsync(
            userAccessToken: string.Empty,
            query: query,
            maxResults: 50,
            cancellationToken: cancellationToken);

        var newMessages = messageList.Messages
            .Where(m => !existingMessageIdSet.Contains(m.Id))
            .ToList();

        if (newMessages.Count == 0)
        {
            _logger.LogDebug("No new emails found for client {ClientId}", clientId);
            return;
        }

        foreach (var message in newMessages)
        {
            var emailMirror = new EmailMirror
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                ClientId = clientId,
                GmailMessageId = message.Id,
                GmailThreadId = message.ThreadId,
                Subject = message.Subject,
                From = message.From,
                To = message.To,
                BodyPreview = message.BodyPreview,
                HasAttachments = message.HasAttachments,
                ReceivedAt = message.Date
            };

            await dbContext.Set<EmailMirror>().AddAsync(emailMirror, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Synced {NewCount} new emails for client {ClientId}",
            newMessages.Count,
            clientId);
    }
}
