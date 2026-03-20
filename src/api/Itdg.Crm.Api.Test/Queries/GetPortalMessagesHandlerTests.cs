namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetPortalMessagesHandlerTests
{
    private readonly IMessageRepository _repository;
    private readonly ILogger<GetPortalMessagesHandler> _logger;
    private readonly GetPortalMessagesHandler _handler;

    public GetPortalMessagesHandlerTests()
    {
        _repository = Substitute.For<IMessageRepository>();
        _logger = Substitute.For<ILogger<GetPortalMessagesHandler>>();
        _handler = new GetPortalMessagesHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsMessages_ForClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var query = new GetPortalMessages(clientId);

        var messages = new List<Message>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                SenderId = Guid.NewGuid(),
                Direction = MessageDirection.Inbound,
                Subject = "Message 1",
                Body = "Body 1",
                IsPortalMessage = true,
                IsRead = false,
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                SenderId = Guid.NewGuid(),
                Direction = MessageDirection.Outbound,
                Subject = "Message 2",
                Body = "Body 2",
                IsPortalMessage = true,
                IsRead = true,
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow
            }
        };

        _repository.GetByClientIdAsync(clientId, Arg.Any<CancellationToken>())
            .Returns(messages);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dtos = result.ToList();
        dtos.Should().HaveCount(2);
        dtos[0].Subject.Should().Be("Message 1");
        dtos[0].Direction.Should().Be("Inbound");
        dtos[1].Subject.Should().Be("Message 2");
        dtos[1].Direction.Should().Be("Outbound");
        dtos[1].IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmpty_WhenNoMessages()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var query = new GetPortalMessages(clientId);

        _repository.GetByClientIdAsync(clientId, Arg.Any<CancellationToken>())
            .Returns(new List<Message>());

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
