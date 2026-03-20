namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class AssignClientRequest
{
    [JsonPropertyName("user_id")]
    [Required]
    public required Guid UserId { get; set; }
}
