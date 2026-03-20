namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class AssignClientHandler : ICommandHandler<AssignClient>
{
    private readonly IClientAssignmentRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<AssignClientHandler> _logger;

    public AssignClientHandler(
        IClientAssignmentRepository repository,
        ITenantProvider tenantProvider,
        ILogger<AssignClientHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task HandleAsync(AssignClient command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Assign Client");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Assigning client {ClientId} to user {UserId} | CorrelationId: {CorrelationId}",
            command.ClientId, command.UserId, correlationId);

        var exists = await _repository.ExistsAsync(command.UserId, command.ClientId, cancellationToken);
        if (exists)
        {
            throw new ConflictException($"Client '{command.ClientId}' is already assigned to user '{command.UserId}'.");
        }

        var assignment = new ClientAssignment
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            UserId = command.UserId,
            AssignedAt = DateTimeOffset.UtcNow,
            TenantId = _tenantProvider.GetTenantId()
        };

        await _repository.AddAsync(assignment, cancellationToken);

        _logger.LogInformation("Client {ClientId} assigned to user {UserId} successfully | CorrelationId: {CorrelationId}",
            command.ClientId, command.UserId, correlationId);
    }
}
