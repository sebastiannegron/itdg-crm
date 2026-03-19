namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DeleteClientHandler : ICommandHandler<DeleteClient>
{
    private readonly IClientRepository _repository;
    private readonly ILogger<DeleteClientHandler> _logger;

    public DeleteClientHandler(
        IClientRepository repository,
        ILogger<DeleteClientHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(DeleteClient command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Delete Client");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Deleting client {ClientId} | CorrelationId: {CorrelationId}", command.ClientId, correlationId);

        var client = await _repository.GetByIdAsync(command.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), command.ClientId);

        client.DeletedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(client, cancellationToken);

        _logger.LogInformation("Client {ClientId} soft-deleted successfully | CorrelationId: {CorrelationId}", command.ClientId, correlationId);
    }
}
