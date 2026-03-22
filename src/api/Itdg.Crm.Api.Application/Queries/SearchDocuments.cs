namespace Itdg.Crm.Api.Application.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;

public record SearchDocuments(
    string Query,
    Guid? ClientId = null,
    string? Category = null,
    DateTimeOffset? DateFrom = null,
    DateTimeOffset? DateTo = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<PaginatedResultDto<DocumentSearchResultDto>>;
