namespace Itdg.Crm.Api.Diagnostics;

using System.Diagnostics;

public static class DiagnosticsConfig
{
    public const string ServiceName = "Itdg.Crm.Api";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
}
