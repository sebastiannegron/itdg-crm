namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public static class PortalEndpoints
{
    public static RouteGroupBuilder MapPortalEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Portal");
        group.WithTags("Portal");

        group.MapGet("/Messages", GetPortalMessagesEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.ClientPortal)
            .WithName("GetPortalMessages")
            .Produces<IEnumerable<MessageDto>>(StatusCodes.Status200OK);

        group.MapPost("/Messages", SendPortalMessageEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.ClientPortal)
            .WithName("SendPortalMessage")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/Messages/{message_id}/Read", MarkMessageAsReadEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.ClientPortal)
            .WithName("MarkMessageAsRead")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/Documents", GetPortalDocumentsEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.ClientPortal)
            .WithName("GetPortalDocuments")
            .Produces<PaginatedResultDto<DocumentDto>>(StatusCodes.Status200OK);

        group.MapPost("/Documents", UploadPortalDocumentEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.ClientPortal)
            .WithName("UploadPortalDocument")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> GetPortalMessagesEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetPortalMessages, IEnumerable<MessageDto>> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var clientId = GetClientIdFromClaims(httpContext);
            var result = await handler.HandleAsync(new GetPortalMessages(clientId), Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
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
        IValidator<SendPortalMessageRequest> validator,
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

            var clientId = GetClientIdFromClaims(httpContext);
            var senderId = clientId; // Portal messages: client is the sender
            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";

            var command = new SendPortalMessage(clientId, senderId, request.Subject, request.Body);
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);

            return Results.Created();
        }
        catch (Exception ex)
        {
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
        CancellationToken cancellationToken)
    {
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

    private static async Task<IResult> GetPortalDocumentsEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetPortalDocuments, PaginatedResultDto<DocumentDto>> handler,
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
            var clientId = GetClientIdFromClaims(httpContext);
            var query = new GetPortalDocuments(clientId, page, pageSize, categoryId, year, search);
            var result = await handler.HandleAsync(query, Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (ForbiddenException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_portal_documents_failed" } });
        }
    }

    private static async Task<IResult> UploadPortalDocumentEndpoint(
        HttpContext httpContext,
        ICommandHandler<UploadPortalDocument> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var clientId = GetClientIdFromClaims(httpContext);

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

            using var stream = file.OpenReadStream();
            var command = new UploadPortalDocument(
                ClientId: clientId,
                CategoryId: categoryId,
                FileName: file.FileName,
                ContentStream: stream,
                ContentType: file.ContentType,
                FileSize: file.Length
            );

            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);
            return Results.Created();
        }
        catch (ForbiddenException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status403Forbidden,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
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
                extensions: new Dictionary<string, object?> { { "errorCode", "upload_portal_document_failed" } });
        }
    }
}
