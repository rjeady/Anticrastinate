using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kitchen.Shortcuts;

namespace AnticrastinateCore
{
    public static class ProgramFinder
    {
        private const string ShortcutExtension = ".lnk";
        private static readonly string StartMenuCommonPath
            = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");

        public static IEnumerable<Program> GetPrograms()
        {
            var di = new DirectoryInfo(StartMenuCommonPath);
            return di.EnumerateFiles("*.lnk", SearchOption.AllDirectories)
                .Select(ProgramFromShortcutInfo)
                .Where(p => p != null);
        }

        private static Program ProgramFromShortcutInfo(FileInfo shortcutInfo)
        {
            if (shortcutInfo.Extension != ShortcutExtension)
                return null;

            string name = Path.GetFileNameWithoutExtension(shortcutInfo.Name);

            var shortcut = new ShellShortcut(shortcutInfo.FullName);

            string targetPath = MsiShortcutUtils.IsMsiShortcut(shortcut)
                ? MsiShortcutUtils.GetMsiShortcutTarget(shortcutInfo.FullName)
                : shortcut.TargetPath;

            if (string.IsNullOrEmpty(targetPath))
                return null;

            return new Program(name, targetPath, shortcut.Icon);
        }
    }
}