namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateUserHandlerTests
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UpdateUserHandler> _logger;
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _repository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<UpdateUserHandler>>();
        _handler = new UpdateUserHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_UpdatesUser_WithCorrectRoleAndStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            EntraObjectId = "entra-123",
            Email = "user@example.com",
            DisplayName = "Test User",
            Role = UserRole.Associate,
            IsActive = true,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var command = new UpdateUser(userId, UserRole.Administrator, false);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Is<User>(u =>
            u.Role == UserRole.Administrator && u.IsActive == false), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var command = new UpdateUser(userId, UserRole.Administrator, true);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{userId}*");
    }
}
