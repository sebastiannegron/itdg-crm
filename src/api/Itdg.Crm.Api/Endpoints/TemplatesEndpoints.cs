namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Requests;

public static class TemplatesEndpoints
{
    public static RouteGroupBuilder MapTemplatesEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Templates");
        group.WithTags("Templates");

        group.MapGet("", GetTemplatesEndpoint)
            .RequireAuthorization(policy => policy.RequireRole("Templates.Read", "Templates.ReadWrite"))
            .WithName("GetTemplates")
            .Produces<IEnumerable<CommunicationTemplateDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetTemplateByIdEndpoint)
            .RequireAuthorization(policy => policy.RequireRole("Templates.Read", "Templates.ReadWrite"))
            .WithName("GetTemplateById")
            .Produces<CommunicationTemplateDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("", CreateTemplateEndpoint)
            .RequireAuthorization(policy => policy.RequireRole("Templates.ReadWrite"))
            .WithName("CreateTemplate")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateTemplateEndpoint)
            .RequireAuthorization(policy => policy.RequireRole("Templates.ReadWrite"))
            .WithName("UpdateTemplate")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{id:guid}", RetireTemplateEndpoint)
            .RequireAuthorization(policy => policy.RequireRole("Templates.ReadWrite"))
            .WithName("RetireTemplate")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetTemplatesEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetTemplates, IEnumerable<CommunicationTemplateDto>> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetTemplates(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_templates_failed" } });
        }
    }

    private static async Task<IResult> GetTemplateByIdEndpoint(
        Guid id,
        HttpContext httpContext,
        IQueryHandler<GetTemplateById, CommunicationTemplateDto> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetTemplateById(id), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
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
                extensions: new Dictionary<string, object?> { { "errorCode", "get_template_failed" } });
        }
    }

    private static async Task<IResult> CreateTemplateEndpoint(
        CreateTemplateRequest request,
        HttpContext httpContext,
        ICommandHandler<CreateTemplate> handler,
        IValidator<CreateTemplateRequest> validator,
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

            var command = new CreateTemplate(
                request.Category,
                request.Name,
                request.SubjectTemplate,
                request.BodyTemplate,
                request.Language,
                Guid.Empty // CreatedById will be resolved from claims in future iterations
            );

            await handler.HandleAsync(command, request.Language, Guid.Parse(correlationId!), cancellationToken);
            return Results.Created();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "create_template_failed" } });
        }
    }

    private static async Task<IResult> UpdateTemplateEndpoint(
        Guid id,
        UpdateTemplateRequest request,
        HttpContext httpContext,
        ICommandHandler<UpdateTemplate> handler,
        IValidator<UpdateTemplateRequest> validator,
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

            var command = new UpdateTemplate(
                id,
                request.Category,
                request.Name,
                request.SubjectTemplate,
                request.BodyTemplate,
                request.Language
            );

            await handler.HandleAsync(command, request.Language, Guid.Parse(correlationId!), cancellationToken);
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
                extensions: new Dictionary<string, object?> { { "errorCode", "update_template_failed" } });
        }
    }

    private static async Task<IResult> RetireTemplateEndpoint(
        Guid id,
        HttpContext httpContext,
        ICommandHandler<RetireTemplate> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var command = new RetireTemplate(id);
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
                extensions: new Dictionary<string, object?> { { "errorCode", "retire_template_failed" } });
        }
    }
}
