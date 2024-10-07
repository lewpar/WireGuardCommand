using System.Net;
using System.Text;

using WireGuardCommand.IO;
using WireGuardCommand.WireGuard;

namespace WireGuardCommand.Tests
{
    public class WireGuardWriterTests
    {
        private byte[] seed = new byte[0];

        [SetUp]
        public void Setup() 
        { 
            seed = new byte[]
            {
                1, 2, 3, 4, 5, 6, 7, 8,
                9, 10, 11, 12, 13, 14, 15, 16,
                17, 18, 19, 20, 21, 22, 23, 24,
                25, 26, 27, 28, 29, 30, 31, 32
            };
        }

        [Test]
        public void Write_Peer_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                var server = new WireGuardBuilder(
                    new WireGuardBuilderOptions()
                    {
                        Seed = seed,
                        Subnet = new IPNetwork2(IPAddress.Parse("192.168.0.0"), 24),
                        AllowedIPs = "0.0.0.0/0, ::/0",
                        ListenPort = 51820,
                        UseLastAddress = true,
                        UsePresharedKeys = true
                    }
                ).AddPeers(3).Build();

                Print(server);

                foreach(var peer in server.Peers)
                {
                    Print(peer);
                }
            });
        }

        private void Print(WireGuardPeer peer)
        {
            var writer = new WireGuardWriter();

            var ms = new MemoryStream();
            writer.Write(peer, ms);

            string conf = Encoding.UTF8.GetString(ms.ToArray());

            TestContext.Out.WriteLine(conf);
        }
    }
}