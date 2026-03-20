namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record UpdateTier(
    Guid TierId,
    string Name,
    int SortOrder
) : ICommand;
