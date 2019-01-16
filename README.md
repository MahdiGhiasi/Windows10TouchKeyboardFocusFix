# Touch Keyboard Focus Fix

There's a bug in Windows 10 touch screen devices where maximized Win32 (Classic) windows don't get resized when touch keyboard opens. This can hurt the user experience, as user might not be able to see the text box they're writing on.

This tool fixes this problem by resizing the active window when on screen keyboard opens, if that's a Win32 (Classic) app. Behavior of UWP apps remain unchanged, as they are aware of on screen keyboard and update their UI accordingly.

It works on regular Desktop Mode as well as Tablet Mode.

**Download the latest version from [the releases page](https://github.com/MahdiGhiasi/Windows10TouchKeyboardFocusFix/releases).**
