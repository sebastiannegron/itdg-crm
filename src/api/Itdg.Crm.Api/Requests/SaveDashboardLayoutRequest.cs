namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class SaveDashboardLayoutRequest
{
    [JsonPropertyName("widget_configurations")]
    [Required]
    public required string WidgetConfigurations { get; set; }
}
