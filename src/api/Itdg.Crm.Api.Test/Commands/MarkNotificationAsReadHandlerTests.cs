namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class MarkNotificationAsReadHandlerTests
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<MarkNotificationAsReadHandler> _logger;
    private readonly MarkNotificationAsReadHandler _handler;

    public MarkNotificationAsReadHandlerTests()
    {
        _repository = Substitute.For<INotificationRepository>();
        _logger = Substitute.For<ILogger<MarkNotificationAsReadHandler>>();
        _handler = new MarkNotificationAsReadHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_MarksNotificationAsRead_WhenNotificationExists()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var command = new MarkNotificationAsRead(notificationId);

        var notification = new Notification
        {
            Id = notificationId,
            UserId = Guid.NewGuid(),
            EventType = NotificationEventType.DocumentUploaded,
            Channel = NotificationChannel.InApp,
            Title = "Test",
            Body = "Test body",
            Status = NotificationStatus.Delivered,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns(notification);
        _repository.UpdateAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        notification.Status.Should().Be(NotificationStatus.Read);
        notification.ReadAt.Should().NotBeNull();
        await _repository.Received(1).UpdateAsync(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenNotificationDoesNotExist()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var command = new MarkNotificationAsRead(notificationId);

        _repository.GetByIdAsync(notificationId, Arg.Any<CancellationToken>())
            .Returns((Notification?)null);

        // Act & Assert
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
