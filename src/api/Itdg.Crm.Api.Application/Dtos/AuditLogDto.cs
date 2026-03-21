namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record AuditLogDto(
    [property: JsonPropertyName("audit_log_id")] Guid AuditLogId,
    [property: JsonPropertyName("user_id")] Guid UserId,
    [property: JsonPropertyName("entity_type")] string EntityType,
    [property: JsonPropertyName("entity_id")] Guid EntityId,
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("old_values")] string? OldValues,
    [property: JsonPropertyName("new_values")] string? NewValues,
    [property: JsonPropertyName("timestamp")] DateTimeOffset Timestamp,
    [property: JsonPropertyName("ip_address")] string? IpAddress
);
