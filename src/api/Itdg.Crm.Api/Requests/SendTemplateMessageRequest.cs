namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class SendTemplateMessageRequest
{
    [JsonPropertyName("template_id")]
    [Required]
    public required Guid TemplateId { get; set; }

    [JsonPropertyName("client_id")]
    [Required]
    public required Guid ClientId { get; set; }

    [JsonPropertyName("merge_fields")]
    [Required]
    public required Dictionary<string, string> MergeFields { get; set; }

    [JsonPropertyName("send_via_portal")]
    public bool SendViaPortal { get; set; } = true;

    [JsonPropertyName("send_via_email")]
    public bool SendViaEmail { get; set; }

    [JsonPropertyName("recipient_email")]
    [EmailAddress]
    [StringLength(320)]
    public string? RecipientEmail { get; set; }
}
