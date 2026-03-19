namespace Itdg.Crm.Api.Application.Dtos;

using System.Text.Json.Serialization;

public record PaginatedResultDto<T>(
    [property: JsonPropertyName("items")] IReadOnlyList<T> Items,
    [property: JsonPropertyName("total_count")] int TotalCount,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("page_size")] int PageSize
);
