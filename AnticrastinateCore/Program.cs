using System.Drawing;

namespace AnticrastinateCore
{
    public class Program
    {
        public Program(string name, string filePath, Icon icon)
        {
            Name = name;
            FilePath = filePath;
            Icon = icon;
        }

        public string Name { get; private set; }
        public string FilePath { get; private set; }
        public Icon Icon { get; private set; }

    }
}