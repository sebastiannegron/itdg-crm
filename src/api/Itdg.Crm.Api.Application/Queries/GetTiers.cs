namespace Itdg.Crm.Api.Application.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;

public record GetTiers() : IQuery<IEnumerable<ClientTierDto>>;
