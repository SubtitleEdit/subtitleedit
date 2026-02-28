using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic;

public static  class CursorPositionHelper
{
    // Windows API for getting cursor position
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

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
}
