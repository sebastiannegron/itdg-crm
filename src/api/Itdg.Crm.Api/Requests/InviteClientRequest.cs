namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class InviteClientRequest
{
    [JsonPropertyName("email")]
    [Required, StringLength(256)]
    [EmailAddress]
    public required string Email { get; set; }
}
