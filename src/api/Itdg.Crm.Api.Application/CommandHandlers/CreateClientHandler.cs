namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class CreateClientHandler : ICommandHandler<CreateClient>
{
    private readonly IClientRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateClientHandler> _logger;

    public CreateClientHandler(
        IClientRepository repository,
        ITenantProvider tenantProvider,
        ILogger<CreateClientHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task HandleAsync(CreateClient command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Create Client");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Creating client {ClientName} | CorrelationId: {CorrelationId}", command.Name, correlationId);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            ContactEmail = command.ContactEmail,
            Phone = command.Phone,
            Address = command.Address,
            TierId = command.TierId,
            Status = command.Status,
            IndustryTag = command.IndustryTag,
            Notes = command.Notes,
            CustomFields = command.CustomFields,
            TenantId = _tenantProvider.GetTenantId()
        };

        await _repository.AddAsync(client, cancellationToken);

        _logger.LogInformation("Client {ClientId} created successfully | CorrelationId: {CorrelationId}", client.Id, correlationId);
    }
}
