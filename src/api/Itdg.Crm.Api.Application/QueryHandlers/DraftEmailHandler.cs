namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Microsoft.Extensions.Logging;

public class DraftEmailHandler : IQueryHandler<DraftEmail, string>
{
    private readonly IAiDraftingService _aiDraftingService;
    private readonly ILogger<DraftEmailHandler> _logger;

    public DraftEmailHandler(IAiDraftingService aiDraftingService, ILogger<DraftEmailHandler> logger)
    {
        _aiDraftingService = aiDraftingService;
        _logger = logger;
    }

    public async Task<string> HandleAsync(DraftEmail query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Draft Email");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Generating AI email draft for client {ClientName} in {Language} | CorrelationId: {CorrelationId}",
            query.ClientName, query.Language, correlationId);

        var request = new AiDraftRequest(query.ClientName, query.Topic, query.Language, query.AdditionalContext);
        var draft = await _aiDraftingService.GenerateDraftAsync(request, cancellationToken);

        _logger.LogInformation("Successfully generated AI email draft for client {ClientName} | CorrelationId: {CorrelationId}",
            query.ClientName, correlationId);

        return draft;
    }
}
