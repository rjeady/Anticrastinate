using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Security;
using System.Xml.Linq;

namespace AnticrastinateCore
{
    internal class RuleSetRepository
    {
        private readonly List<RuleSet> ruleSets = new List<RuleSet>();

        #region XML File, Element & Attribute Names

        private const string FileName = "RuleSets.xml";

        private readonly string filePath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), FileName);

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

        public IEnumerable<RuleSet> RuleSets
        {
            get { return ruleSets; }
        }

        /// <summary>Adds the specified rule set to the repository.</summary>
        /// <param name="ruleSet">The rule set to add.</param>
        /// <exception cref="IOException">An error occurred when creating or writing to the rule sets file.</exception>
        public void Add(RuleSet ruleSet)
        {
            ruleSets.Add(ruleSet);
            Save();
        }

        /// <summary>Update one or more rule sets. Call this method after any rule set objects are modified to persist changes.</summary>
        /// <exception cref="IOException">An error occurred when creating or writing to the rule sets file.</exception>
        /// <exception cref="System.Security.SecurityException">We do not have the required permissions to create the rule sets file.</exception>
        /// <exception cref="UnauthorizedAccessException">We are not permitted to write to a file on the rule sets file path.</exception>
        public void Update()
        {
            Save();
        }

        /// <summary>
        /// Deletes the specified rule set from the repository.
        /// If one of the specified exceptions occurs, the delete operation will be automatically rolled back.
        /// The exception will then be re-thrown for the calling code to handle.
        /// </summary>
        /// <param name="ruleSet">The rule set to delete.</param>
        /// <exception cref="InvalidOperationException">The specified rule set is not present in the repository.</exception>
        /// <exception cref="IOException">An error occurred when creating or writing to the rule sets file.</exception>
        /// <exception cref="System.Security.SecurityException">We do not have the required permissions to create the rule sets file.</exception>
        /// <exception cref="UnauthorizedAccessException">We are not permitted to write to a file on the rule sets file path.</exception>
        public void Delete(RuleSet ruleSet)
        {
            var numRuleSets = ruleSets.Count;
            for (int i = 0; i < numRuleSets; i++)
            {
                if (ruleSets[i] == ruleSet)
                {
                    ruleSets.RemoveAt(i);
                    try
                    {
                        Save();
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException || ex is UnauthorizedAccessException || ex is SecurityException)
                        {
                            // if the save operation fails for an expected reason, rollback our delete operation.
                            ruleSets.Insert(i, ruleSet);
                        }
                        // now re-throw the exception.
                        throw;
                    }
                }
            }
            throw new InvalidOperationException("The specified rule set is not present in the repository.");
        }

        /// <summary>
        /// Loads the rule sets from disk.
        /// If one of the specified exceptions occurs, no rule sets will be loaded,
        /// except for an IOException or XmlException where some rule sets may be loaded correctly.
        /// </summary>
        /// <exception cref="FileNotFoundException">The rule sets file was not found.</exception>
        /// <exception cref="DirectoryNotFoundException">The rule sets file directory was not found.</exception>
        /// <exception cref="IOException">An IO error occurred when reading from the rule sets file.</exception>
        /// <exception cref="System.Xml.XmlException">An error occurred while parsing the rule sets file.</exception>
        /// <exception cref="System.Security.SecurityException">We do not have the required permissions to read from the rule sets file.</exception>
        /// <exception cref="UnauthorizedAccessException">We are not permitted to read from a file on the rule sets file path.</exception>
        public void Load()
        {
            var root = XElement.Load(filePath).Element("RuleSets");
            if (root != null)
            {
                var ruleSetEls = root.Elements(RuleSet);
                foreach (var ruleSetEl in ruleSetEls)
                {
                    String name = (string)ruleSetEl.Attribute(RuleSetName);

                    bool blockAllWebsites; // default to false if attribute isn't present/readable
                    bool.TryParse((string)ruleSetEl.Attribute(BlockAllWebsites), out blockAllWebsites);

                    var bpEl = ruleSetEl.Element(BlockedPrograms);
                    var blockedPrograms = (bpEl == null)
                        ? new List<ProgramRule>()
                        : bpEl.Elements(ProgramRule).Select(ProgramRuleFromXElement).Where(e => e != null).ToList();

                    if (blockAllWebsites)
                    {
                        ruleSets.Add(new RuleSet(name, blockedPrograms));
                    }
                    else
                    {
                        var awEl = ruleSetEl.Element(AllowedWebsites);
                        var allowedWebsites = (awEl == null)
                            ? new List<WebsiteRule>()
                            : awEl.Elements(WebsiteRule).Select(WebsiteRuleFromXElement).Where(e => e != null).ToList();

                        var bwEl = ruleSetEl.Element(BlockedWebsites);
                        var blockedWebsites = (bwEl == null)
                            ? new List<WebsiteRule>()
                            : bwEl.Elements(WebsiteRule).Select(WebsiteRuleFromXElement).Where(e => e != null).ToList();

                        ruleSets.Add(new RuleSet(name, blockedPrograms, allowedWebsites, blockedWebsites));
                    }
                }
            }
        }

        /// <summary>Creates a program rule from the specified XElement.</summary>
        /// <param name="x">The xml element.</param>
        private static ProgramRule ProgramRuleFromXElement(XElement x)
        {
            var name = (string)x.Attribute(ProgramName);
            var path = (string)x.Attribute(ProgramPath) ?? string.Empty;

            if (name != null)
                return new ProgramRule(name, path);
            else
                return null;
        }

        /// <summary>Creates a website rule from the specified XElement.</summary>
        /// <param name="x">The xml element.</param>
        private static WebsiteRule WebsiteRuleFromXElement(XElement x)
        {
            var host = (string)x.Attribute(WebsiteHost);
            var path = (string)x.Attribute(WebsitePath) ?? string.Empty;

            if (host != null)
                return new WebsiteRule(host, path);
            else
                return null;
        }

        /// <summary>Saves the RuleSets to disk.</summary>
        /// <exception cref="IOException">An IO error occurred when creating or writing to the file.</exception>
        /// <exception cref="System.Security.SecurityException">We do not have the required permissions to create the file.</exception>
        /// <exception cref="UnauthorizedAccessException">We are not permitted to write to a file on this path.</exception>
        /// Further exceptions may be raised if the file path is of an invalid format.
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

        /// <summary>Creates an XElement for the specified program rule.</summary>
        /// <param name="rule">The program rule.</param>
        private static XElement XElementFromProgramRule(ProgramRule rule)
        {
            return new XElement(ProgramRule, new XAttribute(ProgramName, rule.Name),
                new XAttribute(ProgramPath, rule.Path));
        }

        /// <summary>Creates an XElement for the specified website rule.</summary>
        /// <param name="rule">The website rule.</param>
        private static XElement XElementFromWebsiteRule(WebsiteRule rule)
        {
            return new XElement(WebsiteRule, new XAttribute(WebsiteHost, rule.Host),
                new XAttribute(WebsitePath, rule.Path));
        }
    }
}