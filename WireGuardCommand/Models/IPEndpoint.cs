namespace WireGuardCommand.Models
{
    public class IPEndpoint
    {
        public string? Endpoint { get; set; }
        public int? Port { get; set; }

        public override string ToString()
        {
            return $"{Endpoint}:{Port}";
        }
    }
}
