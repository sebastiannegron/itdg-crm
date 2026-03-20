namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateDocumentCategoryHandler : ICommandHandler<UpdateDocumentCategory>
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ILogger<UpdateDocumentCategoryHandler> _logger;

    public UpdateDocumentCategoryHandler(
        IGenericRepository<DocumentCategory> repository,
        ILogger<UpdateDocumentCategoryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(UpdateDocumentCategory command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Update Document Category");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Updating document category {CategoryId} | CorrelationId: {CorrelationId}", command.CategoryId, correlationId);

        var category = await _repository.GetByIdAsync(command.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(DocumentCategory), command.CategoryId);

        category.Name = command.Name;
        category.NamingConvention = command.NamingConvention;
        category.SortOrder = command.SortOrder;

        await _repository.UpdateAsync(category, cancellationToken);

        _logger.LogInformation("Document category {CategoryId} updated successfully | CorrelationId: {CorrelationId}", command.CategoryId, correlationId);
    }
}
