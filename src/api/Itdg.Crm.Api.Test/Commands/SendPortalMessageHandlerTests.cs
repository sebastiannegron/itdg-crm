namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class SendPortalMessageHandlerTests
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<SendPortalMessageHandler> _logger;
    private readonly SendPortalMessageHandler _handler;

    public SendPortalMessageHandlerTests()
    {
        _repository = Substitute.For<IMessageRepository>();
        _logger = Substitute.For<ILogger<SendPortalMessageHandler>>();
        _handler = new SendPortalMessageHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesMessage_WithCorrectProperties()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var senderId = clientId;
        var command = new SendPortalMessage(clientId, senderId, "Test Subject", "Test Body");
        Message? capturedMessage = null;

        _repository.AddAsync(Arg.Do<Message>(m => capturedMessage = m), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        capturedMessage.Should().NotBeNull();
        capturedMessage!.ClientId.Should().Be(clientId);
        capturedMessage.SenderId.Should().Be(senderId);
        capturedMessage.Subject.Should().Be("Test Subject");
        capturedMessage.Body.Should().Be("Test Body");
        capturedMessage.Direction.Should().Be(MessageDirection.Inbound);
        capturedMessage.IsPortalMessage.Should().BeTrue();
        capturedMessage.IsRead.Should().BeFalse();
    }
}
