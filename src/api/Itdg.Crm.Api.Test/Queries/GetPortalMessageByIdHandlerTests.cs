namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetPortalMessageByIdHandlerTests
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<GetPortalMessageByIdHandler> _logger;
    private readonly GetPortalMessageByIdHandler _handler;

    public GetPortalMessageByIdHandlerTests()
    {
        _repository = Substitute.For<IMessageRepository>();
        _logger = Substitute.For<ILogger<GetPortalMessageByIdHandler>>();
        _handler = new GetPortalMessageByIdHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsMessage_WhenFound()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var query = new GetPortalMessageById(messageId, clientId);

        var message = new Message
        {
            Id = messageId,
            ClientId = clientId,
            SenderId = Guid.NewGuid(),
            Direction = MessageDirection.Inbound,
            Subject = "Found Message",
            Body = "Found Body",
            IsPortalMessage = true,
            IsRead = false,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _repository.GetByIdAndClientIdAsync(messageId, clientId, Arg.Any<CancellationToken>())
            .Returns(message);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(messageId);
        result.ClientId.Should().Be(clientId);
        result.Subject.Should().Be("Found Message");
        result.Direction.Should().Be("Inbound");
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenMessageDoesNotExist()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var query = new GetPortalMessageById(messageId, clientId);

        _repository.GetByIdAndClientIdAsync(messageId, clientId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        // Act & Assert
        var act = () => _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenMessageBelongsToOtherClient()
    {
        // Arrange
        var messageId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var query = new GetPortalMessageById(messageId, clientId);

        // Repository returns null because clientId doesn't match
        _repository.GetByIdAndClientIdAsync(messageId, clientId, Arg.Any<CancellationToken>())
            .Returns((Message?)null);

        // Act & Assert
        var act = () => _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
