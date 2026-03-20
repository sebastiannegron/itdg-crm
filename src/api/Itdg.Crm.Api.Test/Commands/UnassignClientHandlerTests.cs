namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UnassignClientHandlerTests
{
    private readonly IClientAssignmentRepository _repository;
    private readonly ILogger<UnassignClientHandler> _logger;
    private readonly UnassignClientHandler _handler;

    public UnassignClientHandlerTests()
    {
        _repository = Substitute.For<IClientAssignmentRepository>();
        _logger = Substitute.For<ILogger<UnassignClientHandler>>();
        _handler = new UnassignClientHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_DeletesAssignment_WhenAssignmentExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingAssignment = new ClientAssignment
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            UserId = userId,
            AssignedAt = DateTimeOffset.UtcNow,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByClientAndUserAsync(clientId, userId, Arg.Any<CancellationToken>())
            .Returns(existingAssignment);

        var command = new UnassignClient(ClientId: clientId, UserId: userId);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).DeleteAsync(existingAssignment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenAssignmentDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _repository.GetByClientAndUserAsync(clientId, userId, Arg.Any<CancellationToken>())
            .Returns((ClientAssignment?)null);

        var command = new UnassignClient(ClientId: clientId, UserId: userId);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{clientId}/{userId}*");
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<ClientAssignment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PassesCorrectParametersToRepository()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingAssignment = new ClientAssignment
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            UserId = userId,
            AssignedAt = DateTimeOffset.UtcNow,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByClientAndUserAsync(clientId, userId, Arg.Any<CancellationToken>())
            .Returns(existingAssignment);

        var command = new UnassignClient(ClientId: clientId, UserId: userId);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).GetByClientAndUserAsync(clientId, userId, Arg.Any<CancellationToken>());
    }
}
