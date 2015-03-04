using System;
using System.Diagnostics.Contracts;

namespace AnticrastinateCore
{
    class WebsiteRule : IEquatable<WebsiteRule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteRule"/> class.
        /// </summary>
        /// <param name="host">The hostname.</param>
        /// <param name="path">The path.</param>
        /// <exception cref="System.ArgumentNullException">
        /// host or path arguments are null.
        /// </exception>
        public WebsiteRule(string host, string path)
        {   
            Contract.Requires(host != null);
            Contract.Requires(path != null);

            Host = host;
            Path = path;
        }

        /// <summary>
        /// The hostname part of the URL to block, for example "www.flag-to-flag.com". Does not include a protocol.
        /// A page should be blocked if its hostname ends with this string.
        /// </summary>
        public String Host { get; private set; }

        /// <summary>
        /// The path to be blocked, starting with the character after the first '/', for example "members/rhobium/".
        /// A page should be blocked if its path begins with this string.
        /// If the rule does not depend on the path, should be an empty string.
        /// </summary>
        public String Path { get; private set; }

        #region Value Equality Members

        public bool Equals(WebsiteRule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Host, other.Host) && string.Equals(Path, other.Path);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WebsiteRule);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Host.GetHashCode()*397) ^ Path.GetHashCode();
            }
        }

        #endregion
    }
}