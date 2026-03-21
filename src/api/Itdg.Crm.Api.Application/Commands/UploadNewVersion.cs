namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record UploadNewVersion(
    Guid DocumentId,
    string FileName,
    Stream ContentStream,
    string ContentType,
    long FileSize
) : ICommand;
