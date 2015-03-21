using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnticrastinateCore
{
    class ProgramFinder
    {
        public static IEnumerable<string> GetPrograms()
        {
            string commonFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            IEnumerable<string> files = Directory.EnumerateFiles(commonFolder);

            return files;

        } 
    }
}
