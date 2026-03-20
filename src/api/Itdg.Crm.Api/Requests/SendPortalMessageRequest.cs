namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class SendPortalMessageRequest
{
    [JsonPropertyName("subject")]
    [Required, StringLength(500, MinimumLength = 1)]
    public required string Subject { get; set; }

    [JsonPropertyName("body")]
    [Required, StringLength(4000, MinimumLength = 1)]
    public required string Body { get; set; }
}
