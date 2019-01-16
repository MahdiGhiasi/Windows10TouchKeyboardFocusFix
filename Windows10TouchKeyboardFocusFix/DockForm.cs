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

            TabletModeHelper.TabletModeChanged += TabletModeHelper_TabletModeChanged;
            TabletModeChanged(TabletModeHelper.IsTabletMode);

            Opacity = 0.01;
            Top = Screen.PrimaryScreen.Bounds.Height;
            this.Visible = false;
        }

        private void TabletModeHelper_TabletModeChanged(object sender, bool isTabletMode)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                try
                {
                    TabletModeChanged(isTabletMode);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception in TabletModeHelper_TabletModeChanged: " + ex.ToString());
                }
            }));
        }

        private void TabletModeChanged(bool isTabletMode)
        {
            if (isTabletMode)
            {
                this.FormBorderStyle = FormBorderStyle.None; // for zero height dock
                //Edge = AppBarEdges.Bottom;
            }
            else
            {
                this.FormBorderStyle = FormBorderStyle.FixedToolWindow; // Hide from Task View
                Edge = AppBarEdges.Float;
            }

            HideDock();
        }

        private void DockForm_Load(object sender, EventArgs e)
        {
            keyboardStateCheckTimer.Enabled = true;
            notifyIcon.Visible = true;
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
                        MuchDifference((Rectangle)currentKeyboardPosition, (Rectangle)keyboardPosition, keyboardDockedPositionMaxDiff))
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

        private bool MuchDifference(Rectangle r1, Rectangle r2, int maxDelta)
        {
            var deltaLeft = r1.Left - r2.Left;
            var deltaTop = r1.Top - r2.Top;
            var deltaWidth = r1.Width - r2.Width;
            var deltaHeight = r1.Height - r2.Height;

            if (Math.Abs(deltaLeft) > maxDelta)
                return true;
            if (Math.Abs(deltaTop) > maxDelta)
                return true;
            if (Math.Abs(deltaWidth) > maxDelta)
                return true;
            if (Math.Abs(deltaHeight) > maxDelta)
                return true;

            return false;
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

            if (WindowManipulationHelper.IsForegroundWindowMaximized() &&
                IsKeyboardInDockedMode() &&
                !WindowManipulationHelper.IsForegroundWindowUWP())
            {
                isVisible = true;

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
            await Task.Delay(250);
            this.Edge = AppBarEdges.Bottom;

            //TODO: Make this compatible with all taskbar configurations
            this.Height = keyboardPosition.Height - 92;
            this.Width = 1;

        }

        private void HideDock()
        {
            Debug.WriteLine("HideDock");
            Edge = AppBarEdges.Float;

            this.Height = 0;
            this.Width = 1;
            Top = Screen.PrimaryScreen.Bounds.Height;
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            
        }

        private void menuAbout_Click(object sender, EventArgs e)
        {
            var aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        private void menuExit_Click(object sender, EventArgs e)
        {
            ExitProgram();
        }

        private void ExitProgram()
        {
            this.Edge = AppBarEdges.Float;
            notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
