namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class SaveDashboardLayoutHandler : ICommandHandler<SaveDashboardLayout>
{
    private readonly IDashboardLayoutRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<SaveDashboardLayoutHandler> _logger;

    public SaveDashboardLayoutHandler(
        IDashboardLayoutRepository repository,
        ITenantProvider tenantProvider,
        ILogger<SaveDashboardLayoutHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task HandleAsync(SaveDashboardLayout command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Save Dashboard Layout");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Saving dashboard layout for user {UserId} | CorrelationId: {CorrelationId}", command.UserId, correlationId);

        var existing = await _repository.GetByUserIdAsync(command.UserId, cancellationToken);

        if (existing is not null)
        {
            existing.WidgetConfigurations = command.WidgetConfigurations;
            await _repository.UpdateAsync(existing, cancellationToken);

            _logger.LogInformation("Dashboard layout {LayoutId} updated for user {UserId} | CorrelationId: {CorrelationId}", existing.Id, command.UserId, correlationId);
        }
        else
        {
            var layout = new DashboardLayout
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                WidgetConfigurations = command.WidgetConfigurations,
                TenantId = _tenantProvider.GetTenantId()
            };

            await _repository.AddAsync(layout, cancellationToken);

            _logger.LogInformation("Dashboard layout {LayoutId} created for user {UserId} | CorrelationId: {CorrelationId}", layout.Id, command.UserId, correlationId);
        }
    }
}
