namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateUserHandler : ICommandHandler<UpdateUser>
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UpdateUserHandler> _logger;

    public UpdateUserHandler(
        IUserRepository repository,
        ILogger<UpdateUserHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateUser command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Update User");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Updating user {UserId} | CorrelationId: {CorrelationId}", command.UserId, correlationId);

        var user = await _repository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), command.UserId);

        user.Role = command.Role;
        user.IsActive = command.IsActive;

        await _repository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User {UserId} updated successfully | CorrelationId: {CorrelationId}", command.UserId, correlationId);
    }
}
