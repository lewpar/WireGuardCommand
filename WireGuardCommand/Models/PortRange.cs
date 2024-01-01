namespace WireGuardCommand.Models
{
    public enum PortRange : int
    {
        Min = 0,
        WellKnown = 1024,
        Registered = 49152,
        Max = 65535
    }
}
