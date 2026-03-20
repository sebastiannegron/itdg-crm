namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Itdg.Crm.Api.Domain.GeneralConstants;

public class UpdateClientRequest
{
    [JsonPropertyName("name")]
    [Required, StringLength(200, MinimumLength = 2)]
    public required string Name { get; set; }

    [JsonPropertyName("contact_email")]
    [StringLength(200)]
    [EmailAddress]
    public string? ContactEmail { get; set; }

    [JsonPropertyName("phone")]
    [StringLength(50)]
    public string? Phone { get; set; }

    [JsonPropertyName("address")]
    [StringLength(500)]
    public string? Address { get; set; }

    [JsonPropertyName("tier_id")]
    public Guid? TierId { get; set; }

    [JsonPropertyName("status")]
    [Required]
    public required ClientStatus Status { get; set; }

    [JsonPropertyName("industry_tag")]
    [StringLength(100)]
    public string? IndustryTag { get; set; }

    [JsonPropertyName("notes")]
    [StringLength(2000)]
    public string? Notes { get; set; }

    [JsonPropertyName("custom_fields")]
    [StringLength(4000)]
    public string? CustomFields { get; set; }
}
