namespace Itdg.Crm.Api.Endpoints;

using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;

public static class IntegrationsEndpoints
{
    public static RouteGroupBuilder MapIntegrationsEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Integrations");
        group.WithTags("Integrations");

        group.MapGet("/Google/Auth", GetGoogleAuthEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("GetGoogleAuth")
            .Produces(StatusCodes.Status302Found)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/Google/Callback", GoogleCallbackEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("GoogleCallback")
            .Produces(StatusCodes.Status302Found)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/Google/Status", GetGoogleStatusEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("GetGoogleStatus")
            .Produces<GoogleConnectionStatusDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/Google", DisconnectGoogleEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("DisconnectGoogle")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/Gmail/Auth", GetGmailAuthEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetGmailAuth")
            .Produces(StatusCodes.Status302Found)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/Gmail/Callback", GmailCallbackEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GmailCallback")
            .Produces(StatusCodes.Status302Found)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("/Gmail/Status", GetGmailStatusEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetGmailStatus")
            .Produces<GmailConnectionStatusDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("/Gmail", DisconnectGmailEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("DisconnectGmail")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<IResult> GetGoogleAuthEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetGoogleAuthUrl, string> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var url = await handler.HandleAsync(new GetGoogleAuthUrl(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Redirect(url);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "google_auth_failed" } });
        }
    }

    private static async Task<IResult> GoogleCallbackEndpoint(
        HttpContext httpContext,
        ICommandHandler<HandleGoogleCallback> handler,
        string? code,
        string? error,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                return Results.Problem(
                    detail: $"Google OAuth error: {error}",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { { "errorCode", "google_oauth_denied" } });
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Results.Problem(
                    detail: "Authorization code is required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { { "errorCode", "google_callback_missing_code" } });
            }

            var parsedCorrelationId = correlationId is not null ? Guid.Parse(correlationId) : Guid.NewGuid();

            await handler.HandleAsync(new HandleGoogleCallback(code), "en", parsedCorrelationId, cancellationToken);

            // Redirect back to settings page after successful OAuth
            return Results.Redirect("/settings?google_connected=true");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "google_callback_failed" } });
        }
    }

    private static async Task<IResult> GetGoogleStatusEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetGoogleConnectionStatus, GoogleConnectionStatusDto> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var status = await handler.HandleAsync(new GetGoogleConnectionStatus(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(status);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "google_status_failed" } });
        }
    }

    private static async Task<IResult> DisconnectGoogleEndpoint(
        HttpContext httpContext,
        ICommandHandler<DisconnectGoogle> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            await handler.HandleAsync(new DisconnectGoogle(), "en", Guid.Parse(correlationId!), cancellationToken);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "google_disconnect_failed" } });
        }
    }

    private static async Task<IResult> GetGmailAuthEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetGmailAuthUrl, string> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var url = await handler.HandleAsync(new GetGmailAuthUrl(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Redirect(url);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "gmail_auth_failed" } });
        }
    }

    private static async Task<IResult> GmailCallbackEndpoint(
        HttpContext httpContext,
        ICommandHandler<HandleGmailCallback> handler,
        string? code,
        string? error,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                return Results.Problem(
                    detail: $"Gmail OAuth error: {error}",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { { "errorCode", "gmail_oauth_denied" } });
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                return Results.Problem(
                    detail: "Authorization code is required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { { "errorCode", "gmail_callback_missing_code" } });
            }

            var parsedCorrelationId = correlationId is not null ? Guid.Parse(correlationId) : Guid.NewGuid();

            await handler.HandleAsync(new HandleGmailCallback(code), "en", parsedCorrelationId, cancellationToken);

            // Redirect back to settings page after successful OAuth
            return Results.Redirect("/settings?gmail_connected=true");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "gmail_callback_failed" } });
        }
    }

    private static async Task<IResult> GetGmailStatusEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetGmailConnectionStatus, GmailConnectionStatusDto> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var status = await handler.HandleAsync(new GetGmailConnectionStatus(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(status);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "gmail_status_failed" } });
        }
    }

    private static async Task<IResult> DisconnectGmailEndpoint(
        HttpContext httpContext,
        ICommandHandler<DisconnectGmail> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            await handler.HandleAsync(new DisconnectGmail(), "en", Guid.Parse(correlationId!), cancellationToken);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "gmail_disconnect_failed" } });
        }
    }
}
