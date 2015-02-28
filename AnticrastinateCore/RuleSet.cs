using System;
using System.Collections.Generic;
using System.Linq;

namespace AnticrastinateCore
{
    /// <summary>
    /// Represents a set of rules about which websites and programs to block or allow.
    /// </summary>
    class RuleSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleSet"/> class.
        /// </summary>
        /// <param name="name">The RuleSet name.</param>
        /// <param name="blockedPrograms">The blocked program rules.</param>
        /// <param name="allowedWebsites">The allowed website rules.</param>
        /// <param name="blockedWebsites">The blocked website rules.</param>
        public RuleSet(string name, IEnumerable<ProgramRule> blockedPrograms,
            IEnumerable<WebsiteRule> allowedWebsites, IEnumerable<WebsiteRule> blockedWebsites)
        {
            Name = name;
            BlockedPrograms = blockedPrograms;
            AllowedWebsites = allowedWebsites;
            BlockedWebsites = blockedWebsites;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleSet"/> class,
        /// with either all websites allowed or all websites blocked.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="blockedPrograms">The blocked program rules.</param>
        /// <param name="blockAllWebsites">if set to <c>true</c>, all websites will be blocked.</param>
        public RuleSet(string name, IEnumerable<ProgramRule> blockedPrograms, bool blockAllWebsites)
        {
            Name = name;
            BlockedPrograms = blockedPrograms;
            BlockedWebsites = AllowedWebsites = Enumerable.Empty<WebsiteRule>();
            BlockAllWebsites = blockAllWebsites;
        }

        public String Name { get; private set; }

        public IEnumerable<ProgramRule> BlockedPrograms { get; private set; }
        public IEnumerable<WebsiteRule> AllowedWebsites { get; private set; }
        public IEnumerable<WebsiteRule> BlockedWebsites { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to block all websites. If <c>true</c>,
        /// AllowedWebsites and BlockedWebsites lists should be ignored.
        /// </summary>
        public bool BlockAllWebsites { get; private set; }
    }
}
