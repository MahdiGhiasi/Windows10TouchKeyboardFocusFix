using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Windows10TouchKeyboardFocusFix
{
    internal static class TabletModeHelper
    {
        public static bool IsTabletMode { get; private set; }

        public static event EventHandler<bool> TabletModeChanged;

        static TabletModeHelper()
        {
            var currentUser = WindowsIdentity.GetCurrent();
            if (currentUser != null && currentUser.User != null)
            {
                var wqlEventQuery = new EventQuery(string.Format(@"SELECT * FROM RegistryValueChangeEvent WHERE Hive='HKEY_USERS' AND KeyPath='{0}\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ImmersiveShell' AND ValueName='TabletMode'", currentUser.User.Value));
                var managementEventWatcher = new ManagementEventWatcher(wqlEventQuery);
                managementEventWatcher.EventArrived += ManagementEventWatcher_EventArrived;
                managementEventWatcher.Start();
            }
            UpdateModeFromRegistry();
        }

        private static void ManagementEventWatcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            UpdateModeFromRegistry();
        }

        private static void UpdateModeFromRegistry()
        {
            var tabletModeStatus = (int)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ImmersiveShell", "TabletMode", 0);
            var oldTabletModeState = IsTabletMode;
            IsTabletMode = (tabletModeStatus == 1);

            if (IsTabletMode != oldTabletModeState)
                TabletModeChanged?.Invoke(null, IsTabletMode);
        }
    }
}
