﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fiddler;

namespace AnticrastinateCore
{
    internal class WebsiteRuleEnforcer
    {
        private const int Port = 14823;
        private const FiddlerCoreStartupFlags Flags = 
            FiddlerCoreStartupFlags.CaptureLocalhostTraffic |
            FiddlerCoreStartupFlags.MonitorAllConnections |
            FiddlerCoreStartupFlags.RegisterAsSystemProxy;

        private RuleSet ruleSet;
        private bool anythingBlocked;

        public WebsiteRuleEnforcer(RuleSet ruleSet)
        {
            RuleSet = ruleSet;
            FiddlerApplication.SetAppDisplayName("Anticrastinate");
            FiddlerApplication.Startup(Port, Flags);
        }

        public RuleSet RuleSet
        {
            get
            {
                return ruleSet;
            }
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

            if (MatchWebsite(s, RuleSet.AllowedWebsites))
                return;
            
            if (MatchWebsite(s, RuleSet.BlockedWebsites))
                BlockConnection(s);
        }

        /// <summary>
        /// Returns true if the website url for this session matches any of the given rules.
        /// </summary>
        /// <param name="s">The session.</param>
        /// <param name="rules">The rules.</param>
        private bool MatchWebsite(Session s, IEnumerable<WebsiteRule> rules)
        {
            foreach (var rule in rules)
            {
                bool noPathRule = rule.Path == "";
                if (s.hostname.EndsWith(rule.Host))
                {
                    if (noPathRule)
                    {
                        return true;
                    }
                    else
                    {
                        int pathStart = s.url.IndexOf("/", StringComparison.Ordinal);
                        if (pathStart >= 0 && s.url.Substring(pathStart + 1).StartsWith(rule.Path))
                            return true;
                    }
                }
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
