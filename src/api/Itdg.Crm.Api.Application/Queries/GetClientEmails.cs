namespace Itdg.Crm.Api.Application.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;

public record GetClientEmails(
    Guid ClientId,
    int Page = 1,
    int PageSize = 20,
    string? Search = null
) : IQuery<PaginatedResultDto<EmailMirrorDto>>;
