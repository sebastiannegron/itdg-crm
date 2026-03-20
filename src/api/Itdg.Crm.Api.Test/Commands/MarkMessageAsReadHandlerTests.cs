namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class MarkMessageAsReadHandlerTests
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<MarkMessageAsReadHandler> _logger;
    private readonly MarkMessageAsReadHandler _handler;

    public MarkMessageAsReadHandlerTests()
    {
        _repository = Substitute.For<IMessageRepository>();
        _logger = Substitute.For<ILogger<MarkMessageAsReadHandler>>();
        _handler = new MarkMessageAsReadHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_MarksMessageAsRead_WhenMessageExists()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var command = new MarkMessageAsRead(messageId, clientId);

        var message = new Message
        {
            Id = messageId,
            ClientId = clientId,
            SenderId = Guid.NewGuid(),
            Direction = MessageDirection.Outbound,
            Subject = "Test",
            Body = "Test body",
            IsPortalMessage = true,
            IsRead = false,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAndClientIdAsync(messageId, clientId, Arg.Any<CancellationToken>())
            .Returns(message);
        _repository.UpdateAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        message.IsRead.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(message, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenMessageDoesNotExist()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var command = new MarkMessageAsRead(messageId, clientId);

        _repository.GetByIdAndClientIdAsync(messageId, clientId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        // Act & Assert
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenMessageBelongsToOtherClient()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var command = new MarkMessageAsRead(messageId, clientId);

        // Repository returns null because the message doesn't match the clientId
        _repository.GetByIdAndClientIdAsync(messageId, clientId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        // Act & Assert
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
