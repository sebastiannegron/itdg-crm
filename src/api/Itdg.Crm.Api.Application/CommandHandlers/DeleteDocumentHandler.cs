namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DeleteDocumentHandler : ICommandHandler<DeleteDocument>
{
    private readonly IDocumentRepository _repository;
    private readonly IAuditService _auditService;
    private readonly ILogger<DeleteDocumentHandler> _logger;

    public DeleteDocumentHandler(
        IDocumentRepository repository,
        IAuditService auditService,
        ILogger<DeleteDocumentHandler> logger)
    {
        _repository = repository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task HandleAsync(DeleteDocument command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Delete Document");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("DocumentId", command.DocumentId);

        _logger.LogInformation("Soft-deleting document {DocumentId} | CorrelationId: {CorrelationId}", command.DocumentId, correlationId);

        var document = await _repository.GetByIdAsync(command.DocumentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), command.DocumentId);

        document.DeletedAt = DateTimeOffset.UtcNow;

        await _repository.UpdateAsync(document, cancellationToken);

        await _auditService.LogAccessAsync(nameof(Document), command.DocumentId, "Delete", cancellationToken);

        _logger.LogInformation("Document {DocumentId} soft-deleted successfully | CorrelationId: {CorrelationId}", command.DocumentId, correlationId);
    }
}
