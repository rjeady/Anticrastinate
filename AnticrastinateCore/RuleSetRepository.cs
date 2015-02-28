using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Xml.Linq;

namespace AnticrastinateCore
{
    class RuleSetRepository
    {
        private List<RuleSet> ruleSets;

        private const string FileName = "RuleSets.xml";
        private readonly string filePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FileName);

        #region XML Element & Attribute Names

        private const string RuleSet = "RuleSet";
        private const string RuleSetName = "Name";
        private const string BlockAllWebsites = "BlockAllWebsites";

        private const string BlockedPrograms = "BlockedPrograms";
        private const string AllowedWebsites = "AllowedWebsites";
        private const string BlockedWebsites = "BlockedWebsites";

        private const string ProgramRule = "ProgramRule";
        private const string WebsiteRule = "WebsiteRule";

        private const string ProgramName = "Name";
        private const string ProgramPath = "Path";

        private const string WebsiteHost = "Host";
        private const string WebsitePath = "Path";

        #endregion
        
        public RuleSetRepository()
        {
           ruleSets = LoadRuleSets().ToList();
        }

        private IEnumerable<RuleSet> LoadRuleSets()
        {
            var root = XElement.Load(filePath).Element("RuleSets");

            if (root != null)
            {
                var ruleSetEls = root.Elements(RuleSet);
                foreach (var ruleSetEl in ruleSetEls)
                {
                    String name = (string)ruleSetEl.Attribute(RuleSetName);
                    bool blockAllWebsites = bool.Parse((string)ruleSetEl.Attribute(BlockAllWebsites));

                    IEnumerable<ProgramRule> blockedPrograms = Enumerable.Empty<ProgramRule>();

                    var bpEl = ruleSetEl.Element(BlockedPrograms);
                    if (bpEl != null)
                    {
                        blockedPrograms = bpEl.Elements(ProgramRule).Select(ProgramRuleFromXElement).Where(e => e != null);
                    }

                    if (blockAllWebsites)
                    {
                        yield return new RuleSet(name, blockedPrograms);
                    }
                    else
                    {
                        IEnumerable<WebsiteRule> allowedWebsites = Enumerable.Empty<WebsiteRule>();
                        IEnumerable<WebsiteRule> blockedWebsites = Enumerable.Empty<WebsiteRule>();

                        var awEl = ruleSetEl.Element(AllowedWebsites);
                        if (awEl != null)
                        {
                            allowedWebsites = awEl.Elements(WebsiteRule).Select(WebsiteRuleFromXElement).Where(e => e != null);
                        }
                        var bwEl = ruleSetEl.Element(BlockedWebsites);
                        if (bwEl != null)
                        {
                            blockedWebsites = bwEl.Elements(WebsiteRule).Select(WebsiteRuleFromXElement).Where(e => e != null);
                        }

                        yield return new RuleSet(name, blockedPrograms, allowedWebsites, blockedWebsites);
                    }                  
                }
            }
        }

        private ProgramRule ProgramRuleFromXElement(XElement x)
        {
            var name = (string)x.Attribute(ProgramName);
            var path = (string)x.Attribute(ProgramPath) ?? string.Empty;

            if (name != null)
                return new ProgramRule(name, path);
            else
                return null;
        }

        private WebsiteRule WebsiteRuleFromXElement(XElement x)
        {
            var host = (string)x.Attribute(WebsiteHost);
            var path = (string)x.Attribute(WebsitePath) ?? string.Empty;

            if (host != null)
                return new WebsiteRule(host, path);
            else 
                return null;
        }



        private void Save()
        {
            var doc = new XElement("RuleSets",
                RuleSets.Select(r => new XElement(RuleSet,
                        new XAttribute(RuleSetName, r.Name), new XAttribute(BlockAllWebsites, r.BlockAllWebsites),
                        new XElement(BlockedPrograms, 
                            r.BlockedPrograms.Select(XElementFromProgramRule)),
                        new XElement(AllowedWebsites,
                            r.AllowedWebsites.Select(XElementFromWebsiteRule)),
                        new XElement(BlockedWebsites,
                            r.BlockedWebsites.Select(XElementFromWebsiteRule)))));
            doc.Save(filePath);
        }

        private XElement XElementFromProgramRule(ProgramRule rule)
        {
            return new XElement(ProgramRule, new XAttribute(ProgramName, rule.Name), new XAttribute(ProgramPath, rule.Path));
        }

        private XElement XElementFromWebsiteRule(WebsiteRule rule)
        {
            return new XElement(WebsiteRule, new XAttribute(WebsiteHost, rule.Host), new XAttribute(WebsitePath, rule.Path));
        }

        public IEnumerable<RuleSet> RuleSets
        {
            get
            {
                return ruleSets;
            }
        }
    }
}
