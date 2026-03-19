namespace Itdg.Crm.Api.Endpoints;

using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Requests;

public static class PortalEndpoints
{
    public static RouteGroupBuilder MapPortalEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Portal");
        group.WithTags("Portal");

        group.MapGet("/Messages", GetPortalMessagesEndpoint)
            .RequireAuthorization()
            .WithName("GetPortalMessages")
            .Produces<IEnumerable<MessageDto>>(StatusCodes.Status200OK);

        group.MapPost("/Messages", SendPortalMessageEndpoint)
            .RequireAuthorization()
            .WithName("SendPortalMessage")
            .Produces(StatusCodes.Status201Created);

        group.MapPut("/Messages/{message_id}/Read", MarkMessageAsReadEndpoint)
            .RequireAuthorization()
            .WithName("MarkMessageAsRead")
            .Produces(StatusCodes.Status204NoContent);

        return group;
    }

    private static async Task<IResult> GetPortalMessagesEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetPortalMessages, IEnumerable<MessageDto>> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(PortalEndpoints));
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var clientId = GetClientIdFromClaims(httpContext);
            var result = await handler.HandleAsync(new GetPortalMessages(clientId), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Portal messages not found | CorrelationId: {CorrelationId}", correlationId);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get portal messages | CorrelationId: {CorrelationId}", correlationId);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_portal_messages_failed" } });
        }
    }

    private static async Task<IResult> SendPortalMessageEndpoint(
        HttpContext httpContext,
        SendPortalMessageRequest request,
        ICommandHandler<SendPortalMessage> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(PortalEndpoints));
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var clientId = GetClientIdFromClaims(httpContext);
            var senderId = clientId;
            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";

            var command = new SendPortalMessage(clientId, senderId, request.Subject, request.Body);
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);

            return Results.Created();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send portal message | CorrelationId: {CorrelationId}", correlationId);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "send_portal_message_failed" } });
        }
    }

    private static async Task<IResult> MarkMessageAsReadEndpoint(
        HttpContext httpContext,
        Guid message_id,
        ICommandHandler<MarkMessageAsRead> handler,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(PortalEndpoints));
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var clientId = GetClientIdFromClaims(httpContext);
            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";

            var command = new MarkMessageAsRead(message_id, clientId);
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);

            return Results.NoContent();
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Message not found for mark as read | CorrelationId: {CorrelationId}", correlationId);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark message as read | CorrelationId: {CorrelationId}", correlationId);
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "mark_message_read_failed" } });
        }
    }

    private static Guid GetClientIdFromClaims(HttpContext httpContext)
    {
        var clientIdClaim = httpContext.User.FindFirst("client_id")?.Value;
        if (string.IsNullOrEmpty(clientIdClaim) || !Guid.TryParse(clientIdClaim, out var clientId))
        {
            throw new ForbiddenException("Client identity could not be resolved from the request.");
        }
        return clientId;
    }
}
