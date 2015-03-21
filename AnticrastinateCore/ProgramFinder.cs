using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using IWshRuntimeLibrary;

namespace AnticrastinateCore
{
    public class Program
    {
        public Program(string name, string filePath, string iconPath)
        {
            Name = name;
            FilePath = filePath;
            IconPath = iconPath;
        }

        public string Name { get; private set; }
        public string FilePath { get; private set; }
        public string IconPath { get; private set; }

    }

    public static class ProgramFinder
    {
        public static IEnumerable<Program> GetPrograms()
        {
            string startMenuFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);

            var di = new DirectoryInfo(Path.Combine(startMenuFolder, "Programs"));
            var files = di.EnumerateFiles();

            foreach (FileInfo file in files)
            {
                Program program = ProgramFromShortcutInfo(file);
                if (program != null)
                    yield return program;
            }
        }

        private static Program ProgramFromShortcutInfo(FileInfo shortcutInfo)
        {
            if (shortcutInfo.Extension != ".lnk")
                return null;

            string name = Path.GetFileNameWithoutExtension(shortcutInfo.Name);
            string targetPath, iconPath;

            var shell = new WshShell();
            try
            {
                var shortcut = (IWshShortcut)shell.CreateShortcut(shortcutInfo.FullName);
                targetPath = shortcut.TargetPath;
                iconPath = shortcut.IconLocation;
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                return null;
            }

            return new Program(name, targetPath, iconPath);
        }
    }

    public static class MsiShortcutUtils
    {
        private static string GetMsiShortcutTarget(string filePath)
        {
            var product = new StringBuilder(MaxGuidLength + 1);
            var component = new StringBuilder(MaxGuidLength + 1);

            if (MsiGetShortcutTarget(filePath, product, null, component) != MsiResult.Success)
                return null;

            int pathLength = MaxPathLength;
            var path = new StringBuilder(pathLength);

            var installState = MsiGetComponentPath(product.ToString(), component.ToString(), path, ref pathLength);
            
            return installState == InstallState.Local ? path.ToString() : null;
        }

        [DllImport("msi.dll", CharSet = CharSet.Auto)]
        private static extern MsiResult MsiGetShortcutTarget(
            string targetFile,
            StringBuilder productCode,
            StringBuilder featureID,
            StringBuilder componentCode);

        [DllImport("msi.dll", CharSet = CharSet.Auto)]
        private static extern InstallState MsiGetComponentPath(
            string productCode,
            string componentCode,
            StringBuilder componentPath,
            ref int componentPathBufferSize);

        private const int MaxGuidLength = 38;
        private const int MaxPathLength = 1024;

        private enum InstallState
        {
            NotUsed = -7,
            BadConfig = -6,
            Incomplete = -5,
            SourceAbsent = -4,
            MoreData = -3,
            InvalidArg = -2,
            Unknown = -1,
            Broken = 0,
            Advertised = 1,
            Removed = 1,
            Absent = 2,
            Local = 3,
            Source = 4,
            Default = 5
        }

        private enum MsiResult : uint
        {
            Success = 0,
            Error = 1603, // occurs when called from a non-STA thread.
            Failed = 1627
        }
    }
}