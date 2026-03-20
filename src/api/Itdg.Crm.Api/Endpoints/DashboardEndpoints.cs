namespace Itdg.Crm.Api.Endpoints;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;

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
}
