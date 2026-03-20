namespace Itdg.Crm.Api.Requests;

using System.Text.Json.Serialization;

public class RenderTemplateRequest
{
    [JsonPropertyName("merge_fields")]
    public required Dictionary<string, string> MergeFields { get; set; }
}
