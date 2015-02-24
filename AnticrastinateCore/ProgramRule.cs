using System;

namespace AnticrastinateCore
{
    class ProgramRule
    {
        /// <summary>
        /// Initializes a new instance of the ProgramRule class.
        /// </summary>
        /// <param name="path">
        /// The full path to the executable,
        /// or just its file name if all programs of the same name are to be blocked.
        /// </param>
        public ProgramRule(string path)
        {
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
        public String Name { get; }

        /// <summary>
        /// The complete file path of the program. May be null if all programs of the same name are to be blocked.
        /// </summary>
        public String Path { get; }

    }
}