namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateTierHandler : ICommandHandler<UpdateTier>
{
    private readonly IGenericRepository<ClientTier> _repository;
    private readonly ILogger<UpdateTierHandler> _logger;

    public UpdateTierHandler(
        IGenericRepository<ClientTier> repository,
        ILogger<UpdateTierHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateTier command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Update Tier");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Updating tier {TierId} | CorrelationId: {CorrelationId}", command.TierId, correlationId);

        var tier = await _repository.GetByIdAsync(command.TierId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClientTier), command.TierId);

        tier.Name = command.Name;
        tier.SortOrder = command.SortOrder;

        await _repository.UpdateAsync(tier, cancellationToken);

        _logger.LogInformation("Tier {TierId} updated successfully | CorrelationId: {CorrelationId}", command.TierId, correlationId);
    }
}
