namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class CreateTierHandler : ICommandHandler<CreateTier>
{
    private readonly IGenericRepository<ClientTier> _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateTierHandler> _logger;

    public CreateTierHandler(
        IGenericRepository<ClientTier> repository,
        ITenantProvider tenantProvider,
        ILogger<CreateTierHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task HandleAsync(CreateTier command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Create Tier");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Creating tier {TierName} | CorrelationId: {CorrelationId}", command.Name, correlationId);

        var tier = new ClientTier
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            SortOrder = command.SortOrder,
            TenantId = _tenantProvider.GetTenantId()
        };

        await _repository.AddAsync(tier, cancellationToken);

        _logger.LogInformation("Tier {TierId} created successfully | CorrelationId: {CorrelationId}", tier.Id, correlationId);
    }
}
