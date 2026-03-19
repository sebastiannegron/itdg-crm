namespace Itdg.Crm.Api.Endpoints;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Health");
        group.WithTags("Health");

        group.MapGet("", GetHealthEndpoint)
            .AllowAnonymous()
            .WithName("GetHealth")
            .Produces<HealthResponse>(StatusCodes.Status200OK);

        return group;
    }

    private static IResult GetHealthEndpoint()
    {
        return Results.Ok(new HealthResponse("Healthy", DateTimeOffset.UtcNow));
    }
}

public record HealthResponse(string Status, DateTimeOffset Timestamp);
