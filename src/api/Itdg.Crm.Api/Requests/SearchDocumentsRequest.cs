namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class SearchDocumentsRequest
{
    [JsonPropertyName("query")]
    [Required, StringLength(200, MinimumLength = 1)]
    public required string Query { get; set; }

    [JsonPropertyName("client_id")]
    public Guid? ClientId { get; set; }

    [JsonPropertyName("category")]
    [StringLength(200)]
    public string? Category { get; set; }

    [JsonPropertyName("date_from")]
    public DateTimeOffset? DateFrom { get; set; }

    [JsonPropertyName("date_to")]
    public DateTimeOffset? DateTo { get; set; }
}
