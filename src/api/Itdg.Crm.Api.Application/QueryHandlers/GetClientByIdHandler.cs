namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientByIdHandler : IQueryHandler<GetClientById, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly ILogger<GetClientByIdHandler> _logger;

    public GetClientByIdHandler(
        IClientRepository repository,
        ILogger<GetClientByIdHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ClientDto> HandleAsync(GetClientById query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Client By Id");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting client {ClientId} | CorrelationId: {CorrelationId}", query.ClientId, correlationId);

        var client = await _repository.GetByIdWithTierAsync(query.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), query.ClientId);

        return new ClientDto(
            ClientId: client.Id,
            Name: client.Name,
            ContactEmail: client.ContactEmail,
            Phone: client.Phone,
            Address: client.Address,
            TierId: client.TierId,
            TierName: client.Tier?.Name,
            Status: client.Status.ToString(),
            IndustryTag: client.IndustryTag,
            Notes: client.Notes,
            CustomFields: client.CustomFields,
            CreatedAt: client.CreatedAt,
            UpdatedAt: client.UpdatedAt
        );
    }
}
