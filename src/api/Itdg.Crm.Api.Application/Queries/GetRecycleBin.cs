namespace Itdg.Crm.Api.Application.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;

public record GetRecycleBin(
    int Page = 1,
    int PageSize = 20
) : IQuery<PaginatedResultDto<RecycleBinDocumentDto>>;
