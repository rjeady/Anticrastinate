using System;

namespace AnticrastinateCore
{
    class ProgramRule
    {
        public ProgramRule(string name, string path)
        {
            Name = name;
            Path = path;
        }

        /// <summary>
        /// The name of the program executable, including the file extension.
        /// </summary>
        public String Name { get; }

        /// <summary>
        /// The complete file path of the program. May be null if all programs of the same name are to be blocked.
        /// </summary>
        public String Path { get; }

    }
}