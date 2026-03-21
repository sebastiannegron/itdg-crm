namespace Itdg.Crm.Api.Application.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;

public record GetPortalDocuments(
    Guid ClientId,
    int Page = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    int? Year = null,
    string? Search = null
) : IQuery<PaginatedResultDto<DocumentDto>>;
