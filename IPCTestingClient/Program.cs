using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.ServiceModel;

namespace IPCTestingClient
{
    [ServiceContract]
    public interface IStringReverser
    {
        [OperationContract]
        string ReverseString(string value);
    }

    internal static class Program
    {
        private static readonly Stopwatch Sw = new Stopwatch();

        private static void Main(string[] args)
        {
            Console.WriteLine("Client? Bitch I might be.");

            // UseWcf();
            UseStreams();
        }

        private static void UseWcf()
        {
            ChannelFactory<IStringReverser> pipeFactory = new ChannelFactory<IStringReverser>(
                new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/PipeReverse"));

            IStringReverser pipeProxy = null;
            Console.WriteLine("Create channel time: {0}ms", Time(Sw, () => { pipeProxy = pipeFactory.CreateChannel(); }));

            while (true)
            {
                string str = Console.ReadLine();
                Console.WriteLine("response took {0}ms", Time(Sw, () => Console.WriteLine("reversed: {0}", pipeProxy.ReverseString(str))));
            }
        }

        private static void UseStreams()
        {
            using (var clientStream = new NamedPipeClientStream("reversi2"))
            {
                clientStream.Connect();
                using (StreamReader sr = new StreamReader(clientStream))
                using (StreamWriter sw = new StreamWriter(clientStream) {AutoFlush = true})
                {
                    string str;
                    while ((str = Console.ReadLine()) != "")
                    {
                        Console.WriteLine("response took {0}ms", Time(Sw, () =>
                        {
                            sw.WriteLine(str);
                            Console.WriteLine("reversed: {0}", sr.ReadLine());
                        }));
                    }
                }    
            }
        }

        private static double Time(Stopwatch sw, Action a)
        {
            sw.Restart();
            a();
            sw.Stop();
            return 1000 * (double)sw.ElapsedTicks / Stopwatch.Frequency;
        }
    }
}