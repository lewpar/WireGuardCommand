using System.Text.Json.Serialization;

namespace WireGuardCommand.Services.Models;

public class ProjectMetadata
{
    [JsonIgnore]
    public string? Path { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("IsEncrypted")]
    public bool IsEncrypted { get; set; }
}
