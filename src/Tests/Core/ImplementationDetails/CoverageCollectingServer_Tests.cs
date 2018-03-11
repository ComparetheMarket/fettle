using System.Net.Sockets;
using System.Text;
using Fettle.Core.Internal;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Fettle.Tests.Core.ImplementationDetails
{
    class CoverageCollectingServer_Tests
    {
        [Test]
        public void Messages_that_are_sent_are_received()
        {
            var server = new CoverageCollectingServer();
            server.Start();

            var messagesToSend = new[]
            {
                "abc",
                "defg",
                "abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz.abcdefghijklmnopqrstuvwxyz"
            };

            //foreach (var message in messagesToSend)
            Parallel.ForEach(messagesToSend, message =>
            {
                using (var client = new TcpClient("127.0.0.1", 4444))
                {
                    var messageAsBytes = Encoding.UTF8.GetBytes($"{message}\n");
                    client.GetStream().Write(messageAsBytes, 0, messageAsBytes.Length);
                }
            });

            var messagesReceived = server.PopReceived();
            
            server.Stop();

            Assert.That(messagesReceived, Is.EquivalentTo(messagesToSend));
        }
    }
}
