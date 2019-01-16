using Squirrel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Windows10TouchKeyboardFocusFix
{
    internal static class SquirrelUpdateManagerExtension
    {
        public static void CreateOnlyStartMenuShortcutForThisExe(this IUpdateManager This)
        {
            This.CreateShortcutsForExecutable(Path.GetFileName(
                Assembly.GetEntryAssembly().Location),
                ShortcutLocation.StartMenu,
                Environment.CommandLine.Contains("squirrel-install") == false,
                null, null);
        }

        public static void RemoveOnlyStartMenuShortcutForThisExe(this IUpdateManager This)
        {
            This.RemoveShortcutsForExecutable(
                Path.GetFileName(Assembly.GetEntryAssembly().Location),
                ShortcutLocation.StartMenu);
        }
    }
}
