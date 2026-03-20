namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record SendTemplateMessage(
    Guid TemplateId,
    Guid ClientId,
    Guid SenderId,
    IDictionary<string, string> MergeFields,
    bool SendViaPortal,
    bool SendViaEmail,
    string? RecipientEmail) : ICommand;
