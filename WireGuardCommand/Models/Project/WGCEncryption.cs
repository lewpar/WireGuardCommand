using System.Text.Json.Serialization;

namespace WireGuardCommand.Models.Project
{
    public class WGCEncryption
    {
        [JsonPropertyName("IV")]
        public string? IV { get; set; }

        [JsonPropertyName("Salt")]
        public string? Salt { get; set; }
    }
}
