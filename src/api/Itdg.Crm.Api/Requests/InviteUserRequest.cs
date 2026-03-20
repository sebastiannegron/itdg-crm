namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Itdg.Crm.Api.Domain.GeneralConstants;

public class InviteUserRequest
{
    [JsonPropertyName("email")]
    [Required, StringLength(200)]
    [EmailAddress]
    public required string Email { get; set; }

    [JsonPropertyName("display_name")]
    [Required, StringLength(200, MinimumLength = 2)]
    public required string DisplayName { get; set; }

    [JsonPropertyName("role")]
    [Required]
    public required UserRole Role { get; set; }
}
