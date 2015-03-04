﻿using System;
using System.Diagnostics.Contracts;

namespace AnticrastinateCore
{
    class ProgramRule : IEquatable<ProgramRule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramRule"/> class.
        /// </summary>
        /// <param name="name">The file name of the executable.</param>
        /// <param name="path">
        /// The path to the executable,
        /// or an empty string if all programs of the same name are to be blocked.
        /// </param>
        public ProgramRule(string name, string path)
        {
            Contract.Requires(name != null);
            Contract.Requires(path != null);

            Name = name;
            Path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramRule"/> class.
        /// </summary>
        /// <param name="path">
        /// The full path to the executable,
        /// or just its file name if all programs of the same name are to be blocked.
        /// </param>
        public ProgramRule(string path)
        {
            Contract.Requires(path != null);

            int finalSlashPos = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
            if (finalSlashPos == -1)
            {
                // path has no backslashes so it is just a file name.
                Name = path;
                Path = null;
            }
            else
            {
                Path = path;
                Name = path.Substring(finalSlashPos + 1);
            }
        }

        /// <summary>
        /// The name of the program executable, including the file extension.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The complete file path of the program. May be null if all programs of the same name are to be blocked.
        /// </summary>
        public string Path { get; private set; }

        #region Value Equality Members
        
        public bool Equals(ProgramRule other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProgramRule);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.OrdinalIgnoreCase.GetHashCode(Name)*397) ^ (Path != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Path) : 0);
            }
        }

        #endregion
    }
}