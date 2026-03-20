namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetUsersHandler : IQueryHandler<GetUsers, PaginatedResultDto<UserDto>>
{
    private readonly IUserRepository _repository;
    private readonly ILogger<GetUsersHandler> _logger;

    public GetUsersHandler(
        IUserRepository repository,
        ILogger<GetUsersHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<UserDto>> HandleAsync(GetUsers query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Users");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting users page {Page} | CorrelationId: {CorrelationId}", query.Page, correlationId);

        var (items, totalCount) = await _repository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Role,
            query.IsActive,
            query.Search,
            cancellationToken);

        var userDtos = items.Select(user => new UserDto(
            UserId: user.Id,
            EntraObjectId: user.EntraObjectId,
            Email: user.Email,
            DisplayName: user.DisplayName,
            Role: user.Role.ToString(),
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt,
            UpdatedAt: user.UpdatedAt
        )).ToList();

        return new PaginatedResultDto<UserDto>(
            Items: userDtos,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }
}
