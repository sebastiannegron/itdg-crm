namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateClientHandler : ICommandHandler<UpdateClient>
{
    private readonly IClientRepository _repository;
    private readonly ILogger<UpdateClientHandler> _logger;

    public UpdateClientHandler(
        IClientRepository repository,
        ILogger<UpdateClientHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateClient command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Update Client");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Updating client {ClientId} | CorrelationId: {CorrelationId}", command.ClientId, correlationId);

        var client = await _repository.GetByIdAsync(command.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), command.ClientId);

        client.Name = command.Name;
        client.ContactEmail = command.ContactEmail;
        client.Phone = command.Phone;
        client.Address = command.Address;
        client.TierId = command.TierId;
        client.Status = command.Status;
        client.IndustryTag = command.IndustryTag;
        client.Notes = command.Notes;
        client.CustomFields = command.CustomFields;

        await _repository.UpdateAsync(client, cancellationToken);

        _logger.LogInformation("Client {ClientId} updated successfully | CorrelationId: {CorrelationId}", command.ClientId, correlationId);
    }
}
