namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientAssignmentsHandler : IQueryHandler<GetClientAssignments, IEnumerable<ClientAssignmentDto>>
{
    private readonly IClientAssignmentRepository _repository;
    private readonly ILogger<GetClientAssignmentsHandler> _logger;

    public GetClientAssignmentsHandler(
        IClientAssignmentRepository repository,
        ILogger<GetClientAssignmentsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<ClientAssignmentDto>> HandleAsync(GetClientAssignments query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Client Assignments");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting assignments for client {ClientId} | CorrelationId: {CorrelationId}",
            query.ClientId, correlationId);

        var assignments = await _repository.GetByClientIdWithUserAsync(query.ClientId, cancellationToken);

        return assignments.Select(a => new ClientAssignmentDto(
            UserId: a.UserId,
            DisplayName: a.User?.DisplayName ?? string.Empty,
            Email: a.User?.Email ?? string.Empty,
            AssignedAt: a.AssignedAt
        ));
    }
}
