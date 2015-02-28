using System;
using System.IO;
using System.IO.Pipes;
using System.ServiceModel;
using System.Threading.Tasks;

namespace IPCTestingServer
{
    [ServiceContract]
    public interface IStringReverser
    {
        [OperationContract]
        string ReverseString(string value);
    }

    public class StringReverser : IStringReverser
    {
        public string ReverseString(string value)
        {
            var retVal = value.ToCharArray();
            var idx = 0;
            for (var i = value.Length - 1; i >= 0; i--)
                retVal[idx++] = value[i];

            return new string(retVal);
        }
    }

    internal static class Program
    {
        private static readonly StringReverser stringReverser = new StringReverser();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            Console.WriteLine("Server? Bitch I might be.");
            UseStreams();
        }

        private static void UseStreams()
        {
            while (true)
            {
                NamedPipeServerStream stream =
                    new NamedPipeServerStream("reverser", PipeDirection.InOut, -1, PipeTransmissionMode.Byte);

                stream.WaitForConnection();
                Task.Factory.StartNew(() => Process(stream));
            }
        }

        private static void Process(NamedPipeServerStream stream)
        {
            using (stream)
            using (var br = new BinaryReader(stream))
            using (var bw = new BinaryWriter(stream))
            {
                string request;
                while (true)
                {
                    try
                    {
                        request = br.ReadString();
                        var response = stringReverser.ReverseString(request);
                        Console.WriteLine("Client request received: reversing '{0}' produces '{1}'", request, response);
                        bw.Write(response);
                        bw.Flush();
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }    
                }
                Console.WriteLine("Client disconnected.");
            }
        }
    }
}