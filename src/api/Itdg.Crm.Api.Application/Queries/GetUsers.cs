namespace Itdg.Crm.Api.Application.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Domain.GeneralConstants;

public record GetUsers(
    int Page = 1,
    int PageSize = 20,
    UserRole? Role = null,
    bool? IsActive = null,
    string? Search = null
) : IQuery<PaginatedResultDto<UserDto>>;
