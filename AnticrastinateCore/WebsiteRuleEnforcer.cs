using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Fiddler;

namespace AnticrastinateCore
{
    internal class WebsiteRuleEnforcer
    {
        private const int FiddlerPort = 14823;
        private const FiddlerCoreStartupFlags FiddlerFlags = 
            FiddlerCoreStartupFlags.CaptureLocalhostTraffic |
            FiddlerCoreStartupFlags.MonitorAllConnections |
            FiddlerCoreStartupFlags.RegisterAsSystemProxy;

        private RuleSet ruleSet;
        private bool anythingBlocked;

        public WebsiteRuleEnforcer(RuleSet ruleSet)
        {
            RuleSet = ruleSet;
            FiddlerApplication.SetAppDisplayName("Anticrastinate");
            FiddlerApplication.Startup(FiddlerPort, FiddlerFlags);
        }

        public RuleSet RuleSet
        {
            get { return ruleSet; }
            set
            {
                if (ruleSet != value)
                {
                    ruleSet = value;
                    AnythingBlocked = ruleSet.BlockedWebsites.Any();
                }
            }
        }

        private bool AnythingBlocked
        {
            set
            {
                if (!anythingBlocked && value)
                    FiddlerApplication.BeforeRequest += HandleFiddlerBeforeRequest;
                else if (anythingBlocked && !value)
                    FiddlerApplication.BeforeRequest -= HandleFiddlerBeforeRequest;
                anythingBlocked = value;
            }
        }

        private void HandleFiddlerBeforeRequest(Session s)
        {
            if (RuleSet.BlockAllWebsites)
                BlockConnection(s);

            int pathStart = s.url.IndexOf("/", StringComparison.Ordinal);
            // path is empty if URL (without protocol) ends in '/' or does not contain a '/'.
            // empty path will only be matched by a rule with an empty path.
            string path = pathStart == -1 ? "" : s.url.Substring(pathStart + 1);

            if (MatchWebsite(s.hostname, path, RuleSet.AllowedWebsites))
                return;

            if (MatchWebsite(s.hostname, path, RuleSet.BlockedWebsites))
                BlockConnection(s);
        }

        /// <summary>
        /// Returns true if the website url for this session matches any of the given rules.
        /// </summary>
        /// <param name="hostName">The hostname.</param>
        /// <param name="path">The path.</param>
        /// <param name="rules">The rules.</param>
        /// <returns></returns>
        private bool MatchWebsite(String hostName, String path, IEnumerable<WebsiteRule> rules)
        {
            // TODO: compare performance to using AsParallel()
            foreach (var rule in rules)
            {
                if (hostName.EndsWith(rule.Host) && path.StartsWith(rule.Path))
                    return true;
            }
            return false;
        }

        private void BlockConnection(Session s)
        {
            // TODO: make a proper response. See https://reqrypt.org/samples/webfilter.html
            s.utilCreateResponseAndBypassServer();
            s.oResponse.headers.Add("Content-Type", "text/plain");
            s.ResponseBody = Encoding.UTF8.GetBytes("You done fucked up now");
        }

        ~WebsiteRuleEnforcer()
        {
            // TODO: when implemented as a service, this must be called when the service stops.
            FiddlerApplication.Shutdown();
        }
}
}
