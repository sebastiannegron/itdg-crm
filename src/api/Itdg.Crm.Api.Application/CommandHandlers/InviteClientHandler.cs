namespace Itdg.Crm.Api.Application.CommandHandlers;

using System.Security.Cryptography;
using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class InviteClientHandler : ICommandHandler<InviteClient>
{
    private readonly IClientRepository _clientRepository;
    private readonly IGenericRepository<ClientPortalInvitation> _invitationRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmailSender _emailSender;
    private readonly IPortalConfiguration _portalConfiguration;
    private readonly ILogger<InviteClientHandler> _logger;

    public InviteClientHandler(
        IClientRepository clientRepository,
        IGenericRepository<ClientPortalInvitation> invitationRepository,
        ITenantProvider tenantProvider,
        IEmailSender emailSender,
        IPortalConfiguration portalConfiguration,
        ILogger<InviteClientHandler> logger)
    {
        _clientRepository = clientRepository;
        _invitationRepository = invitationRepository;
        _tenantProvider = tenantProvider;
        _emailSender = emailSender;
        _portalConfiguration = portalConfiguration;
        _logger = logger;
    }

    public async Task HandleAsync(InviteClient command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Invite Client");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Inviting client {ClientId} via email {Email} | CorrelationId: {CorrelationId}",
            command.ClientId, command.Email, correlationId);

        var client = await _clientRepository.GetByIdAsync(command.ClientId, cancellationToken)
            ?? throw new NotFoundException("Client", command.ClientId);

        string token = GenerateInvitationToken();
        int expiryDays = _portalConfiguration.GetInvitationExpiryDays();

        var invitation = new ClientPortalInvitation
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            Email = command.Email,
            Token = token,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(expiryDays),
            TenantId = _tenantProvider.GetTenantId()
        };

        await _invitationRepository.AddAsync(invitation, cancellationToken);

        string baseUrl = _portalConfiguration.GetBaseUrl();
        string invitationLink = $"{baseUrl}/invite?token={token}";

        await _emailSender.SendAsync(
            command.Email,
            "You have been invited to the ITDG Client Portal",
            $"Hello, you have been invited to access the ITDG Client Portal for {client.Name}. Please click the following link to complete your registration: {invitationLink}",
            cancellationToken);

        _logger.LogInformation("Client invitation {InvitationId} created for client {ClientId} | CorrelationId: {CorrelationId}",
            invitation.Id, command.ClientId, correlationId);
    }

    private static string GenerateInvitationToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
