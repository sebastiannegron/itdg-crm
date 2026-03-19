namespace Itdg.Crm.Api.Infrastructure.Options;

using System.ComponentModel.DataAnnotations;

public class DatabaseOptions
{
    public const string Key = "Database";

    [Required]
    public required string ConnectionString { get; set; }
}
