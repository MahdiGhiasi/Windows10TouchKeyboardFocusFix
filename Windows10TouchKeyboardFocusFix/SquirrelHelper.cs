using Squirrel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows10TouchKeyboardFocusFix
{
    internal static class SquirrelHelper
    {
        private const string releasesPath = "https://www.ghiasi.net/apps/Windows10TouchKeyboardFocusFix/Releases";

        internal static void ProcessSquirrelEvents()
        {
            try
            {
                using (var mgr = new UpdateManager(releasesPath))
                {
                    // Note, in most of these scenarios, the app exits after this method
                    // completes!
                    SquirrelAwareApp.HandleEvents(
                        onInitialInstall: v =>
                        {
                            mgr.CreateOnlyStartMenuShortcutForThisExe();
                        },
                        onAppUpdate: v =>
                        {
                            if (StartupHelper.IsRegisteredToRunAtStartup())
                                StartupHelper.AddToStartup(); // Fix executable path change
                            mgr.CreateOnlyStartMenuShortcutForThisExe();
                        },
                        onAppUninstall: v =>
                        {
                            StartupHelper.RemoveFromStartup();
                            mgr.RemoveOnlyStartMenuShortcutForThisExe();
                        },
                        onFirstRun: () =>
                        {
                            StartupHelper.AddToStartup();
                        });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ProcessSquirrelEvents failed: " + ex.ToString());
            }
        }

        internal static async void CheckForUpdates()
        {
            try
            {
                using (var mgr = new UpdateManager(releasesPath))
                {
                    await mgr.UpdateApp();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CheckForUpdates failed: " + ex.ToString());
            }
        }
    }
}
