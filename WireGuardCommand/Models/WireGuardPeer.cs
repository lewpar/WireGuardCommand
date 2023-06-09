namespace WireGuardCommand.Models
{
    public class WireGuardPeer
    {
        public int Id { get; set; }
        public string? PublicKey { get; set; }
        public string? PresharedKey { get; set; }
        public string? AllowedIPS { get; set; }

        public WireGuardClient? Config { get; set; }
    }
}
