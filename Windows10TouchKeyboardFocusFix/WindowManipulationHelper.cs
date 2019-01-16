using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows10TouchKeyboardFocusFix.Win32.Structs;

namespace Windows10TouchKeyboardFocusFix
{
    public static class WindowManipulationHelper
    {
        private const int WPF_RESTORETOMAXIMIZED = 0x0002;

        /// <summary>
        /// If window is maximized, changes its state to windowed, but in the exact
        /// same position.
        /// </summary>
        /// <returns>Window's original placement configuration</returns>
        internal static WindowState ChangeForegroundWindowToWindowedFullScreen(int heightDecrement = 0)
        {
            var activeWindow = GetForegroundWindow();

            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.Length = Marshal.SizeOf(placement);
            GetWindowPlacement(activeWindow, ref placement);

            if (placement.ShowCmd != Win32.Enums.ShowWindowCommands.Maximize)
                return new WindowState(activeWindow, placement);

            WINDOWPLACEMENT newPlacement = new WINDOWPLACEMENT
            {
                Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT)),
                ShowCmd = Win32.Enums.ShowWindowCommands.Normal,
                MaxPosition = placement.MaxPosition,
                MinPosition = placement.MinPosition,
                NormalPosition = new System.Drawing.Rectangle(0, 0, 
                    Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height - heightDecrement)
            };
            SetWindowPlacement(activeWindow, ref newPlacement);

            return new WindowState(activeWindow, placement);
        }

        internal static void ReturnForegroundWindowToDefaultMaximizedState(WindowState originalState)
        {
            if (originalState.placement.ShowCmd != Win32.Enums.ShowWindowCommands.Maximize)
                return;

            WINDOWPLACEMENT curPlacement = new WINDOWPLACEMENT();
            curPlacement.Length = Marshal.SizeOf(curPlacement);
            GetWindowPlacement(originalState.windowHandle, ref curPlacement);

            if (curPlacement.ShowCmd == Win32.Enums.ShowWindowCommands.ShowMinimized)
            {
                WINDOWPLACEMENT newPlacement = new WINDOWPLACEMENT
                {
                    Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT)),
                    ShowCmd = curPlacement.ShowCmd,
                    MaxPosition = originalState.placement.MaxPosition,
                    MinPosition = originalState.placement.MinPosition,
                    NormalPosition = originalState.placement.NormalPosition,
                    Flags = WPF_RESTORETOMAXIMIZED,
                };
                SetWindowPlacement(originalState.windowHandle, ref newPlacement);
                return;
            }


            // Set a temporary placement with NormalPosition same as maximized
            // to avoid flickering, then set the real NormalPosition after window
            // is already maximized.

            WINDOWPLACEMENT tempPlacement = new WINDOWPLACEMENT
            {
                Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT)),
                ShowCmd = Win32.Enums.ShowWindowCommands.Maximize,
                MaxPosition = originalState.placement.MaxPosition,
                MinPosition = originalState.placement.MinPosition,
                NormalPosition = new System.Drawing.Rectangle(0, 0, Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height)
            };
            SetWindowPlacement(originalState.windowHandle, ref tempPlacement);

            SetWindowPlacement(originalState.windowHandle, ref originalState.placement);
        }

        internal static bool IsForegroundWindowMaximized()
        {
            var activeWindow = GetForegroundWindow();

            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.Length = Marshal.SizeOf(placement);
            GetWindowPlacement(activeWindow, ref placement);

            return (placement.ShowCmd == Win32.Enums.ShowWindowCommands.Maximize);
        }

        internal static bool IsForegroundWindowUWP()
        {
            try
            {
                var foregroundWindow = GetForegroundWindow();
                var threadId = GetWindowThreadProcessId(foregroundWindow, out uint processId);
                var process = Process.GetProcessById((int)processId);

                if (process.ProcessName == "ApplicationFrameHost")
                    return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsForegroundWindowUWP failed: " + ex.ToString());
            }

            return false;
        }

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window. 
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpdwProcessId">A pointer to a variable that receives the process identifier. If this parameter is not NULL, GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it does not.</param>
        /// <returns>The return value is the identifier of the thread that created the window.</returns>
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        ///     Retrieves a handle to the foreground window (the window with which the user is currently working). The system
        ///     assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads.
        ///     <para>See https://msdn.microsoft.com/en-us/library/windows/desktop/ms633505%28v=vs.85%29.aspx for more information.</para>
        /// </summary>
        /// <returns>
        ///     C++ ( Type: Type: HWND )<br /> The return value is a handle to the foreground window. The foreground window
        ///     can be NULL in certain circumstances, such as when a window is losing activation.
        /// </returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Sets the show state and the restored, minimized, and maximized positions of the specified window.
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window.
        /// </param>
        /// <param name="lpwndpl">
        /// A pointer to a WINDOWPLACEMENT structure that specifies the new show state and window positions.
        /// <para>
        /// Before calling SetWindowPlacement, set the length member of the WINDOWPLACEMENT structure to sizeof(WINDOWPLACEMENT). SetWindowPlacement fails if the length member is not set correctly.
        /// </para>
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// <para>
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </para>
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPlacement(IntPtr hWnd,
           [In] ref WINDOWPLACEMENT lpwndpl);

        /// <summary>
        /// Retrieves the show state and the restored, minimized, and maximized positions of the specified window.
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window.
        /// </param>
        /// <param name="lpwndpl">
        /// A pointer to the WINDOWPLACEMENT structure that receives the show state and position information.
        /// <para>
        /// Before calling GetWindowPlacement, set the length member to sizeof(WINDOWPLACEMENT). GetWindowPlacement fails if lpwndpl-> length is not set correctly.
        /// </para>
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// <para>
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </para>
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, 
            ref WINDOWPLACEMENT lpwndpl);


        internal class WindowState
        {
            internal IntPtr windowHandle;
            internal WINDOWPLACEMENT placement;

            internal WindowState(IntPtr windowHandle, WINDOWPLACEMENT placement)
            {
                this.windowHandle = windowHandle;
                this.placement = placement;
            }
        }
    }
}


