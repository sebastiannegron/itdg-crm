namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class CreateDocumentCategoryHandler : ICommandHandler<CreateDocumentCategory>
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateDocumentCategoryHandler> _logger;

    public CreateDocumentCategoryHandler(
        IGenericRepository<DocumentCategory> repository,
        ITenantProvider tenantProvider,
        ILogger<CreateDocumentCategoryHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task HandleAsync(CreateDocumentCategory command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Create Document Category");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Creating document category {CategoryName} | CorrelationId: {CorrelationId}", command.Name, correlationId);

        var category = new DocumentCategory
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            NamingConvention = command.NamingConvention,
            SortOrder = command.SortOrder,
            IsDefault = false,
            TenantId = _tenantProvider.GetTenantId()
        };

        await _repository.AddAsync(category, cancellationToken);

        _logger.LogInformation("Document category {CategoryId} created successfully | CorrelationId: {CorrelationId}", category.Id, correlationId);
    }
}
