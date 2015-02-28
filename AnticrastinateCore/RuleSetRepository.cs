using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AnticrastinateCore
{
    class RuleSetRepository
    {
        // private List<RuleSet> ruleSets;

        private const string FileName = "RuleSets.xml";
        private readonly string filePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FileName);

        public RuleSetRepository()
        {
            Load();
        }

        private void Load()
        {
            
        }

        private void Save()
        {
            var doc = new XElement("RuleSets",
                RuleSets.Select(r => new XElement("RuleSet",
                        new XAttribute("Name", r.Name), new XAttribute("BlockAllWebsites", r.BlockAllWebsites),
                        new XElement("BlockedPrograms", 
                            r.BlockedPrograms.Select(MakeProgramRuleElement)),
                        new XElement("AllowedWebsites",
                            r.AllowedWebsites.Select(MakeWebsiteRuleElement)),
                        new XElement("BlockedWebsites",
                            r.BlockedWebsites.Select(MakeWebsiteRuleElement)))));
            doc.Save(filePath);
        }

        private XElement MakeProgramRuleElement(ProgramRule rule)
        {
            return new XElement("ProgramRule", new XAttribute("Name", rule.Name), new XAttribute("Path", rule.Path));
        }

        private XElement MakeWebsiteRuleElement(WebsiteRule rule)
        {
            return new XElement("WebsiteRule", new XAttribute("Host", rule.Host), new XAttribute("Path", rule.Path));
        }

        public IEnumerable<RuleSet> RuleSets
        {
            get
            {
                yield break;
                
            }
        }
    }
}
