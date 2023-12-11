using System.Text.Json.Serialization;

namespace WireGuardCommand.Models.Project
{
    public class WGCProject
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("WireGuardConfig")]
        public WGCConfig? Configuration { get; set; }
    }
}
