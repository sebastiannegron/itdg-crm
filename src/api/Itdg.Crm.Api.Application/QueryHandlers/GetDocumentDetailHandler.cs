namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDocumentDetailHandler : IQueryHandler<GetDocumentDetail, DocumentDetailDto>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetDocumentDetailHandler> _logger;

    public GetDocumentDetailHandler(
        IDocumentRepository documentRepository,
        IUserRepository userRepository,
        IClientAssignmentRepository clientAssignmentRepository,
        IGoogleDriveTokenProvider tokenProvider,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetDocumentDetailHandler> logger)
    {
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _clientAssignmentRepository = clientAssignmentRepository;
        _tokenProvider = tokenProvider;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<DocumentDetailDto> HandleAsync(
        GetDocumentDetail query,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Document Detail");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation(
            "Getting document detail {DocumentId} | CorrelationId: {CorrelationId}",
            query.DocumentId, correlationId);

        var (document, versions) = await _documentRepository.GetByIdWithVersionsAsync(query.DocumentId, cancellationToken);
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
        string? webViewLink = !string.IsNullOrWhiteSpace(accessToken)
            ? $"https://drive.google.com/file/d/{document.GoogleDriveFileId}/view"
            : null;

        var versionDtos = versions.Select(v => new DocumentVersionDto(
            VersionId: v.Id,
            DocumentId: v.DocumentId,
            VersionNumber: v.VersionNumber,
            GoogleDriveFileId: v.GoogleDriveFileId,
            UploadedById: v.UploadedById,
            UploadedAt: v.UploadedAt
        )).ToList();

        return new DocumentDetailDto(
            DocumentId: document.Id,
            ClientId: document.ClientId,
            CategoryId: document.CategoryId,
            CategoryName: document.Category?.Name,
            FileName: document.FileName,
            GoogleDriveFileId: document.GoogleDriveFileId,
            UploadedById: document.UploadedById,
            CurrentVersion: document.CurrentVersion,
            FileSize: document.FileSize,
            MimeType: document.MimeType,
            CreatedAt: document.CreatedAt,
            UpdatedAt: document.UpdatedAt,
            WebViewLink: webViewLink,
            Versions: versionDtos
        );
    }
}
