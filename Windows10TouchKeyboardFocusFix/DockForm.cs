using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Windows10TouchKeyboardFocusFix
{
    public partial class DockForm : ShellLib.ApplicationDesktopToolbar
    {
        readonly int keyboardDockedPositionMaxDiff = 4;

        bool isVisible;
        WindowManipulationHelper.WindowState lastWindowState;

        public DockForm()
        {
            InitializeComponent();

            HideDock();
            TabletModeHelper.TabletModeChanged += TabletModeHelper_TabletModeChanged;
            if (TabletModeHelper.IsTabletMode)
                Edge = AppBarEdges.Bottom;
            else
                Edge = AppBarEdges.Float;
            Opacity = 0.01;
            Top = Screen.PrimaryScreen.Bounds.Height;
        }

        private void TabletModeHelper_TabletModeChanged(object sender, bool isTabletMode)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                try
                {
                    if (isTabletMode)
                        Edge = AppBarEdges.Bottom;
                    else
                        Edge = AppBarEdges.Float;

                    HideDock();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception in TabletModeHelper_TabletModeChanged: " + ex.ToString());
                }
            }));
     }

        private void DockForm_Load(object sender, EventArgs e)
        {
            keyboardStateCheckTimer.Enabled = true;
        }

        private bool IsKeyboardInDockedMode()
        {
            var position = TouchKeyboardHelper.GetTouchKeyboardPosition();

            if (position == null)
                return false;

            var screenBounds = Screen.PrimaryScreen.Bounds;

            var keyboardPosition = (Rectangle)position;
            if (keyboardPosition.Left <= keyboardDockedPositionMaxDiff &&
                keyboardPosition.Bottom + keyboardDockedPositionMaxDiff >= screenBounds.Height &&
                keyboardPosition.Right + keyboardDockedPositionMaxDiff >= screenBounds.Width)
                return true;

            return false;
        }

        Rectangle? keyboardPosition;
        private async void keyboardStateCheckTimer_Tick(object sender, EventArgs e)
        {
            var isOpen = TouchKeyboardHelper.IsOpen();
            if (isOpen && !isVisible && IsKeyboardInDockedMode())
            {
                keyboardPosition = TouchKeyboardHelper.GetTouchKeyboardPosition();
                if (keyboardPosition != null)
                    KeyboardOpened((Rectangle)keyboardPosition);
            }
            else if (isOpen && isVisible)
            {
                // Make sure keyboard type is not changed from docked.
                if (!IsKeyboardInDockedMode())
                {
                    KeyboardClosed();
                }
                else
                {
                    // Check for keyboard resize
                    var currentKeyboardPosition = TouchKeyboardHelper.GetTouchKeyboardPosition();

                    if (currentKeyboardPosition != null && keyboardPosition != null &&
                        currentKeyboardPosition != keyboardPosition)
                    {
                        Debug.WriteLine("Keyboard height changed");
                        UpdateKeyboardHeight((Rectangle)currentKeyboardPosition);
                        keyboardPosition = currentKeyboardPosition;
                    }


                    // Check for foreground app change
                    this.Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            if (!TabletModeHelper.IsTabletMode && currentKeyboardPosition != null)
                            {
                                var currentForegroundWindow = WindowManipulationHelper.GetForegroundWindow();
                                if (currentForegroundWindow != lastWindowState.windowHandle)
                                {
                                    Debug.WriteLine("Foreground window changed");
                                    ExpandForegroundWindow();
                                    ShrinkForegroundWindow((Rectangle)currentKeyboardPosition);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Exception in Check for foreground app change: " + ex.ToString());
                        }
                    }));
                }
            }
            else if (!isOpen && isVisible)
            {
                KeyboardClosed();
            }
        }

        private void UpdateKeyboardHeight(Rectangle keyboardPosition)
        {
            if (TabletModeHelper.IsTabletMode)
            {
                ShowDock(keyboardPosition);
            }
            else
            {
                ExpandForegroundWindow();
                ShrinkForegroundWindow(keyboardPosition);
            }
        }

        private void KeyboardOpened(Rectangle keyboardPosition)
        {
            Debug.WriteLine("KeyboardOpened");
            isVisible = true;

            if (WindowManipulationHelper.IsForegroundWindowMaximized() &&
                IsKeyboardInDockedMode())
            {
                if (TabletModeHelper.IsTabletMode)
                {
                    ShowDock(keyboardPosition);
                    lastWindowState = null;
                }
                else
                {
                    ShrinkForegroundWindow(keyboardPosition);
                }
            }
            else
            {
                lastWindowState = null;
            }
        }

        private void ShrinkForegroundWindow(Rectangle keyboardPosition)
        {
            //TODO: Make this compatible with all taskbar configurations
            lastWindowState = WindowManipulationHelper.ChangeForegroundWindowToWindowedFullScreen(
                keyboardPosition.Height - 92);
        }

        private void KeyboardClosed()
        {
            Debug.WriteLine("KeyboardClosed");
            isVisible = false;

            if (TabletModeHelper.IsTabletMode)
            {
                HideDock();
            }
            else
            {
                ExpandForegroundWindow();
            }
        }

        private void ExpandForegroundWindow()
        {
            if (lastWindowState != null)
                WindowManipulationHelper.ReturnForegroundWindowToDefaultMaximizedState(lastWindowState);
        }

        private async void ShowDock(Rectangle keyboardPosition)
        {
            Debug.WriteLine("ShowDock");
            await Task.Delay(100);

            //TODO: Make this compatible with all taskbar configurations
            this.Height = keyboardPosition.Height - 92;
            this.Width = 1;

        }

        private void HideDock()
        {
            Debug.WriteLine("HideDock");
            //Edge = AppBarEdges.Float;

            this.Height = 0;
            this.Width = 1;
            Top = Screen.PrimaryScreen.Bounds.Height;
        }
    }
}
