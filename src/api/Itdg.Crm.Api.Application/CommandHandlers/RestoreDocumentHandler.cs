namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class RestoreDocumentHandler : ICommandHandler<RestoreDocument>
{
    private readonly IDocumentRepository _repository;
    private readonly ILogger<RestoreDocumentHandler> _logger;

    public RestoreDocumentHandler(
        IDocumentRepository repository,
        ILogger<RestoreDocumentHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(RestoreDocument command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Restore Document");
        activity?.SetTag("CorrelationId", correlationId);
        activity?.SetTag("DocumentId", command.DocumentId);

        _logger.LogInformation("Restoring document {DocumentId} | CorrelationId: {CorrelationId}", command.DocumentId, correlationId);

        var document = await _repository.GetByIdIncludingDeletedAsync(command.DocumentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Document), command.DocumentId);

        if (document.DeletedAt is null)
        {
            throw new DomainException("Document is not in the recycle bin.", "document_not_deleted");
        }

        document.DeletedAt = null;

        await _repository.UpdateAsync(document, cancellationToken);

        _logger.LogInformation("Document {DocumentId} restored successfully | CorrelationId: {CorrelationId}", command.DocumentId, correlationId);
    }
}
