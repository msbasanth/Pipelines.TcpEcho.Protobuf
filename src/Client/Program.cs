using Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpEcho
{
    class Program
    {
        private static IDictionary<int, int> messageSizeMap = new Dictionary<int, int>() { { 32, 30 }, { 128, 126 }, { 512, 509}, { 1024, 1021}, { 2048, 2045}, { 4096, 4093}, { 8192, 8189}, { 10000, 9997},
                { 100000, 99996}, {1000000, 999996}, { 10000000, 9999995 } };
        static async Task Main(string[] args)
        {
            string messageSize = args.FirstOrDefault();

            

            var clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            Console.WriteLine("Connecting to port 8087");

            clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 8087));

            if (messageSize == null)
            {
                var buffer = new byte[1];
                while (true)
                {

                    buffer[0] = (byte)Console.Read();
                    await clientSocket.SendAsync(new ArraySegment<byte>(buffer, 0, 1), SocketFlags.None);
                }
            }
            else
            {
                try
                {
                    var count = int.Parse(messageSize);
                    var buffer = GetPersonBytes(messageSizeMap[count]);
                    //var buffer = Encoding.ASCII.GetBytes(new string('a', count) + Environment.NewLine);
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();
                    for (int i = 0; i < 1_000_00; i++)
                    {
                        await clientSocket.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    }
                    stopwatch.Stop();

                    Console.WriteLine($"Elapsed {stopwatch.Elapsed.TotalSeconds:F} sec.");
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            Console.ReadLine();
        }

        private static byte[] GetPersonBytes(int count)
        {
            var person = new Person
            {
                Name = new string('a', count)
            };
            byte[] streamArray;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, person);
                stream.Position = 0;
                streamArray = stream.ToArray();
                return streamArray;
            }
        }
    }
}
