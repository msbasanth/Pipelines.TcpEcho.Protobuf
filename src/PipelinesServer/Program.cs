using Common;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TcpEcho
{
    class Program
    {
        private static bool _echo;
        private static int BufferSize = 1024;
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

            var pipe = new Pipe();
            Task writing = FillPipeAsync(socket, pipe.Writer);
            Task reading = ReadPipeAsync(socket, pipe.Reader);

            await Task.WhenAll(reading, writing);

            Console.WriteLine($"[{socket.RemoteEndPoint}]: disconnected");
        }

        private static async Task FillPipeAsync(Socket socket, PipeWriter writer)
        {
            while (true)
            {
                try
                {
                    Memory<byte> memory = writer.GetMemory(BufferSize);
                    int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw ex;
                }
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }

            // Signal to the reader that we're done writing
            writer.Complete();
        }

        private static async Task ReadPipeAsync(Socket socket, PipeReader reader)
        {
            ReadOnlySequence<byte> buffer=new ReadOnlySequence<byte>();
            try
            {
                while (true)
                {
                    ReadResult result = await reader.ReadAsync();
                    buffer = result.Buffer;
                    while(buffer.Length >= BufferSize)
                    {
                        var line = buffer.Slice(0, BufferSize);
                        ProcessLine(socket, line);
                        buffer = buffer.Slice(BufferSize);
                    }

                    //We sliced the buffer until no more data could be processed
                    //Tell the PipeReader how much we consumed and how much we left to process
                    reader.AdvanceTo(buffer.Start, buffer.End);

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("Buffer Length: {0}", buffer.Length);
                Console.WriteLine(exc);
            }
            reader.Complete();
        }

        private static void ProcessLine(Socket socket, in ReadOnlySequence<byte> buffer)
        {
            var person = Deserialize(buffer);
            if (_echo)
            {
                Console.Write($"[{socket.RemoteEndPoint}]: ");
                foreach (var segment in buffer)
                {
#if NETCOREAPP2_1
                Console.Write(Encoding.UTF8.GetString(segment.Span));
#else
                    Console.Write(Encoding.UTF8.GetString(segment));
#endif
                }
                Console.WriteLine();
            }
        }

        private static Person Deserialize(ReadOnlySequence<byte> buffer)
        {
            var reader = ProtoReader.Create(out ProtoReader.State state, buffer, RuntimeTypeModel.Default, new SerializationContext());
            var data = new Person();

            int header = 0;
            while ((header = reader.ReadFieldHeader(ref state)) > 0)
            {
                switch (header)
                {
                    case 1:
                        data.Name = reader.ReadString(ref state);
                        break;
                    default:
                        reader.SkipField(ref state);
                        break;
                }
            }

            return data;
        }
    }

#if NET47
    internal static class Extensions
    {
        public static Task<int> ReceiveAsync(this Socket socket, Memory<byte> memory, SocketFlags socketFlags)
        {
            var arraySegment = GetArray(memory);
            return SocketTaskExtensions.ReceiveAsync(socket, arraySegment, socketFlags);
        }

        public static string GetString(this Encoding encoding, ReadOnlyMemory<byte> memory)
        {
            var arraySegment = GetArray(memory);
            return encoding.GetString(arraySegment.Array, arraySegment.Offset, arraySegment.Count);
        }

        private static ArraySegment<byte> GetArray(Memory<byte> memory)
        {
            return GetArray((ReadOnlyMemory<byte>)memory);
        }

        private static ArraySegment<byte> GetArray(ReadOnlyMemory<byte> memory)
        {
            if (!MemoryMarshal.TryGetArray(memory, out var result))
            {
                throw new InvalidOperationException("Buffer backed by array was expected");
            }

            return result;
        }
    }
#endif
}
