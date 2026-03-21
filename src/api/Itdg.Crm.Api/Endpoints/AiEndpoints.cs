namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public static class AiEndpoints
{
    public static RouteGroupBuilder MapAiEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Ai");
        group.WithTags("Ai");

        group.MapPost("/DraftEmail", DraftEmailEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("DraftEmail")
            .Produces<AiDraftEmailResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }

    private static async Task<IResult> DraftEmailEndpoint(
        DraftEmailRequest request,
        HttpContext httpContext,
        IQueryHandler<DraftEmail, string> handler,
        IValidator<DraftEmailRequest> validator,
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

            var query = new DraftEmail(
                request.ClientName,
                request.Topic,
                request.Language,
                request.AdditionalContext
            );

            var draft = await handler.HandleAsync(query, Guid.Parse(correlationId!), cancellationToken);

            return Results.Ok(new AiDraftEmailResponse(draft));
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "draft_email_failed" } });
        }
    }
}

public record AiDraftEmailResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("draft")] string Draft);
