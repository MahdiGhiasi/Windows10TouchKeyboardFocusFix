using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Windows10TouchKeyboardFocusFix
{
    internal static class StartupHelper
    {
        private static string AppName => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        public static void AddToStartup()
        {
            var executablePath = Assembly.GetEntryAssembly().Location;
            AddToStartup(AppName, executablePath);
        }

        public static void RemoveFromStartup()
        {
            RemoveFromStartup(AppName);
        }

        public static bool IsRegisteredToRunAtStartup()
        {
            return IsRegisteredToRunAtStartup(AppName);
        }

        /// <summary>
        /// Add application to Startup of windows
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="path"></param>
        private static void AddToStartup(string appName, string path)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue(appName, "\"" + path + "\"");
            }
        }

        /// <summary>
        /// Remove application from Startup of windows
        /// </summary>
        /// <param name="appName"></param>
        private static void RemoveFromStartup(string appName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.DeleteValue(appName, false);
            }
        }

        /// <summary>
        /// Checks if application is registered to run at startup
        /// </summary>
        /// <param name="appName"></param>
        /// <returns></returns>
        private static bool IsRegisteredToRunAtStartup(string appName)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                var value = key.GetValue(appName);
                return value != null;
            }
        }
    }
}
