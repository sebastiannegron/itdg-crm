namespace Itdg.Crm.Api.Endpoints;

using FluentValidation;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Requests;

public static class UsersEndpoints
{
    public static RouteGroupBuilder MapUsersEndpoints(this IEndpointRouteBuilder builder)
    {
        RouteGroupBuilder group = builder.MapGroup("/api/v1/Users");
        group.WithTags("Users");

        group.MapGet("", GetUsersEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("GetUsers")
            .Produces<PaginatedResultDto<UserDto>>(StatusCodes.Status200OK);

        group.MapGet("/{user_id:guid}", GetUserByIdEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("GetUserById")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{user_id:guid}", UpdateUserEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("UpdateUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapPost("/invite", InviteUserEndpoint)
            .RequireAuthorization(AuthorizationPolicyNames.Administrator)
            .WithName("InviteUser")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem();

        return group;
    }

    private static async Task<IResult> GetUsersEndpoint(
        HttpContext httpContext,
        IQueryHandler<GetUsers, PaginatedResultDto<UserDto>> handler,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        UserRole? role = null,
        bool? isActive = null,
        string? search = null)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var query = new GetUsers(page, pageSize, role, isActive, search);
            var result = await handler.HandleAsync(query, Guid.Parse(correlationId!), cancellationToken);
            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "get_users_failed" } });
        }
    }

    private static async Task<IResult> GetUserByIdEndpoint(
        Guid user_id,
        HttpContext httpContext,
        IQueryHandler<GetUserById, UserDto> handler,
        CancellationToken cancellationToken)
    {
        string? correlationId = httpContext.Request.Headers["X-Correlation-Id"];
        try
        {
            var result = await handler.HandleAsync(new GetUserById(user_id), Guid.Parse(correlationId!), cancellationToken);
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
                extensions: new Dictionary<string, object?> { { "errorCode", "get_user_failed" } });
        }
    }

    private static async Task<IResult> UpdateUserEndpoint(
        Guid user_id,
        UpdateUserRequest request,
        HttpContext httpContext,
        ICommandHandler<UpdateUser> handler,
        IValidator<UpdateUserRequest> validator,
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

            var command = new UpdateUser(
                user_id,
                request.Role,
                request.IsActive
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
                extensions: new Dictionary<string, object?> { { "errorCode", "update_user_failed" } });
        }
    }

    private static async Task<IResult> InviteUserEndpoint(
        InviteUserRequest request,
        HttpContext httpContext,
        ICommandHandler<InviteUser> handler,
        IValidator<InviteUserRequest> validator,
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

            var command = new InviteUser(
                request.Email,
                request.DisplayName,
                request.Role
            );

            string language = httpContext.Request.Headers.AcceptLanguage.FirstOrDefault() ?? "en-pr";
            await handler.HandleAsync(command, language, Guid.Parse(correlationId!), cancellationToken);
            return Results.Created();
        }
        catch (ConflictException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict,
                extensions: new Dictionary<string, object?> { { "errorCode", ex.ErrorCode } });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { { "errorCode", "invite_user_failed" } });
        }
    }
}
