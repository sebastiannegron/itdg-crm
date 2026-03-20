namespace Itdg.Crm.Api.Application.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Domain.GeneralConstants;

public record GetClients(
    int Page = 1,
    int PageSize = 20,
    ClientStatus? Status = null,
    Guid? TierId = null,
    string? Search = null
) : IQuery<PaginatedResultDto<ClientDto>>;
