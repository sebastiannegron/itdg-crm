namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Itdg.Crm.Api.Domain.GeneralConstants;

public class UpdateUserRequest
{
    [JsonPropertyName("role")]
    [Required]
    public required UserRole Role { get; set; }

    [JsonPropertyName("is_active")]
    [Required]
    public required bool IsActive { get; set; }
}
