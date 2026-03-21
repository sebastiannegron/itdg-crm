namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Diagnostics;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class DocumentPurgeBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DocumentPurgeOptions _options;
    private readonly ILogger<DocumentPurgeBackgroundService> _logger;

    public DocumentPurgeBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<DocumentPurgeOptions> options,
        ILogger<DocumentPurgeBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Document purge is disabled. Background service will not run");
            return;
        }

        _logger.LogInformation(
            "Document purge background service started with interval {Interval} and retention {RetentionDays} days",
            _options.TimeBetweenRuns,
            _options.RetentionDays);

        using var timer = new PeriodicTimer(_options.TimeBetweenRuns);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await PurgeDeletedDocumentsAsync(stoppingToken);
        }
    }

    internal async Task PurgeDeletedDocumentsAsync(CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Purge Deleted Documents");

        try
        {
            _logger.LogInformation("Starting document purge cycle");

            await using var scope = _serviceProvider.CreateAsyncScope();
            var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();

            var cutoffDate = DateTimeOffset.UtcNow.AddDays(-_options.RetentionDays);
            var expiredDocuments = await documentRepository.GetDocumentsDeletedBeforeAsync(cutoffDate, cancellationToken);

            if (expiredDocuments.Count == 0)
            {
                _logger.LogDebug("No expired documents found for purging");
                return;
            }

            foreach (var document in expiredDocuments)
            {
                try
                {
                    await documentRepository.DeleteAsync(document, cancellationToken);
                    _logger.LogInformation(
                        "Permanently deleted document {DocumentId} (deleted at {DeletedAt})",
                        document.Id,
                        document.DeletedAt);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to permanently delete document {DocumentId}. Skipping",
                        document.Id);
                }
            }

            _logger.LogInformation("Document purge cycle completed. Purged {Count} documents", expiredDocuments.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document purge cycle failed");
        }
    }
}
