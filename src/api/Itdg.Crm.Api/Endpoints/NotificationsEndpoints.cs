namespace Itdg.Crm.Api.Endpoints;

using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public static class NotificationsEndpoints
{
    public static RouteGroupBuilder MapNotificationsEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Notifications");
        group.WithTags("Notifications");

        group.MapGet("", GetNotificationsEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetNotifications")
            .Produces<PaginatedResultDto<NotificationDto>>(StatusCodes.Status200OK);

        group.MapGet("/UnreadCount", GetUnreadNotificationCountEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetUnreadNotificationCount")
            .Produces<int>(StatusCodes.Status200OK);

        group.MapPut("/{notification_id:guid}/Read", MarkNotificationAsReadEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("MarkNotificationAsRead")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/ReadAll", MarkAllNotificationsAsReadEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("MarkAllNotificationsAsRead")
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/Preferences", GetNotificationPreferencesEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetNotificationPreferences")
            .Produces<IEnumerable<NotificationPreferenceDto>>(StatusCodes.Status200OK);

        group.MapPut("/Preferences", UpdateNotificationPreferencesEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("UpdateNotificationPreferences")
            .Produces(StatusCodes.Status204NoContent);

        return group;
    }

    private static async Task<IResult> GetNotificationsEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetNotifications, PaginatedResultDto<NotificationDto>> handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        NotificationStatus? status = null)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var query = new GetNotifications(page, pageSize, status);
            var result = await handler.HandleAsync(query, Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_notifications_failed" } });
        }
    }

    private static async Task<IResult> GetUnreadNotificationCountEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetUnreadNotificationCount, int> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetUnreadNotificationCount(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_unread_notification_count_failed" } });
        }
    }

    private static async Task<IResult> MarkNotificationAsReadEndpoint(
        Guid notification_id,
        HttpContext httpContext,
        ICommandHandler<MarkNotificationAsRead> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var command = new MarkNotificationAsRead(notification_id);
            await handler.HandleAsync(command, string.Empty, Guid.Parse(correlationId!), cancellationToken);
            return Results.NoContent();
        }
        catch (NotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "mark_notification_as_read_failed" } });
        }
    }

    private static async Task<IResult> MarkAllNotificationsAsReadEndpoint(
        HttpContext httpContext,
        ICommandHandler<MarkAllNotificationsAsRead> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var command = new MarkAllNotificationsAsRead();
            await handler.HandleAsync(command, string.Empty, Guid.Parse(correlationId!), cancellationToken);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "mark_all_notifications_as_read_failed" } });
        }
    }

    private static async Task<IResult> GetNotificationPreferencesEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetNotificationPreferences, IEnumerable<NotificationPreferenceDto>> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetNotificationPreferences(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_notification_preferences_failed" } });
        }
    }

    private static async Task<IResult> UpdateNotificationPreferencesEndpoint(
        UpdateNotificationPreferencesRequest request,
        HttpContext httpContext,
        ICommandHandler<UpdateNotificationPreferences> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var preferences = request.Preferences.Select(p => new NotificationPreferenceDto(
                PreferenceId: Guid.Empty,
                EventType: p.EventType,
                Channel: p.Channel,
                IsEnabled: p.IsEnabled,
                DigestMode: p.DigestMode
            )).ToList();

            var command = new UpdateNotificationPreferences(preferences);
            await handler.HandleAsync(command, string.Empty, Guid.Parse(correlationId!), cancellationToken);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "update_notification_preferences_failed" } });
        }
    }
}
