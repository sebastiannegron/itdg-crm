namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DeleteDocumentCategoryHandler : ICommandHandler<DeleteDocumentCategory>
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ILogger<DeleteDocumentCategoryHandler> _logger;

    public DeleteDocumentCategoryHandler(
        IGenericRepository<DocumentCategory> repository,
        ILogger<DeleteDocumentCategoryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(DeleteDocumentCategory command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Delete Document Category");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Deleting document category {CategoryId} | CorrelationId: {CorrelationId}", command.CategoryId, correlationId);

        var category = await _repository.GetByIdAsync(command.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(DocumentCategory), command.CategoryId);

        await _repository.DeleteAsync(category, cancellationToken);

        _logger.LogInformation("Document category {CategoryId} deleted successfully | CorrelationId: {CorrelationId}", command.CategoryId, correlationId);
    }
}
