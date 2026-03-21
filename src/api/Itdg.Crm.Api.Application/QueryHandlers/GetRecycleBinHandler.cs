namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetRecycleBinHandler : IQueryHandler<GetRecycleBin, PaginatedResultDto<RecycleBinDocumentDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetRecycleBinHandler> _logger;

    public GetRecycleBinHandler(
        IDocumentRepository documentRepository,
        ILogger<GetRecycleBinHandler> logger)
    {
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<RecycleBinDocumentDto>> HandleAsync(
        GetRecycleBin query,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Recycle Bin");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation(
            "Getting recycle bin documents page {Page} | CorrelationId: {CorrelationId}",
            query.Page, correlationId);

        var (items, totalCount) = await _documentRepository.GetDeletedDocumentsPagedAsync(
            query.Page,
            query.PageSize,
            cancellationToken);

        var dtos = items.Select(doc => new RecycleBinDocumentDto(
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
            DeletedAt: doc.DeletedAt
        )).ToList();

        return new PaginatedResultDto<RecycleBinDocumentDto>(
            Items: dtos,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }
}
