using Avalonia;
using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic;

public static  class CursorPositionHelper
{
    // Windows API for getting cursor position
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    private const int VkLButton = 0x01;
    private const int VkRButton = 0x02;
    private const int VkMButton = 0x04;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    // macOS API for getting cursor position
    private const string CoreGraphicsLib = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    private const string ApplicationServicesLib = "/System/Library/Frameworks/ApplicationServices.framework/ApplicationServices";


    [DllImport(CoreGraphicsLib)]
    private static extern CGPoint CGEventSourceGetCursorPosition(uint source);

    [DllImport(CoreGraphicsLib)]
    private static extern bool CGEventSourceButtonState(int stateId, uint button);

    private const int CGEventSourceStateCombinedSessionState = 0;

    [DllImport(CoreGraphicsLib)]
    private static extern IntPtr CGEventCreate(IntPtr source);

    [DllImport(ApplicationServicesLib)]
    private static extern CGPoint CGEventGetLocation(IntPtr eventRef);

    [DllImport(CoreGraphicsLib)]
    private static extern void CFRelease(IntPtr cf);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;
    }

    // Linux X11 API for getting cursor position
    [DllImport("libX11.so.6")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6")]
    private static extern bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child,
        out int rootX, out int rootY, out int winX, out int winY, out uint mask);

    [DllImport("libX11.so.6")]
    private static extern IntPtr XDefaultRootWindow(IntPtr display);

    public static (int X, int Y)? GetCursorPosition()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                if (GetCursorPos(out POINT cursorPos))
                {
                    return (cursorPos.X, cursorPos.Y);
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                var eventRef = CGEventCreate(IntPtr.Zero);
                if (eventRef != IntPtr.Zero)
                {
                    var point = CGEventGetLocation(eventRef);
                    CFRelease(eventRef);
                    var x = (int)Math.Round(point.X, MidpointRounding.AwayFromZero);
                    var y = (int)Math.Round(point.Y, MidpointRounding.AwayFromZero);
                    return (x, y);
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                var display = XOpenDisplay(IntPtr.Zero);
                if (display != IntPtr.Zero)
                {
                    var rootWindow = XDefaultRootWindow(display);
                    if (XQueryPointer(display, rootWindow, out _, out _, out int rootX, out int rootY,
                            out _, out _, out _))
                    {
                        XCloseDisplay(display);
                        return (rootX, rootY);
                    }

                    XCloseDisplay(display);
                }
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
        }

        return null;
    }

    /// <summary>
    /// Whether any mouse button is physically down right now, or null where the buttons cannot
    /// be sampled (unsupported platform, or the query failed - e.g. no X11 under Wayland).
    /// Used to tell a window activation the user clicked for apart from one the OS handed over
    /// (undocked foreground steal, #14168): a click's activation is delivered while the button
    /// is still down, an OS handover after another application's window closed is not.
    /// </summary>
    public static bool? IsAnyPointerButtonDown()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                // The high bit is "down now". VK_L/RBUTTON are physical buttons, so a swapped
                // primary button still lands on one of the three.
                return ((GetAsyncKeyState(VkLButton) |
                         GetAsyncKeyState(VkRButton) |
                         GetAsyncKeyState(VkMButton)) & 0x8000) != 0;
            }

            if (OperatingSystem.IsMacOS())
            {
                return CGEventSourceButtonState(CGEventSourceStateCombinedSessionState, 0) ||
                       CGEventSourceButtonState(CGEventSourceStateCombinedSessionState, 1) ||
                       CGEventSourceButtonState(CGEventSourceStateCombinedSessionState, 2);
            }

            if (OperatingSystem.IsLinux())
            {
                var display = XOpenDisplay(IntPtr.Zero);
                if (display != IntPtr.Zero)
                {
                    var rootWindow = XDefaultRootWindow(display);
                    var ok = XQueryPointer(display, rootWindow, out _, out _, out _, out _,
                        out _, out _, out var mask);
                    XCloseDisplay(display);
                    if (ok)
                    {
                        // Button1Mask | Button2Mask | Button3Mask
                        return (mask & 0x700) != 0;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception);
        }

        return null;
    }

    /// <summary>
    /// True when a desktop cursor position from <see cref="GetCursorPosition"/> is inside the
    /// client area of <paramref name="window"/>.
    /// The video windows poll the cursor because the mpv "wid" player renders into a native child
    /// window that swallows Avalonia pointer events - but that poll sees the whole desktop, so
    /// without this test any mouse movement in any app on any monitor counted as user activity
    /// and popped the video controls back up (issue #13207).
    /// </summary>
    public static bool IsCursorOverWindow(Window? window, (int X, int Y) cursorPosition)
    {
        if (window == null || !window.IsVisible || window.WindowState == WindowState.Minimized)
        {
            return false;
        }

        try
        {
            var point = window.PointToClient(new PixelPoint(cursorPosition.X, cursorPosition.Y));
            var size = window.ClientSize;
            return point.X >= 0 && point.Y >= 0 && point.X < size.Width && point.Y < size.Height;
        }
        catch
        {
            // Window not (yet) attached to a platform implementation - no logging, this runs
            // on a 100 ms timer and would spam the log.
            return false;
        }
    }
}
