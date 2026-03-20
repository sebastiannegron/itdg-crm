namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record CreateTier(
    string Name,
    int SortOrder
) : ICommand;
