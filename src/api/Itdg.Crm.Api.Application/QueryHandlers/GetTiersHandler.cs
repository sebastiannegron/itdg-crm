namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetTiersHandler : IQueryHandler<GetTiers, IEnumerable<ClientTierDto>>
{
    private readonly IGenericRepository<ClientTier> _repository;
    private readonly ILogger<GetTiersHandler> _logger;

    public GetTiersHandler(
        IGenericRepository<ClientTier> repository,
        ILogger<GetTiersHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ClientTierDto>> HandleAsync(GetTiers query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Tiers");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting all tiers | CorrelationId: {CorrelationId}", correlationId);

        var tiers = await _repository.GetAllAsync(cancellationToken);

        return tiers
            .OrderBy(t => t.SortOrder)
            .Select(t => new ClientTierDto(
                t.Id,
                t.Name,
                t.SortOrder,
                t.CreatedAt,
                t.UpdatedAt
            ));
    }
}
