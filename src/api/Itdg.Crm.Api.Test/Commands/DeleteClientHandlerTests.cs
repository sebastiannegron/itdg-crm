namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DeleteClientHandlerTests
{
    private readonly IClientRepository _repository;
    private readonly ILogger<DeleteClientHandler> _logger;
    private readonly DeleteClientHandler _handler;

    public DeleteClientHandlerTests()
    {
        _repository = Substitute.For<IClientRepository>();
        _logger = Substitute.For<ILogger<DeleteClientHandler>>();
        _handler = new DeleteClientHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_SoftDeletesClient_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var existingClient = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid(),
            DeletedAt = null
        };

        _repository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns(existingClient);

        var command = new DeleteClient(ClientId: clientId);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Is<Client>(c =>
            c.Id == clientId &&
            c.DeletedAt != null
        ), Arg.Any<CancellationToken>());
        existingClient.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _repository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns((Client?)null);

        var command = new DeleteClient(ClientId: clientId);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{clientId}*");
    }

    [Fact]
    public async Task HandleAsync_UsesUpdateNotDelete_ForSoftDelete()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var existingClient = new Client
        {
            Id = clientId,
            Name = "Test Client",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns(existingClient);

        var command = new DeleteClient(ClientId: clientId);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Any<Client>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Client>(), Arg.Any<CancellationToken>());
    }
}
