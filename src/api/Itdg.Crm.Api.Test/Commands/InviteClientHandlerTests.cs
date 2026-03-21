namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class InviteClientHandlerTests
{
    private readonly IClientRepository _clientRepository;
    private readonly IGenericRepository<ClientPortalInvitation> _invitationRepository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmailSender _emailSender;
    private readonly IPortalConfiguration _portalConfiguration;
    private readonly ILogger<InviteClientHandler> _logger;
    private readonly InviteClientHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public InviteClientHandlerTests()
    {
        _clientRepository = Substitute.For<IClientRepository>();
        _invitationRepository = Substitute.For<IGenericRepository<ClientPortalInvitation>>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _emailSender = Substitute.For<IEmailSender>();
        _portalConfiguration = Substitute.For<IPortalConfiguration>();
        _logger = Substitute.For<ILogger<InviteClientHandler>>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _portalConfiguration.GetBaseUrl().Returns("https://portal.itdg.com");
        _portalConfiguration.GetInvitationExpiryDays().Returns(7);
        _handler = new InviteClientHandler(
            _clientRepository,
            _invitationRepository,
            _tenantProvider,
            _emailSender,
            _portalConfiguration,
            _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesInvitation_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Status = ClientStatus.Active,
            TenantId = _tenantId
        };
        _clientRepository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        ClientPortalInvitation? captured = null;
        await _invitationRepository.AddAsync(
            Arg.Do<ClientPortalInvitation>(i => captured = i),
            Arg.Any<CancellationToken>());

        var command = new InviteClient(clientId, "client@example.com");

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _invitationRepository.Received(1).AddAsync(
            Arg.Any<ClientPortalInvitation>(),
            Arg.Any<CancellationToken>());
        captured.Should().NotBeNull();
        captured!.ClientId.Should().Be(clientId);
        captured.Email.Should().Be("client@example.com");
        captured.Token.Should().NotBeNullOrEmpty();
        captured.Status.Should().Be(InvitationStatus.Pending);
        captured.TenantId.Should().Be(_tenantId);
        captured.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_GeneratesUniqueToken()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Status = ClientStatus.Active,
            TenantId = _tenantId
        };
        _clientRepository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        var tokens = new List<string>();
        await _invitationRepository.AddAsync(
            Arg.Do<ClientPortalInvitation>(i => tokens.Add(i.Token)),
            Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(new InviteClient(clientId, "a@example.com"), "en-pr", Guid.NewGuid(), CancellationToken.None);
        await _handler.HandleAsync(new InviteClient(clientId, "b@example.com"), "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        tokens.Should().HaveCount(2);
        tokens[0].Should().NotBe(tokens[1]);
    }

    [Fact]
    public async Task HandleAsync_SetsExpirationFromConfiguration()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Status = ClientStatus.Active,
            TenantId = _tenantId
        };
        _clientRepository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        ClientPortalInvitation? captured = null;
        await _invitationRepository.AddAsync(
            Arg.Do<ClientPortalInvitation>(i => captured = i),
            Arg.Any<CancellationToken>());

        var command = new InviteClient(clientId, "client@example.com");

        // Act
        var beforeInvoke = DateTimeOffset.UtcNow;
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);
        var afterInvoke = DateTimeOffset.UtcNow;

        // Assert
        captured.Should().NotBeNull();
        captured!.ExpiresAt.Should().BeAfter(beforeInvoke.AddDays(7).AddSeconds(-1));
        captured.ExpiresAt.Should().BeBefore(afterInvoke.AddDays(7).AddSeconds(1));
    }

    [Fact]
    public async Task HandleAsync_SendsInvitationEmail_WithLink()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Status = ClientStatus.Active,
            TenantId = _tenantId
        };
        _clientRepository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        var command = new InviteClient(clientId, "client@example.com");

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _emailSender.Received(1).SendAsync(
            "client@example.com",
            Arg.Any<string>(),
            Arg.Is<string>(body =>
                body.Contains("https://portal.itdg.com/invite?token=") &&
                body.Contains("Test Client")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _clientRepository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns((Client?)null);

        var command = new InviteClient(clientId, "client@example.com");

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{clientId}*");
    }

    [Fact]
    public async Task HandleAsync_DoesNotSendEmail_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _clientRepository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns((Client?)null);

        var command = new InviteClient(clientId, "client@example.com");

        // Act
        try
        {
            await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);
        }
        catch (NotFoundException)
        {
            // Expected
        }

        // Assert
        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SetsTenantId_FromTenantProvider()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Status = ClientStatus.Active,
            TenantId = _tenantId
        };
        _clientRepository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        ClientPortalInvitation? captured = null;
        await _invitationRepository.AddAsync(
            Arg.Do<ClientPortalInvitation>(i => captured = i),
            Arg.Any<CancellationToken>());

        var command = new InviteClient(clientId, "client@example.com");

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task HandleAsync_GeneratesUrlSafeToken()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Status = ClientStatus.Active,
            TenantId = _tenantId
        };
        _clientRepository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        ClientPortalInvitation? captured = null;
        await _invitationRepository.AddAsync(
            Arg.Do<ClientPortalInvitation>(i => captured = i),
            Arg.Any<CancellationToken>());

        var command = new InviteClient(clientId, "client@example.com");

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.Token.Should().NotContain("+");
        captured.Token.Should().NotContain("/");
        captured.Token.Should().NotContain("=");
    }
}
