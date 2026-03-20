namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class InviteUserHandler : ICommandHandler<InviteUser>
{
    private readonly IUserRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<InviteUserHandler> _logger;

    public InviteUserHandler(
        IUserRepository repository,
        ITenantProvider tenantProvider,
        IEmailSender emailSender,
        ILogger<InviteUserHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task HandleAsync(InviteUser command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Invite User");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Inviting user {Email} | CorrelationId: {CorrelationId}", command.Email, correlationId);

        var existingUser = await _repository.GetByEmailAsync(command.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException($"A user with email '{command.Email}' already exists.", "user_email_conflict");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            EntraObjectId = string.Empty,
            Email = command.Email,
            DisplayName = command.DisplayName,
            Role = command.Role,
            IsActive = false,
            TenantId = _tenantProvider.GetTenantId()
        };

        await _repository.AddAsync(user, cancellationToken);

        await _emailSender.SendAsync(
            command.Email,
            "You have been invited to ITDG CRM",
            $"Hello {command.DisplayName}, you have been invited to join the ITDG CRM platform. Please complete your registration to get started.",
            cancellationToken);

        _logger.LogInformation("User {UserId} invited successfully | CorrelationId: {CorrelationId}", user.Id, correlationId);
    }
}
