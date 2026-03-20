namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public static class ClientsEndpoints
{
    public static RouteGroupBuilder MapClientsEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Clients");
        group.WithTags("Clients");

        group.MapGet("", GetClientsEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Associate)
            .WithName("GetClients")
            .Produces<PaginatedResultDto<ClientDto>>(StatusCodes.Status200OK);

        group.MapGet("/{client_id:guid}", GetClientByIdEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.ClientAssignment)
            .WithName("GetClientById")
            .Produces<ClientDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("", CreateClientEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("CreateClient")
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{client_id:guid}", UpdateClientEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.ClientAssignment)
            .WithName("UpdateClient")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapDelete("/{client_id:guid}", DeleteClientEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("DeleteClient")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetClientsEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetClients, PaginatedResultDto<ClientDto>> handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        ClientStatus? status = null,
        Guid? tierId = null,
        string? search = null)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var query = new GetClients(page, pageSize, status, tierId, search);
            var result = await handler.HandleAsync(query, Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_clients_failed" } });
        }
    }

    private static async Task<IResult> GetClientByIdEndpoint(
        Guid client_id,
        HttpContext httpContext,
        IQueryHandler<GetClientById, ClientDto> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetClientById(client_id), Guid.Parse(correlationId!), cancellationToken);
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
                extensions: new Dictionary<string, object?> { { "errorCode", "get_client_failed" } });
        }
    }

    private static async Task<IResult> CreateClientEndpoint(
        CreateClientRequest request,
        HttpContext httpContext,
        ICommandHandler<CreateClient> handler,
        IValidator<CreateClientRequest> validator,
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

            var command = new CreateClient(
                request.Name,
                request.ContactEmail,
                request.Phone,
                request.Address,
                request.TierId,
                request.Status,
                request.IndustryTag,
                request.Notes,
                request.CustomFields
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
                extensions: new Dictionary<string, object?> { { "errorCode", "create_client_failed" } });
        }
    }

    private static async Task<IResult> UpdateClientEndpoint(
        Guid client_id,
        UpdateClientRequest request,
        HttpContext httpContext,
        ICommandHandler<UpdateClient> handler,
        IValidator<UpdateClientRequest> validator,
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

            var command = new UpdateClient(
                client_id,
                request.Name,
                request.ContactEmail,
                request.Phone,
                request.Address,
                request.TierId,
                request.Status,
                request.IndustryTag,
                request.Notes,
                request.CustomFields
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
                extensions: new Dictionary<string, object?> { { "errorCode", "update_client_failed" } });
        }
    }

    private static async Task<IResult> DeleteClientEndpoint(
        Guid client_id,
        HttpContext httpContext,
        ICommandHandler<DeleteClient> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var command = new DeleteClient(client_id);
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
                extensions: new Dictionary<string, object?> { { "errorCode", "delete_client_failed" } });
        }
    }
}
