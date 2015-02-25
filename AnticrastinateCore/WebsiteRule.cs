using System;

namespace AnticrastinateCore
{
    class WebsiteRule
    {
        public WebsiteRule(string host, string path)
        {
            Host = host;
            Path = path;
        }

        /// <summary>
        /// The hostname part of the URL to block, for example "www.flag-to-flag.com". Does not include a protocol.
        /// A page should be blocked if its hostname ends with this string.
        /// </summary>
        public String Host { get; private set; }

        /// <summary>
        /// The path to be blocked, starting with a '/' character, for example "/members/rhobium/".
        /// A page should be blocked if its path begins with this string.
        /// If the rule does not depend on the path, should be an empty string.
        /// </summary>
        public String Path { get; private set; }
        
    }
}