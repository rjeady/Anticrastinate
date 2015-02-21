using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fiddler;

namespace FiddlerTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Fiddler.FiddlerApplication.Shutdown();
            Fiddler.FiddlerApplication.Shutdown();
            Fiddler.FiddlerApplication.Shutdown();

            Console.WriteLine("Fiddler starting.");
           

            Fiddler.FiddlerApplication.SetAppDisplayName("FiddlerTesting");

           //  Fiddler.FiddlerApplication.OnNotification += delegate(object sender, NotificationEventArgs oNEA) { Console.WriteLine("** NotifyUser: " + oNEA.NotifyString); };
            // Fiddler.FiddlerApplication.Log.OnLogString += delegate(object sender, LogEventArgs oLEA) { Console.WriteLine("** LogString: " + oLEA.LogString); };
            Fiddler.FiddlerApplication.BeforeRequest += s =>
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
            };

            var flags = FiddlerCoreStartupFlags.CaptureLocalhostTraffic |
                        FiddlerCoreStartupFlags.MonitorAllConnections |
                        FiddlerCoreStartupFlags.RegisterAsSystemProxy;

            Fiddler.FiddlerApplication.Startup(14823, flags);
            Console.WriteLine("Fiddler started.");
            Console.ReadLine();
            Fiddler.FiddlerApplication.Shutdown();
            Console.WriteLine("Fiddler stopped.");
            Console.ReadLine();
        }
    }
}
