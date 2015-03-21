using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Fiddler;

namespace FiddlerTesting
{
    static class Program
    {
        static void Main(string[] args)
        {
            //FiddlerApplication.Shutdown();
            //FiddlerApplication.Shutdown();
            Console.WriteLine("First let's kill something. Type the exe name (sans file extension) then press enter to kill it.");

            var procs = Process.GetProcessesByName(Console.ReadLine());
            if (procs.Any())
            {
                procs.First().Kill();
                Console.WriteLine("Suck it, bitch!");
            }
            else
            {
                Console.WriteLine("Couldn't kill that program. Your spelling is bad and you should feel bad.");
            }

            Console.WriteLine("Fiddler will start when you press enter.");
            Console.ReadLine();
           
            FiddlerApplication.SetAppDisplayName("FiddlerTesting");

            // Fiddler.FiddlerApplication.OnNotification += delegate(object sender, NotificationEventArgs oNEA) { Console.WriteLine("** NotifyUser: " + oNEA.NotifyString); };
            // Fiddler.FiddlerApplication.Log.OnLogString += delegate(object sender, LogEventArgs oLEA) { Console.WriteLine("** LogString: " + oLEA.LogString); };
            FiddlerApplication.BeforeRequest += HandleFiddlerBeforeRequest;

            var flags = FiddlerCoreStartupFlags.MonitorAllConnections |
                        FiddlerCoreStartupFlags.RegisterAsSystemProxy |
                        FiddlerCoreStartupFlags.OptimizeThreadPool |
                        FiddlerCoreStartupFlags.ChainToUpstreamGateway;

            FiddlerApplication.Startup(14823, flags);
            Console.WriteLine("Fiddler started.");
            Console.ReadLine();
            FiddlerApplication.Shutdown();
            Console.WriteLine("Fiddler stopped.");
            Console.ReadLine();

            
        }

        private static void HandleFiddlerBeforeRequest(Session s)
        {
            if (s.fullUrl.Contains("flag"))
            {
                Console.WriteLine("Connection aborted: {0}", s.fullUrl);
                // s.Abort();
                s.utilCreateResponseAndBypassServer();
                s.oResponse.headers.Add("Content-Type", "text/plain");
                s.ResponseBody = Encoding.UTF8.GetBytes("You done fucked up now");
            }
            else
            {
                Console.WriteLine("Connection permitted: {0}", s.fullUrl);
            }
        }
    }
}
