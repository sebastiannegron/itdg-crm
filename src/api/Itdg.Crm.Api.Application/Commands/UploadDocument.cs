namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record UploadDocument(
    Guid ClientId,
    Guid CategoryId,
    string FileName,
    Stream ContentStream,
    string ContentType,
    long FileSize,
    string? GoogleDriveParentFolderId
) : ICommand;
