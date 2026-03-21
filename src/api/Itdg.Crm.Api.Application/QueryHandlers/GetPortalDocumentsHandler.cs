namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetPortalDocumentsHandler : IQueryHandler<GetPortalDocuments, PaginatedResultDto<DocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetPortalDocumentsHandler> _logger;

    public GetPortalDocumentsHandler(
        IDocumentRepository documentRepository,
        ILogger<GetPortalDocumentsHandler> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<DocumentDto>> HandleAsync(
        GetPortalDocuments query,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Portal Documents");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation(
            "Portal client {ClientId} retrieving documents page {Page} | CorrelationId: {CorrelationId}",
            query.ClientId, query.Page, correlationId);

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
