namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UnassignClientHandler : ICommandHandler<UnassignClient>
{
    private readonly IClientAssignmentRepository _repository;
    private readonly ILogger<UnassignClientHandler> _logger;

    public UnassignClientHandler(
        IClientAssignmentRepository repository,
        ILogger<UnassignClientHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(UnassignClient command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Unassign Client");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Unassigning client {ClientId} from user {UserId} | CorrelationId: {CorrelationId}",
            command.ClientId, command.UserId, correlationId);

        var assignment = await _repository.GetByClientAndUserAsync(command.ClientId, command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(ClientAssignment), $"{command.ClientId}/{command.UserId}");

        await _repository.DeleteAsync(assignment, cancellationToken);

        _logger.LogInformation("Client {ClientId} unassigned from user {UserId} successfully | CorrelationId: {CorrelationId}",
            command.ClientId, command.UserId, correlationId);
    }
}
