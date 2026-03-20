namespace Itdg.Crm.Api.Extensions;

using Itdg.Crm.Api.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapAllEndpoints(this WebApplication app)
    {
        app.MapHealthEndpoints();
        app.MapClientsEndpoints();
        app.MapTemplatesEndpoints();
        app.MapPortalEndpoints();
        app.MapMessagesEndpoints();
        app.MapUsersEndpoints();
        app.MapTiersEndpoints();
        app.MapDashboardEndpoints();
        app.MapDocumentCategoriesEndpoints();
        app.MapNotificationsEndpoints();
        return app;
    }
}
