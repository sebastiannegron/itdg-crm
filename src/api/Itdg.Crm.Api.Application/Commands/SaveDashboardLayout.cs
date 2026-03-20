namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record SaveDashboardLayout(
    Guid UserId,
    string WidgetConfigurations
) : ICommand;
