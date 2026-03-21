namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record UploadPortalDocument(
    Guid ClientId,
    Guid CategoryId,
    string FileName,
    Stream ContentStream,
    string ContentType,
    long FileSize
) : ICommand;
