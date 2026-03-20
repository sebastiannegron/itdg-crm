namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record ReorderDocumentCategories(
    IReadOnlyList<ReorderItem> Items
) : ICommand;

public record ReorderItem(
    Guid CategoryId,
    int SortOrder
);
