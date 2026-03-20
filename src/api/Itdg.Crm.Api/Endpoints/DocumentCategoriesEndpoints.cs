namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public static class DocumentCategoriesEndpoints
{
    public static RouteGroupBuilder MapDocumentCategoriesEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/DocumentCategories");
        group.WithTags("DocumentCategories");

        group.MapGet("", GetDocumentCategoriesEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetDocumentCategories")
            .Produces<IEnumerable<DocumentCategoryDto>>(StatusCodes.Status200OK);

        group.MapPost("", CreateDocumentCategoryEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("CreateDocumentCategory")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{category_id:guid}", UpdateDocumentCategoryEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("UpdateDocumentCategory")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{category_id:guid}", DeleteDocumentCategoryEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("DeleteDocumentCategory")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/Reorder", ReorderDocumentCategoriesEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("ReorderDocumentCategories")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> GetDocumentCategoriesEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetDocumentCategories, IEnumerable<DocumentCategoryDto>> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetDocumentCategories(), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_document_categories_failed" } });
        }
    }

    private static async Task<IResult> CreateDocumentCategoryEndpoint(
        CreateDocumentCategoryRequest request,
        HttpContext httpContext,
        ICommandHandler<CreateDocumentCategory> handler,
        IValidator<CreateDocumentCategoryRequest> validator,
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

            var command = new CreateDocumentCategory(
                request.Name,
                request.NamingConvention,
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
                extensions: new Dictionary<string, object?> { { "errorCode", "create_document_category_failed" } });
        }
    }

    private static async Task<IResult> UpdateDocumentCategoryEndpoint(
        Guid category_id,
        UpdateDocumentCategoryRequest request,
        HttpContext httpContext,
        ICommandHandler<UpdateDocumentCategory> handler,
        IValidator<UpdateDocumentCategoryRequest> validator,
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

            var command = new UpdateDocumentCategory(
                category_id,
                request.Name,
                request.NamingConvention,
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
                extensions: new Dictionary<string, object?> { { "errorCode", "update_document_category_failed" } });
        }
    }

    private static async Task<IResult> DeleteDocumentCategoryEndpoint(
        Guid category_id,
        HttpContext httpContext,
        ICommandHandler<DeleteDocumentCategory> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var command = new DeleteDocumentCategory(category_id);

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
                extensions: new Dictionary<string, object?> { { "errorCode", "delete_document_category_failed" } });
        }
    }

    private static async Task<IResult> ReorderDocumentCategoriesEndpoint(
        ReorderDocumentCategoriesRequest request,
        HttpContext httpContext,
        ICommandHandler<ReorderDocumentCategories> handler,
        IValidator<ReorderDocumentCategoriesRequest> validator,
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

            var command = new ReorderDocumentCategories(
                request.Items.Select(i => new ReorderItem(i.CategoryId, i.SortOrder)).ToList()
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
                extensions: new Dictionary<string, object?> { { "errorCode", "reorder_document_categories_failed" } });
        }
    }
}
