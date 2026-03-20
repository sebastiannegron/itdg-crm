namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record DashboardSummaryDto(
    [property: JsonPropertyName("total_clients")] int TotalClients,
    [property: JsonPropertyName("clients_by_status")] IReadOnlyList<ClientStatusCountDto> ClientsByStatus,
    [property: JsonPropertyName("clients_by_tier")] IReadOnlyList<ClientTierCountDto> ClientsByTier,
    [property: JsonPropertyName("pending_tasks_count")] int PendingTasksCount,
    [property: JsonPropertyName("recent_escalations_count")] int RecentEscalationsCount,
    [property: JsonPropertyName("upcoming_deadlines_count")] int UpcomingDeadlinesCount,
    [property: JsonPropertyName("unread_notifications_count")] int UnreadNotificationsCount
);

public record ClientStatusCountDto(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("count")] int Count
);

public record ClientTierCountDto(
    [property: JsonPropertyName("tier_id")] Guid? TierId,
    [property: JsonPropertyName("tier_name")] string? TierName,
    [property: JsonPropertyName("count")] int Count
);
