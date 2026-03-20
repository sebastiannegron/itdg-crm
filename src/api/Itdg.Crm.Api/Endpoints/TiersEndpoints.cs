namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public static class TiersEndpoints
{
    public static RouteGroupBuilder MapTiersEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Tiers");
        group.WithTags("Tiers");

        group.MapGet("", GetTiersEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetTiers")
            .Produces<IEnumerable<ClientTierDto>>(StatusCodes.Status200OK);

        group.MapPost("", CreateTierEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("CreateTier")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{tier_id:guid}", UpdateTierEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("UpdateTier")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> GetTiersEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetTiers, IEnumerable<ClientTierDto>> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetTiers(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_tiers_failed" } });
        }
    }

    private static async Task<IResult> CreateTierEndpoint(
        CreateTierRequest request,
        HttpContext httpContext,
        ICommandHandler<CreateTier> handler,
        IValidator<CreateTierRequest> validator,
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

            var command = new CreateTier(
                request.Name,
                request.SortOrder
            );

            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);
            return Results.Created();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "create_tier_failed" } });
        }
    }

    private static async Task<IResult> UpdateTierEndpoint(
        Guid tier_id,
        UpdateTierRequest request,
        HttpContext httpContext,
        ICommandHandler<UpdateTier> handler,
        IValidator<UpdateTierRequest> validator,
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

            var command = new UpdateTier(
                tier_id,
                request.Name,
                request.SortOrder
            );

            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);
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
                extensions: new Dictionary<string, object?> { { "errorCode", "update_tier_failed" } });
        }
    }
}
