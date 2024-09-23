using System.Security.Cryptography;

using WireGuardCommand.Extensions;

namespace WireGuardCommand.Services.Models;

public class ProjectData
{
    public string Seed { get; set; }

    public ProjectData()
    {
        Seed = RandomNumberGenerator.GetBytes(32).ToBase64();
    }

    public ProjectData Copy()
    {
        return new ProjectData()
        {
            Seed = Seed
        };
    }
}
