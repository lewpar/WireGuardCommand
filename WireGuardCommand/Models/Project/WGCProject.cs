using System.Text.Json.Serialization;

namespace WireGuardCommand.Models.Project
{
    public class WGCProject
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Encrypted")]
        public bool Encrypted { get; set; }

        [JsonPropertyName("Encryption")]
        public WGCEncryption? Encryption { get; set; }

        [JsonIgnore]
        public string? Path { get; set; }

        [JsonIgnore]
        public const string PATH_PROJECTS = @"./Projects";
    }
}
