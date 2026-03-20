namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class MarkNotificationAsReadHandler : ICommandHandler<MarkNotificationAsRead>
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<MarkNotificationAsReadHandler> _logger;

    public MarkNotificationAsReadHandler(
        INotificationRepository repository,
        ILogger<MarkNotificationAsReadHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(MarkNotificationAsRead command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Mark Notification As Read");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("NotificationId", command.NotificationId);

        _logger.LogInformation("Marking notification {NotificationId} as read | CorrelationId: {CorrelationId}", command.NotificationId, correlationId);

        var notification = await _repository.GetByIdAsync(command.NotificationId, cancellationToken);

        if (notification is null)
        {
            throw new NotFoundException("Notification", command.NotificationId);
        }

        notification.Status = NotificationStatus.Read;
        notification.ReadAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(notification, cancellationToken);

        _logger.LogInformation("Notification {NotificationId} marked as read | CorrelationId: {CorrelationId}", command.NotificationId, correlationId);
    }
}
