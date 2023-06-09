namespace WireGuardCommand.Models
{
    public class WireGuardClient
    {
        public string? Address { get; set; }
        public int Port { get; set; }
        public string? PrivateKey { get; set; }
        public string? DNS { get; set; }
        public string? PublicKey { get; set; }
        public string? PresharedKey { get; set; }
        public string? AllowedIPS { get; set; }
        public string? Endpoint { get; set; }
    }
}
