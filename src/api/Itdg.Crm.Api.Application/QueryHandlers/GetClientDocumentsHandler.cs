namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientDocumentsHandler : IQueryHandler<GetClientDocuments, PaginatedResultDto<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientDocumentsHandler> _logger;

    public GetClientDocumentsHandler(
        IDocumentRepository documentRepository,
        IUserRepository userRepository,
        IClientAssignmentRepository clientAssignmentRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetClientDocumentsHandler> logger)
    {
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _clientAssignmentRepository = clientAssignmentRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<DocumentDto>> HandleAsync(
        GetClientDocuments query,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Client Documents");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation(
            "Getting documents for client {ClientId} page {Page} | CorrelationId: {CorrelationId}",
            query.ClientId, query.Page, correlationId);

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

            var isAssigned = await _clientAssignmentRepository.ExistsAsync(user.Id, query.ClientId, cancellationToken);
            if (!isAssigned)
            {
                throw new ForbiddenException();
            }
        }

        var (items, totalCount) = await _documentRepository.GetPagedByClientIdAsync(
            query.ClientId,
            query.Page,
            query.PageSize,
            query.CategoryId,
            query.Year,
            query.Search,
            cancellationToken);

        var documentDtos = items.Select(doc => new DocumentDto(
            DocumentId: doc.Id,
            ClientId: doc.ClientId,
            CategoryId: doc.CategoryId,
            CategoryName: doc.Category?.Name,
            FileName: doc.FileName,
            GoogleDriveFileId: doc.GoogleDriveFileId,
            UploadedById: doc.UploadedById,
            CurrentVersion: doc.CurrentVersion,
            FileSize: doc.FileSize,
            MimeType: doc.MimeType,
            CreatedAt: doc.CreatedAt,
            UpdatedAt: doc.UpdatedAt
        )).ToList();

        return new PaginatedResultDto<DocumentDto>(
            Items: documentDtos,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }
}
