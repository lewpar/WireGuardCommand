namespace WireGuardCommand.Models
{
    public class WireGuardAddress
    {
        public string? Address { get; set; }
        public int[]? Octets { get; set; }
        public int CIDR { get; set; }
    }
}
