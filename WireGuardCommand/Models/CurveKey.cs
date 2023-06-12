using System;

namespace WireGuardCommand.Models
{
    public class CurveKey
    {
        public byte[]? Key { get; set; }

        public override string ToString()
        {
            return (Key != null) ? Convert.ToBase64String(Key) : string.Empty;
        }
    }
}
