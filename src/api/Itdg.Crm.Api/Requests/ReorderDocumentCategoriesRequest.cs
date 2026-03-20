namespace Itdg.Crm.Api.Requests;

using System.Text.Json.Serialization;

public class ReorderDocumentCategoriesRequest
{
    [JsonPropertyName("items")]
    public required IReadOnlyList<ReorderDocumentCategoryItem> Items { get; set; }
}

public class ReorderDocumentCategoryItem
{
    [JsonPropertyName("category_id")]
    public required Guid CategoryId { get; set; }

    [JsonPropertyName("sort_order")]
    public required int SortOrder { get; set; }
}
