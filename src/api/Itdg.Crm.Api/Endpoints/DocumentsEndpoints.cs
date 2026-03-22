namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public static class DocumentsEndpoints
{
    public static RouteGroupBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Clients/{client_id:guid}/Documents");
        group.WithTags("Documents");

        group.MapGet("", GetClientDocumentsEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetClientDocuments")
            .Produces<PaginatedResultDto<DocumentDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("", UploadDocumentEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("UploadDocument")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .DisableAntiforgery();

        RouteGroupBuilder documentGroup = builder.MapGroup("/api/v1/Documents");
        documentGroup.WithTags("Documents");

        documentGroup.MapGet("/{document_id:guid}", DownloadDocumentEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("DownloadDocument")
            .Produces<DocumentDownloadDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        documentGroup.MapDelete("/{document_id:guid}", DeleteDocumentEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("DeleteDocument")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        documentGroup.MapGet("/RecycleBin", GetRecycleBinEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("GetRecycleBin")
            .Produces<PaginatedResultDto<RecycleBinDocumentDto>>(StatusCodes.Status200OK);

        documentGroup.MapPost("/{document_id:guid}/Restore", RestoreDocumentEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("RestoreDocument")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        documentGroup.MapGet("/{document_id:guid}/Detail", GetDocumentDetailEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetDocumentDetail")
            .Produces<DocumentDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        documentGroup.MapPost("/{document_id:guid}/Versions", UploadNewVersionEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("UploadNewVersion")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery();

        documentGroup.MapGet("/{document_id:guid}/Audit", GetDocumentAuditTrailEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("GetDocumentAuditTrail")
            .Produces<PaginatedResultDto<AuditLogDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        documentGroup.MapPost("/Search", SearchDocumentsEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("SearchDocuments")
            .Produces<PaginatedResultDto<DocumentSearchResultDto>>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> GetClientDocumentsEndpoint(
        Guid client_id,
        HttpContext httpContext,
        IQueryHandler<GetClientDocuments, PaginatedResultDto<DocumentDto>> handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        Guid? categoryId = null,
        int? year = null,
        string? search = null)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var query = new GetClientDocuments(client_id, page, pageSize, categoryId, year, search);
            var result = await handler.HandleAsync(query, Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (ForbiddenException)
        {
            return Results.Problem(
                detail: "You do not have permission to access documents for this client.",
                statusCode: StatusCodes.Status403Forbidden,
                extensions: new Dictionary<string, object?> { { "errorCode", "forbidden" } });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_client_documents_failed" } });
        }
    }

    private static async Task<IResult> DownloadDocumentEndpoint(
        Guid document_id,
        HttpContext httpContext,
        IQueryHandler<DownloadDocument, DocumentDownloadDto> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new DownloadDocument(document_id), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (NotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (ForbiddenException)
        {
            return Results.Problem(
                detail: "You do not have permission to access this document.",
                statusCode: StatusCodes.Status403Forbidden,
                extensions: new Dictionary<string, object?> { { "errorCode", "forbidden" } });
        }
        catch (DomainException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "download_document_failed" } });
        }
    }

    private static async Task<IResult> UploadDocumentEndpoint(
        Guid client_id,
        HttpContext httpContext,
        ICommandHandler<UploadDocument> handler,
        IValidator<UploadDocumentRequest> validator,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var form = await httpContext.Request.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file");

            if (file is null || file.Length == 0)
            {
                return Results.Problem(
                    detail: "A file is required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { { "errorCode", "file_required" } });
            }

            if (!Guid.TryParse(form["category_id"], out var categoryId))
            {
                return Results.Problem(
                    detail: "A valid category_id is required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { { "errorCode", "invalid_category_id" } });
            }

            var parentFolderId = form["google_drive_parent_folder_id"].FirstOrDefault();

            var request = new UploadDocumentRequest
            {
                CategoryId = categoryId,
                GoogleDriveParentFolderId = parentFolderId
            };

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            using var stream = file.OpenReadStream();
            var command = new UploadDocument(
                ClientId: client_id,
                CategoryId: categoryId,
                FileName: file.FileName,
                ContentStream: stream,
                ContentType: file.ContentType,
                FileSize: file.Length,
                GoogleDriveParentFolderId: parentFolderId
            );

            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);
            return Results.Created();
        }
        catch (NotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (DomainException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "upload_document_failed" } });
        }
    }

    private static async Task<IResult> DeleteDocumentEndpoint(
        Guid document_id,
        HttpContext httpContext,
        ICommandHandler<DeleteDocument> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";
            await handler.HandleAsync(new DeleteDocument(document_id), language, Guid.Parse(correlationId!), cancellationToken);
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
                extensions: new Dictionary<string, object?> { { "errorCode", "delete_document_failed" } });
        }
    }

    private static async Task<IResult> GetRecycleBinEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetRecycleBin, PaginatedResultDto<RecycleBinDocumentDto>> handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var query = new GetRecycleBin(page, pageSize);
            var result = await handler.HandleAsync(query, Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_recycle_bin_failed" } });
        }
    }

    private static async Task<IResult> RestoreDocumentEndpoint(
        Guid document_id,
        HttpContext httpContext,
        ICommandHandler<RestoreDocument> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";
            await handler.HandleAsync(new RestoreDocument(document_id), language, Guid.Parse(correlationId!), cancellationToken);
            return Results.NoContent();
        }
        catch (NotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (DomainException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "restore_document_failed" } });
        }
    }

    private static async Task<IResult> GetDocumentDetailEndpoint(
        Guid document_id,
        HttpContext httpContext,
        IQueryHandler<GetDocumentDetail, DocumentDetailDto> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetDocumentDetail(document_id), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (NotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (ForbiddenException)
        {
            return Results.Problem(
                detail: "You do not have permission to access this document.",
                statusCode: StatusCodes.Status403Forbidden,
                extensions: new Dictionary<string, object?> { { "errorCode", "forbidden" } });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_document_detail_failed" } });
        }
    }

    private static async Task<IResult> UploadNewVersionEndpoint(
        Guid document_id,
        HttpContext httpContext,
        ICommandHandler<UploadNewVersion> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var form = await httpContext.Request.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file");

            if (file is null || file.Length == 0)
            {
                return Results.Problem(
                    detail: "A file is required.",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { { "errorCode", "file_required" } });
            }

            using var stream = file.OpenReadStream();
            var command = new UploadNewVersion(
                DocumentId: document_id,
                FileName: file.FileName,
                ContentStream: stream,
                ContentType: file.ContentType,
                FileSize: file.Length
            );

            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);
            return Results.Created();
        }
        catch (NotFoundException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (DomainException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "upload_new_version_failed" } });
        }
    }

    private static async Task<IResult> GetDocumentAuditTrailEndpoint(
        Guid document_id,
        HttpContext httpContext,
        IQueryHandler<GetDocumentAuditTrail, PaginatedResultDto<AuditLogDto>> handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var query = new GetDocumentAuditTrail(document_id, page, pageSize);
            var result = await handler.HandleAsync(query, Guid.Parse(correlationId!), cancellationToken);
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
                extensions: new Dictionary<string, object?> { { "errorCode", "get_document_audit_trail_failed" } });
        }
    }

    private static async Task<IResult> SearchDocumentsEndpoint(
        HttpContext httpContext,
        IQueryHandler<SearchDocuments, PaginatedResultDto<DocumentSearchResultDto>> handler,
        IValidator<SearchDocumentsRequest> validator,
        SearchDocumentsRequest request,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var query = new SearchDocuments(
                Query: request.Query,
                ClientId: request.ClientId,
                Category: request.Category,
                DateFrom: request.DateFrom,
                DateTo: request.DateTo,
                Page: page,
                PageSize: pageSize);

            var result = await handler.HandleAsync(query, Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "search_documents_failed" } });
        }
    }
}
