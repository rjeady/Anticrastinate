using System;
using AnticrastinateCore;

namespace AnticrastinateTesting
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Here are the files:");
            Console.WriteLine();

            foreach (var program in ProgramFinder.GetPrograms())
            {
                Console.WriteLine("name: {0}, path: {1}, icon: {2}", program.Name, program.FilePath, program.IconPath);   
            }
            Console.ReadLine();
        }
    }
}
