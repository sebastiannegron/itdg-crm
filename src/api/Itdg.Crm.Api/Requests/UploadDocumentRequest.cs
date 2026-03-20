namespace Itdg.Crm.Api.Requests;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UploadDocumentRequest
{
    [JsonPropertyName("category_id")]
    [Required]
    public required Guid CategoryId { get; set; }

    [JsonPropertyName("google_drive_parent_folder_id")]
    [StringLength(200)]
    public string? GoogleDriveParentFolderId { get; set; }
}
