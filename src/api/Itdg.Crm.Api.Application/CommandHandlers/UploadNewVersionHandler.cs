namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UploadNewVersionHandler : ICommandHandler<UploadNewVersion>
{
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "text/plain",
        "text/csv"
    };

    private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

    private readonly IDocumentRepository _documentRepository;
    private readonly IGenericRepository<DocumentVersion> _versionRepository;
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<UploadNewVersionHandler> _logger;

    public UploadNewVersionHandler(
        IDocumentRepository documentRepository,
        IGenericRepository<DocumentVersion> versionRepository,
        IGoogleDriveService driveService,
        IGoogleDriveTokenProvider tokenProvider,
        ICurrentUserProvider currentUserProvider,
        ILogger<UploadNewVersionHandler> logger)
    {
        _documentRepository = documentRepository;
        _versionRepository = versionRepository;
        _driveService = driveService;
        _tokenProvider = tokenProvider;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task HandleAsync(UploadNewVersion command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Upload New Version");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("DocumentId", command.DocumentId);

        _logger.LogInformation("Uploading new version for document {DocumentId} | CorrelationId: {CorrelationId}",
            command.DocumentId, correlationId);

        // Validate file type
        if (!AllowedMimeTypes.Contains(command.ContentType))
        {
            throw new DomainException(
                $"File type '{command.ContentType}' is not allowed. Allowed types: {string.Join(", ", AllowedMimeTypes)}",
                "invalid_file_type");
        }

        // Validate file size
        if (command.FileSize <= 0 || command.FileSize > MaxFileSizeBytes)
        {
            throw new DomainException(
                $"File size must be between 1 byte and {MaxFileSizeBytes / (1024 * 1024)} MB.",
                "invalid_file_size");
        }

        // Verify document exists
        var document = await _documentRepository.GetByIdWithCategoryAsync(command.DocumentId, cancellationToken)
            ?? throw new NotFoundException("Document", command.DocumentId);

        // Upload to Google Drive
        var accessToken = _tokenProvider.GetAccessToken()
            ?? throw new DomainException("Google Drive access token is not available.", "google_drive_token_unavailable");

        var driveFile = await _driveService.UploadFileAsync(
            accessToken,
            command.FileName,
            command.ContentStream,
            command.ContentType,
            null,
            cancellationToken);

        _logger.LogInformation("New version uploaded to Google Drive with ID {DriveFileId} | CorrelationId: {CorrelationId}",
            driveFile.Id, correlationId);

        // Increment version
        var newVersionNumber = document.CurrentVersion + 1;

        // Create DocumentVersion record
        var uploadedById = Guid.Parse(_currentUserProvider.GetEntraObjectId()!);

        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = newVersionNumber,
            GoogleDriveFileId = driveFile.Id,
            UploadedById = uploadedById,
            UploadedAt = DateTimeOffset.UtcNow
        };

        await _versionRepository.AddAsync(version, cancellationToken);

        // Update document record
        document.CurrentVersion = newVersionNumber;
        document.GoogleDriveFileId = driveFile.Id;
        document.FileSize = command.FileSize;
        document.MimeType = command.ContentType;

        await _documentRepository.UpdateAsync(document, cancellationToken);

        _logger.LogInformation("Document {DocumentId} updated to version {VersionNumber} | CorrelationId: {CorrelationId}",
            document.Id, newVersionNumber, correlationId);
    }
}
