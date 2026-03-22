namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Requests;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Dashboard");
        group.WithTags("Dashboard");

        group.MapGet("/Summary", GetDashboardSummaryEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetDashboardSummary")
            .Produces<DashboardSummaryDto>(StatusCodes.Status200OK);

        group.MapGet("/Layout", GetDashboardLayoutEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetDashboardLayout")
            .Produces<DashboardLayoutDto>(StatusCodes.Status200OK);

        group.MapPut("/Layout", SaveDashboardLayoutEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("SaveDashboardLayout")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        group.MapGet("/Calendar", GetDashboardCalendarEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetDashboardCalendar")
            .Produces<DashboardCalendarDto>(StatusCodes.Status200OK);

        return group;
    }

    private static async Task<IResult> GetDashboardSummaryEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetDashboardSummary, DashboardSummaryDto> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetDashboardSummary(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_dashboard_summary_failed" } });
        }
    }

    private static async Task<IResult> GetDashboardLayoutEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetDashboardLayout, DashboardLayoutDto?> handler,
        ICurrentUserProvider currentUserProvider,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var entraObjectId = currentUserProvider.GetEntraObjectId();
            if (string.IsNullOrWhiteSpace(entraObjectId))
            {
                return Results.Problem(
                    detail: "Unable to resolve current user.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    extensions: new Dictionary<string, object?> { { "errorCode", "user_not_found" } });
            }

            var user = await userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
            if (user is null)
            {
                return Results.Problem(
                    detail: "User not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    extensions: new Dictionary<string, object?> { { "errorCode", "user_not_found" } });
            }

            var result = await handler.HandleAsync(new GetDashboardLayout(user.Id), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_dashboard_layout_failed" } });
        }
    }

    private static async Task<IResult> SaveDashboardLayoutEndpoint(
        SaveDashboardLayoutRequest request,
        HttpContext httpContext,
        ICommandHandler<SaveDashboardLayout> handler,
        IValidator<SaveDashboardLayoutRequest> validator,
        ICurrentUserProvider currentUserProvider,
        IUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var entraObjectId = currentUserProvider.GetEntraObjectId();
            if (string.IsNullOrWhiteSpace(entraObjectId))
            {
                return Results.Problem(
                    detail: "Unable to resolve current user.",
                    statusCode: StatusCodes.Status401Unauthorized,
                    extensions: new Dictionary<string, object?> { { "errorCode", "user_not_found" } });
            }

            var user = await userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
            if (user is null)
            {
                return Results.Problem(
                    detail: "User not found.",
                    statusCode: StatusCodes.Status404NotFound,
                    extensions: new Dictionary<string, object?> { { "errorCode", "user_not_found" } });
            }

            var command = new SaveDashboardLayout(user.Id, request.WidgetConfigurations);
            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "save_dashboard_layout_failed" } });
        }
    }

    private static async Task<IResult> GetDashboardCalendarEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetDashboardCalendar, DashboardCalendarDto> handler,
        DateTimeOffset? start_date,
        DateTimeOffset? end_date,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var startDate = start_date ?? DateTimeOffset.UtcNow.Date;
            var endDate = end_date ?? startDate.AddDays(30);

            var result = await handler.HandleAsync(
                new GetDashboardCalendar(startDate, endDate),
                Guid.Parse(correlationId!),
                cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_dashboard_calendar_failed" } });
        }
    }
}
