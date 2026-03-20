namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public static class DocumentsEndpoints
{
    public static RouteGroupBuilder MapDocumentsEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Clients/{client_id:guid}/Documents");
        group.WithTags("Documents");

        group.MapPost("", UploadDocumentEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("UploadDocument")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .DisableAntiforgery();

        return group;
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
}
