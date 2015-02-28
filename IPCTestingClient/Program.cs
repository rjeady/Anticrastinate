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
            Console.WriteLine("Type in a string and press enter to reverse it.");

            UseStreams();
        }


        private static void UseStreams()
        {
            Sw.Restart();
            using (var clientStream = new NamedPipeClientStream("reverser"))
            {
                clientStream.Connect();
                using (BinaryReader br = new BinaryReader(clientStream))
                using (BinaryWriter bw = new BinaryWriter(clientStream))
                {
                    Sw.Stop();
                    Console.WriteLine("Setup time: {0}ms", 1000 * (double)Sw.ElapsedTicks / Stopwatch.Frequency);

                    string str;
                    while ((str = Console.ReadLine()) != "")
                    {
                        Console.WriteLine("response took {0}ms", Time(Sw, () =>
                        {
                            bw.Write(str);
                            bw.Flush();
                            Console.WriteLine("reversed: {0}", br.ReadString());
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