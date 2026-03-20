namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetUserByIdHandler : IQueryHandler<GetUserById, UserDto>
{
    private readonly IUserRepository _repository;
    private readonly ILogger<GetUserByIdHandler> _logger;

    public GetUserByIdHandler(
        IUserRepository repository,
        ILogger<GetUserByIdHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserDto> HandleAsync(GetUserById query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get User By Id");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting user {UserId} | CorrelationId: {CorrelationId}", query.UserId, correlationId);

        var user = await _repository.GetByIdAsync(query.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), query.UserId);

        return new UserDto(
            UserId: user.Id,
            EntraObjectId: user.EntraObjectId,
            Email: user.Email,
            DisplayName: user.DisplayName,
            Role: user.Role.ToString(),
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        );
    }
}
