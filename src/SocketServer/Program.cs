using Common;
using ProtoBuf;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TcpEcho
{
    class Program
    {
        private static bool _echo;
        private static int BufferSize = 2048;


        static async Task Main(string[] args)
        {
            _echo = args.FirstOrDefault() == "echo";

            foreach (string arg in args)
            {
                if (int.TryParse(arg, out BufferSize))
                {
                    break;
                }
            }

            var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 8087));

            Console.WriteLine("Listening on port 8087");

            listenSocket.Listen(120);

            while (true)
            {
                var socket = await listenSocket.AcceptAsync();
                _ = ProcessLinesAsync(socket);
            }
        }

        private static async Task ProcessLinesAsync(Socket socket)
        {
            Console.WriteLine($"[{socket.RemoteEndPoint}]: connected");
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[BufferSize]);

            try
            {
                while (true)
                {
                    using (var stream = new MemoryStream())
                    {
                        do
                        {
                            var result = await socket.ReceiveAsync(buffer, SocketFlags.None);
                            stream.Write(buffer.Array, buffer.Offset, result);
                        }
                        while (stream.Length < BufferSize);
                        var person = Serializer.Deserialize<Person>(stream);
                    }
                }
            }
            catch (Exception ex)
            { Console.WriteLine(ex); }
            Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
        }

        private static void ProcessLine(Socket socket, string s)
        {
            if (_echo)
            {
                Console.Write($"[{socket.RemoteEndPoint}]: ");
                Console.WriteLine(s);
            }
        }
    }
}
