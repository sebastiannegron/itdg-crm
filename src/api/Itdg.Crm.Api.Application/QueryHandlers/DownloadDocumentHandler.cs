namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DownloadDocumentHandler : IQueryHandler<DownloadDocument, DocumentDownloadDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IAuditService _auditService;
    private readonly ILogger<DownloadDocumentHandler> _logger;

    public DownloadDocumentHandler(
        IDocumentRepository documentRepository,
        IUserRepository userRepository,
        IClientAssignmentRepository clientAssignmentRepository,
        IGoogleDriveTokenProvider tokenProvider,
        ICurrentUserProvider currentUserProvider,
        IAuditService auditService,
        ILogger<DownloadDocumentHandler> logger)
    {
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _clientAssignmentRepository = clientAssignmentRepository;
        _tokenProvider = tokenProvider;
        _currentUserProvider = currentUserProvider;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<DocumentDownloadDto> HandleAsync(
        DownloadDocument query,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Download Document");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation(
            "Downloading document {DocumentId} | CorrelationId: {CorrelationId}",
            query.DocumentId, correlationId);

        var document = await _documentRepository.GetByIdWithCategoryAsync(query.DocumentId, cancellationToken);
        if (document is null)
        {
            throw new NotFoundException("Document", query.DocumentId);
        }

        if (!_currentUserProvider.IsInRole(nameof(UserRole.Administrator)))
        {
            var entraObjectId = _currentUserProvider.GetEntraObjectId();
            if (string.IsNullOrWhiteSpace(entraObjectId))
            {
                throw new ForbiddenException();
            }

            var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
            if (user is null)
            {
                throw new ForbiddenException();
            }

            var isAssigned = await _clientAssignmentRepository.ExistsAsync(user.Id, document.ClientId, cancellationToken);
            if (!isAssigned)
            {
                throw new ForbiddenException();
            }
        }

        var accessToken = _tokenProvider.GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new DomainException("Google Drive access token is not available.", "google_drive_token_unavailable");
        }

        await _auditService.LogAccessAsync(nameof(Document), document.Id, "Download", cancellationToken);

        string webViewLink = $"https://drive.google.com/file/d/{document.GoogleDriveFileId}/view";

        return new DocumentDownloadDto(
            DocumentId: document.Id,
            FileName: document.FileName,
            MimeType: document.MimeType,
            FileSize: document.FileSize,
            GoogleDriveFileId: document.GoogleDriveFileId,
            WebViewLink: webViewLink
        );
    }
}
