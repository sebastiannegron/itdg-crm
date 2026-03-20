namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class InviteUserHandlerTests
{
    private readonly IUserRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<InviteUserHandler> _logger;
    private readonly InviteUserHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public InviteUserHandlerTests()
    {
        _repository = Substitute.For<IUserRepository>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Substitute.For<ILogger<InviteUserHandler>>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new InviteUserHandler(_repository, _tenantProvider, _emailSender, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesUserAndSendsEmail_WhenEmailDoesNotExist()
    {
        // Arrange
        var command = new InviteUser(
            Email: "newuser@example.com",
            DisplayName: "New User",
            Role: UserRole.Associate
        );

        _repository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        User? capturedUser = null;
        await _repository.AddAsync(Arg.Do<User>(u => capturedUser = u), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be("newuser@example.com");
        capturedUser.DisplayName.Should().Be("New User");
        capturedUser.Role.Should().Be(UserRole.Associate);
        capturedUser.IsActive.Should().BeFalse();
        capturedUser.EntraObjectId.Should().BeEmpty();
        capturedUser.TenantId.Should().Be(_tenantId);
        capturedUser.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_SendsInvitationEmail()
    {
        // Arrange
        var command = new InviteUser(
            Email: "newuser@example.com",
            DisplayName: "New User",
            Role: UserRole.Associate
        );

        _repository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _emailSender.Received(1).SendAsync(
            "newuser@example.com",
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("New User")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflictException_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            EntraObjectId = "entra-123",
            Email = "existing@example.com",
            DisplayName = "Existing User",
            Role = UserRole.Associate,
            TenantId = _tenantId
        };

        _repository.GetByEmailAsync("existing@example.com", Arg.Any<CancellationToken>()).Returns(existingUser);

        var command = new InviteUser(
            Email: "existing@example.com",
            DisplayName: "Another User",
            Role: UserRole.Associate
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*existing@example.com*");
    }

    [Fact]
    public async Task HandleAsync_SetsTenantId_FromTenantProvider()
    {
        // Arrange
        var command = new InviteUser(
            Email: "newuser@example.com",
            DisplayName: "New User",
            Role: UserRole.Administrator
        );

        _repository.GetByEmailAsync(command.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        User? capturedUser = null;
        await _repository.AddAsync(Arg.Do<User>(u => capturedUser = u), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.TenantId.Should().Be(_tenantId);
    }
}
