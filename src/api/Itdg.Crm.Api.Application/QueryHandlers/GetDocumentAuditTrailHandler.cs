namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDocumentAuditTrailHandler : IQueryHandler<GetDocumentAuditTrail, PaginatedResultDto<AuditLogDto>>
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetDocumentAuditTrailHandler> _logger;

    public GetDocumentAuditTrailHandler(
        IAuditLogRepository auditLogRepository,
        IDocumentRepository documentRepository,
        ILogger<GetDocumentAuditTrailHandler> logger)
    {
        _auditLogRepository = auditLogRepository;
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<AuditLogDto>> HandleAsync(
        GetDocumentAuditTrail query,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Document Audit Trail");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("DocumentId", query.DocumentId);

        _logger.LogInformation(
            "Getting audit trail for document {DocumentId} page {Page} | CorrelationId: {CorrelationId}",
            query.DocumentId, query.Page, correlationId);

        var document = await _documentRepository.GetByIdAsync(query.DocumentId, cancellationToken);
        if (document is null)
        {
            throw new NotFoundException("Document", query.DocumentId);
        }

        var (items, totalCount) = await _auditLogRepository.GetByEntityAsync(
            nameof(Document),
            query.DocumentId,
            query.Page,
            query.PageSize,
            cancellationToken);

        var dtos = items.Select(log => new AuditLogDto(
            AuditLogId: log.Id,
            UserId: log.UserId,
            EntityType: log.EntityType,
            EntityId: log.EntityId,
            Action: log.Action,
            OldValues: log.OldValues,
            NewValues: log.NewValues,
            Timestamp: log.Timestamp,
            IpAddress: log.IpAddress
        )).ToList();

        return new PaginatedResultDto<AuditLogDto>(
            Items: dtos,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }
}
