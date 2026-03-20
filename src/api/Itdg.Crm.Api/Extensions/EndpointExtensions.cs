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
        app.MapUsersEndpoints();
        app.MapTiersEndpoints();
        return app;
    }
}
