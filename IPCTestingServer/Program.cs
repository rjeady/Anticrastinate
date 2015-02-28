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
        private static StringReverser stringReverser = new StringReverser();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            Console.WriteLine("Server? Bitch I might be.");

            // UseWCF();
            UseStreams();
        }

        private static void UseWCF()
        {
            using (var host = new ServiceHost(typeof (StringReverser), new Uri("net.pipe://localhost")))
            {
                host.AddServiceEndpoint(typeof (IStringReverser), new NetNamedPipeBinding(), "PipeReverse");

                host.Open();

                Console.WriteLine("Service is available. Press <ENTER> to exit.");
                Console.ReadLine();

                host.Close();
            }
        }

        private static void UseStreams()
        {
            while (true)
            {
                NamedPipeServerStream stream =
                    new NamedPipeServerStream("reversi2", PipeDirection.InOut, -1, PipeTransmissionMode.Byte);

                stream.WaitForConnection();
                Task.Factory.StartNew(() => Process(stream));
            }
        }

        private static void Process(NamedPipeServerStream stream)
        {
            using (stream)
            using (var sr = new StreamReader(stream))
            using (var sw = new StreamWriter(stream) {AutoFlush = true})
            {
                string request;
                while ((request = sr.ReadLine()) != null)
                {
                    var response = stringReverser.ReverseString(request);
                    sw.WriteLine(response);
                }
                Console.WriteLine("Client disconnected.");
            }
        }
    }
}