using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fettle.Core.Internal
{
    internal class CoverageCollectingServer
    {
        private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
        
        private readonly List<string> messages = new List<string>();
        private readonly Object lockObject = new Object();

        private readonly Thread receiveThread = new Thread(ReceiveThreadMethod);
        private TcpListener server;

        public void Start()
        {
            server = new TcpListener(IPAddress.Loopback, 4444);
            server.Start();

            receiveThread.Start(this);
        }

        public void Stop()
        {
            server.Stop();
            cancellationSource.Cancel();

            receiveThread.Join(timeout: TimeSpan.FromSeconds(5));
        }

        public string[] PopReceived()
        {
            lock (lockObject)
            {
                var result = messages.ToArray();
                messages.Clear();
                return result;
            }
        }

        private static void ReceiveThreadMethod(object obj)
        {
            var instance = (CoverageCollectingServer)obj;

            while (!instance.cancellationSource.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = instance.server.AcceptTcpClient();
                
                    lock (instance.lockObject)
                    {
                        var receivedMessages = ReadMessagesFromStream(client.GetStream());
                        //receivedMessages.ToList().ForEach(m => Console.WriteLine($"......{m}"));

                        instance.messages.AddRange(receivedMessages);
                    }
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.Interrupted)
                    {
                        // Server was stopped
                        return;
                    }
                    else
                    {
                        throw;
                    }
                }

                
            }
        }

        private static string[] ReadMessagesFromStream(NetworkStream stream)
        {
            var buffer = new byte[32];
            var result = new StringBuilder();

            int numBytesRead;

            while((numBytesRead = stream.Read(buffer, 0, buffer.Length)) != 0) 
            {   
                var chunk = Encoding.UTF8.GetString(buffer, 0, numBytesRead);
                result.Append(chunk);
            }

            return result.ToString().Split(new[]{ '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
