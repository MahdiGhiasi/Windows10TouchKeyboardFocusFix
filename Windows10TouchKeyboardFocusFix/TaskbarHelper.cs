using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows10TouchKeyboardFocusFix
{
    internal static class TaskbarHelper
    {
        public static TaskbarPosition GetTaskbarPosition()
        {
            if (Screen.PrimaryScreen.WorkingArea.Left > 0)
                return TaskbarPosition.Left;
            else if (Screen.PrimaryScreen.WorkingArea.Top > 0)
                return TaskbarPosition.Top;
            else if (Screen.PrimaryScreen.WorkingArea.Width != Screen.PrimaryScreen.Bounds.Width)
                return TaskbarPosition.Right;
            else
                return TaskbarPosition.Bottom;
        }

        public static Size GetTaskbarSize()
        {
            var pos = GetTaskbarPosition();

            if (pos == TaskbarPosition.Top || pos == TaskbarPosition.Bottom)
            {
                return new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height - Screen.PrimaryScreen.WorkingArea.Height);
            }
            else
            {
                return new Size(Screen.PrimaryScreen.Bounds.Width - Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height);
            }
        }

        internal enum TaskbarPosition
        {
            Left,
            Right,
            Top,
            Bottom,
        }
    }
}
