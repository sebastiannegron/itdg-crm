namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DashboardLayoutDto(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("user_id")] Guid UserId,
    [property: JsonPropertyName("widget_configurations")] string? WidgetConfigurations,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt
);
