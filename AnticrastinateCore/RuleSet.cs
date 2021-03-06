﻿using System;
using System.Collections.Generic;

namespace AnticrastinateCore
{
    /// <summary>
    /// Represents a set of rules about which websites and programs to block or allow.
    /// </summary>
    internal class RuleSet
    {
        /// <summary>Initializes a new instance of the <see cref="RuleSet" /> class.</summary>
        /// <param name="name">The rule set name.</param>
        /// <param name="blockedPrograms">A list of blocked program rules.</param>
        /// <param name="allowedWebsites">A list of allowed website rules.</param>
        /// <param name="blockedWebsites">A list of blocked website rules.</param>
        public RuleSet(string name, IList<ProgramRule> blockedPrograms,
            IList<WebsiteRule> allowedWebsites, IList<WebsiteRule> blockedWebsites)
        {
            Name = name;
            BlockedPrograms = blockedPrograms;
            AllowedWebsites = allowedWebsites;
            BlockedWebsites = blockedWebsites;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleSet"/> class, with all websites blocked.
        /// </summary>
        /// <param name="name">The rule set name.</param>
        /// <param name="blockedPrograms">The blocked program rules.</param>
        public RuleSet(string name, IList<ProgramRule> blockedPrograms)
        {
            Name = name;
            BlockedPrograms = blockedPrograms;
            BlockedWebsites = new List<WebsiteRule>();
            AllowedWebsites = new List<WebsiteRule>();
            BlockAllWebsites = true;
        }

        public String Name { get; private set; }

        public IList<ProgramRule> BlockedPrograms { get; private set; }
        public IList<WebsiteRule> AllowedWebsites { get; private set; }
        public IList<WebsiteRule> BlockedWebsites { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to block all websites. If <c>true</c>,
        /// AllowedWebsites and BlockedWebsites lists should be ignored.
        /// </summary>
        public bool BlockAllWebsites { get; private set; }
    }
}