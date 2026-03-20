namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Requests;

public static class MessagesEndpoints
{
    public static RouteGroupBuilder MapMessagesEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Messages");
        group.WithTags("Messages");

        group.MapPost("/SendTemplate", SendTemplateMessageEndpoint)
            .RequireAuthorization(policy => policy.RequireRole("Messages.ReadWrite"))
            .WithName("SendTemplateMessage")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> SendTemplateMessageEndpoint(
        SendTemplateMessageRequest request,
        HttpContext httpContext,
        ICommandHandler<SendTemplateMessage> handler,
        IValidator<SendTemplateMessageRequest> validator,
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

            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";

            var command = new SendTemplateMessage(
                request.TemplateId,
                request.ClientId,
                Guid.Empty, // SenderId will be resolved from claims in future iterations
                request.MergeFields,
                request.SendViaPortal,
                request.SendViaEmail,
                request.RecipientEmail);

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
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "send_template_message_failed" } });
        }
    }
}
