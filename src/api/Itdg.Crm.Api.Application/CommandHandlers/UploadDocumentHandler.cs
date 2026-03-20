namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UploadDocumentHandler : ICommandHandler<UploadDocument>
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
    private readonly IGenericRepository<DocumentCategory> _categoryRepository;
    private readonly IGenericRepository<Client> _clientRepository;
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<UploadDocumentHandler> _logger;

    public UploadDocumentHandler(
        IDocumentRepository documentRepository,
        IGenericRepository<DocumentVersion> versionRepository,
        IGenericRepository<DocumentCategory> categoryRepository,
        IGenericRepository<Client> clientRepository,
        IGoogleDriveService driveService,
        IGoogleDriveTokenProvider tokenProvider,
        ITenantProvider tenantProvider,
        ICurrentUserProvider currentUserProvider,
        ILogger<UploadDocumentHandler> logger)
    {
        _documentRepository = documentRepository;
        _versionRepository = versionRepository;
        _categoryRepository = categoryRepository;
        _clientRepository = clientRepository;
        _driveService = driveService;
        _tokenProvider = tokenProvider;
        _tenantProvider = tenantProvider;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task HandleAsync(UploadDocument command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Upload Document");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("ClientId", command.ClientId);

        _logger.LogInformation("Uploading document for client {ClientId} | CorrelationId: {CorrelationId}",
            command.ClientId, correlationId);

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

        // Verify client exists
        var client = await _clientRepository.GetByIdAsync(command.ClientId, cancellationToken)
            ?? throw new NotFoundException("Client", command.ClientId);

        // Verify category exists
        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken)
            ?? throw new NotFoundException("DocumentCategory", command.CategoryId);

        // Enforce naming convention
        var fileName = EnforceNamingConvention(command.FileName, category, client.Name);

        // Upload to Google Drive
        var accessToken = _tokenProvider.GetAccessToken()
            ?? throw new DomainException("Google Drive access token is not available.", "google_drive_token_unavailable");

        var driveFile = await _driveService.UploadFileAsync(
            accessToken,
            fileName,
            command.ContentStream,
            command.ContentType,
            command.GoogleDriveParentFolderId,
            cancellationToken);

        _logger.LogInformation("File uploaded to Google Drive with ID {DriveFileId} | CorrelationId: {CorrelationId}",
            driveFile.Id, correlationId);

        // Create Document record
        var tenantId = _tenantProvider.GetTenantId();
        var uploadedById = Guid.Parse(_currentUserProvider.GetEntraObjectId()!);

        var document = new Document
        {
            Id = Guid.NewGuid(),
            ClientId = command.ClientId,
            CategoryId = command.CategoryId,
            FileName = fileName,
            GoogleDriveFileId = driveFile.Id,
            UploadedById = uploadedById,
            CurrentVersion = 1,
            FileSize = command.FileSize,
            MimeType = command.ContentType,
            TenantId = tenantId
        };

        await _documentRepository.AddAsync(document, cancellationToken);

        // Create DocumentVersion record
        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = 1,
            GoogleDriveFileId = driveFile.Id,
            UploadedById = uploadedById,
            UploadedAt = DateTimeOffset.UtcNow
        };

        await _versionRepository.AddAsync(version, cancellationToken);

        _logger.LogInformation("Document {DocumentId} created with version 1 for client {ClientId} | CorrelationId: {CorrelationId}",
            document.Id, command.ClientId, correlationId);
    }

    public static string EnforceNamingConvention(string originalFileName, DocumentCategory category, string clientName)
    {
        if (string.IsNullOrWhiteSpace(category.NamingConvention))
        {
            return originalFileName;
        }

        var extension = Path.GetExtension(originalFileName);
        var date = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");

        var enforced = category.NamingConvention
            .Replace("{ClientName}", clientName, StringComparison.OrdinalIgnoreCase)
            .Replace("{Date}", date, StringComparison.OrdinalIgnoreCase)
            .Replace("{FileName}", Path.GetFileNameWithoutExtension(originalFileName), StringComparison.OrdinalIgnoreCase);

        return enforced + extension;
    }
}
