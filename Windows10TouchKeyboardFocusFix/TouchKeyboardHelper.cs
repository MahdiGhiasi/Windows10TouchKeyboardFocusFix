using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Windows10TouchKeyboardFocusFix
{
    public static class TouchKeyboardHelper
    {
        // Mechanism of finding touch keyboard window (GetTouchKeyboardWindowHandle())
        // is taken from https://stackoverflow.com/a/47405060/942659

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        public static bool IsOpen()
        {
            return GetTouchKeyboardWindowHandle() != IntPtr.Zero;
        }

        private static IntPtr GetTouchKeyboardWindowHandle()
        {
            var parent = IntPtr.Zero;
            for (; ; )
            {
                parent = FindWindowEx(IntPtr.Zero, parent, WindowParentClass1709);
                if (parent == IntPtr.Zero)
                    return IntPtr.Zero; // no more windows, keyboard state is unknown

                // if it's a child of a WindowParentClass1709 window - the keyboard is open
                var wnd = FindWindowEx(parent, IntPtr.Zero, WindowClass1709, WindowCaption1709);
                if (wnd != IntPtr.Zero)
                    return wnd;
            }
        }

        public static Rectangle? GetTouchKeyboardPosition()
        {
            var window = GetTouchKeyboardWindowHandle();

            if (window == IntPtr.Zero)
                return null;

            var handleRefObj = new Object();
            RECT rct;
            if (!GetWindowRect(new HandleRef(handleRefObj, window), out rct))
                return null;

            return new Rectangle(rct.Left, rct.Top, rct.Right - rct.Left, rct.Bottom - rct.Top);
        }

        private const string WindowClass = "IPTip_Main_Window";
        private const string WindowParentClass1709 = "ApplicationFrameWindow";
        private const string WindowClass1709 = "Windows.UI.Core.CoreWindow";
        private const string WindowCaption1709 = "Microsoft Text Input Application";

        private enum WindowStyle : uint
        {
            Disabled = 0x08000000,
            Visible = 0x10000000,
        }

        private static WindowStyle GetWindowStyle(IntPtr wnd)
        {
            return (WindowStyle)GetWindowLong(wnd, -16);
        }

        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string className, string title = null);

        [DllImport("user32.dll", SetLastError = false)]
        private static extern uint GetWindowLong(IntPtr wnd, int index);
    }
}
