namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientsHandler : IQueryHandler<GetClients, PaginatedResultDto<ClientDto>>
{
    private readonly IClientRepository _repository;
    private readonly ILogger<GetClientsHandler> _logger;

    public GetClientsHandler(
        IClientRepository repository,
        ILogger<GetClientsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<ClientDto>> HandleAsync(GetClients query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Clients");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting clients page {Page} | CorrelationId: {CorrelationId}", query.Page, correlationId);

        var (items, totalCount) = await _repository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Status,
            query.TierId,
            query.Search,
            cancellationToken);

        var clientDtos = items.Select(client => new ClientDto(
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
        )).ToList();

        return new PaginatedResultDto<ClientDto>(
            Items: clientDtos,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }
}
