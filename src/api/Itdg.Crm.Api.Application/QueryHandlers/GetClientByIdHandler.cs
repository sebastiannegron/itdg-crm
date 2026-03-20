namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientByIdHandler : IQueryHandler<GetClientById, ClientDto>
{
    private readonly IClientRepository _repository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientByIdHandler> _logger;

    public GetClientByIdHandler(
        IClientRepository repository,
        IClientAssignmentRepository clientAssignmentRepository,
        IUserRepository userRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetClientByIdHandler> logger)
    {
        _repository = repository;
        _clientAssignmentRepository = clientAssignmentRepository;
        _userRepository = userRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<ClientDto> HandleAsync(GetClientById query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Client By Id");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting client {ClientId} | CorrelationId: {CorrelationId}", query.ClientId, correlationId);

        var client = await _repository.GetByIdWithTierAsync(query.ClientId, cancellationToken)
            ?? throw new NotFoundException(nameof(Client), query.ClientId);

        if (!_currentUserProvider.IsInRole(nameof(UserRole.Administrator)))
        {
            var entraObjectId = _currentUserProvider.GetEntraObjectId();
            if (!string.IsNullOrWhiteSpace(entraObjectId))
            {
                var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
                if (user is null)
                {
                    _logger.LogWarning("Associate user not found in database for EntraObjectId {EntraObjectId} | CorrelationId: {CorrelationId}", entraObjectId, correlationId);
                    throw new NotFoundException(nameof(Client), query.ClientId);
                }

                var isAssigned = await _clientAssignmentRepository.ExistsAsync(user.Id, query.ClientId, cancellationToken);
                if (!isAssigned)
                {
                    throw new NotFoundException(nameof(Client), query.ClientId);
                }
            }
        }

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
