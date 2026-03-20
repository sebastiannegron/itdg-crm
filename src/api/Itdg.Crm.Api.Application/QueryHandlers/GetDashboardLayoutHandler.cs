namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDashboardLayoutHandler : IQueryHandler<GetDashboardLayout, DashboardLayoutDto?>
{
    private readonly IDashboardLayoutRepository _repository;
    private readonly ILogger<GetDashboardLayoutHandler> _logger;

    public GetDashboardLayoutHandler(
        IDashboardLayoutRepository repository,
        ILogger<GetDashboardLayoutHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DashboardLayoutDto?> HandleAsync(GetDashboardLayout query, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Dashboard Layout");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Getting dashboard layout for user {UserId} | CorrelationId: {CorrelationId}", query.UserId, correlationId);

        var layout = await _repository.GetByUserIdAsync(query.UserId, cancellationToken);

        if (layout is null)
        {
            return null;
        }

        return new DashboardLayoutDto(
            Id: layout.Id,
            UserId: layout.UserId,
            WidgetConfigurations: layout.WidgetConfigurations,
            CreatedAt: layout.CreatedAt,
            UpdatedAt: layout.UpdatedAt
        );
    }
}
