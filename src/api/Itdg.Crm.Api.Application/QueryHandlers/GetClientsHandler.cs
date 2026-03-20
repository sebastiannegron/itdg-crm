namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientsHandler : IQueryHandler<GetClients, PaginatedResultDto<ClientDto>>
{
    private readonly IClientRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientsHandler> _logger;

    public GetClientsHandler(
        IClientRepository repository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetClientsHandler> logger)
    {
        _repository = repository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<ClientDto>> HandleAsync(GetClients query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Clients");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting clients page {Page} | CorrelationId: {CorrelationId}", query.Page, correlationId);

        Guid? assignedUserId = null;
        if (!_currentUserProvider.IsInRole(nameof(UserRole.Administrator)))
        {
            var entraObjectId = _currentUserProvider.GetEntraObjectId();
            if (!string.IsNullOrWhiteSpace(entraObjectId))
            {
                var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
                if (user is null)
                {
                    _logger.LogWarning("Associate user not found in database for EntraObjectId {EntraObjectId} | CorrelationId: {CorrelationId}", entraObjectId, correlationId);
                    return new PaginatedResultDto<ClientDto>(Items: [], TotalCount: 0, Page: query.Page, PageSize: query.PageSize);
                }

                assignedUserId = user.Id;
            }
        }

        var (items, totalCount) = await _repository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Status,
            query.TierId,
            query.Search,
            assignedUserId,
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
