using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticrastinateCore
{
    /// <summary>
    /// Represents a set of rules about which websites and programs to block or allow.
    /// </summary>
    class RuleSet
    {
        public IEnumerable<ProgramRule> BlockedPrograms { get; private set; }
        public IEnumerable<WebsiteRule> AllowedWebsites { get; private set; }
        public IEnumerable<WebsiteRule> BlockedWebsites { get; private set; }
    }
}
