namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class ReorderDocumentCategoriesHandler : ICommandHandler<ReorderDocumentCategories>
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ILogger<ReorderDocumentCategoriesHandler> _logger;

    public ReorderDocumentCategoriesHandler(
        IGenericRepository<DocumentCategory> repository,
        ILogger<ReorderDocumentCategoriesHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(ReorderDocumentCategories command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Reorder Document Categories");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Reordering {Count} document categories | CorrelationId: {CorrelationId}", command.Items.Count, correlationId);

        foreach (var item in command.Items)
        {
            var category = await _repository.GetByIdAsync(item.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(DocumentCategory), item.CategoryId);

            category.SortOrder = item.SortOrder;
            await _repository.UpdateAsync(category, cancellationToken);
        }

        _logger.LogInformation("Document categories reordered successfully | CorrelationId: {CorrelationId}", correlationId);
    }
}
