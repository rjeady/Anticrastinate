using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace AnticrastinateCore
{
    class ProgramRule : IEquatable<ProgramRule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramRule"/> class.
        /// </summary>
        /// <param name="name">The file name of the executable.</param>
        public ProgramRule(string name)
        {
            Contract.Requires(name != null);
            Name = name;
        }

        /// <summary>
        /// The name of the program executable, including the file extension.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The name of the program executable, without the file extension.
        /// </summary>
        public string NameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(Name); }
        }

        #region Value Equality Members

        public bool Equals(ProgramRule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProgramRule);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        #endregion
    }
}