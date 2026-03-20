namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record DeleteDocumentCategory(
    Guid CategoryId
) : ICommand;
