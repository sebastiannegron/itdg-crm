namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record CreateDocumentCategory(
    string Name,
    string? NamingConvention,
    int SortOrder
) : ICommand;
