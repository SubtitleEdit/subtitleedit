using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// Registers the menu bar's "Window" submenu with AppKit as the application's windows
/// menu (NSApplication.windowsMenu). Avalonia's NativeMenu API has no notion of a
/// windows menu, and without one the Dock icon's context menu never lists the app's
/// open windows. Once registered, AppKit itself keeps the submenu populated with
/// every open window and mirrors that list into the Dock menu, which is the standard
/// macOS behavior users expect from multi-window apps.
///
/// Uses the Objective-C runtime directly (objc_msgSend). Every entry point is
/// defensive: any failure reports false and the caller keeps the manually populated
/// window list from InitNativeMacMenu as the fallback, so the worst case is the
/// pre-existing behavior.
/// </summary>
internal static class MacWindowsMenuInterop
{
    private const string LibObjC = "/usr/lib/libobjc.A.dylib";

    [DllImport(LibObjC)]
    private static extern IntPtr objc_getClass(string name);

    [DllImport(LibObjC)]
    private static extern IntPtr sel_registerName(string name);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtr(IntPtr receiver, IntPtr sel);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtrPtr(IntPtr receiver, IntPtr sel, IntPtr arg);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtrLong(IntPtr receiver, IntPtr sel, long arg);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern long SendLong(IntPtr receiver, IntPtr sel);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern IntPtr SendPtrUtf8(IntPtr receiver, IntPtr sel, string arg);

    [DllImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static extern void SendVoidPtrPtrBool(IntPtr receiver, IntPtr sel, IntPtr arg1, IntPtr arg2, sbyte arg3);

    private static readonly IntPtr SelSharedApplication = sel_registerName("sharedApplication");
    private static readonly IntPtr SelMainMenu = sel_registerName("mainMenu");
    private static readonly IntPtr SelNumberOfItems = sel_registerName("numberOfItems");
    private static readonly IntPtr SelItemAtIndex = sel_registerName("itemAtIndex:");
    private static readonly IntPtr SelTitle = sel_registerName("title");
    private static readonly IntPtr SelUtf8String = sel_registerName("UTF8String");
    private static readonly IntPtr SelSubmenu = sel_registerName("submenu");
    private static readonly IntPtr SelSetWindowsMenu = sel_registerName("setWindowsMenu:");
    private static readonly IntPtr SelWindowsMenu = sel_registerName("windowsMenu");
    private static readonly IntPtr SelStringWithUtf8 = sel_registerName("stringWithUTF8String:");
    private static readonly IntPtr SelAddWindowsItem = sel_registerName("addWindowsItem:title:filename:");

    // The windows menus (one NSMenu per editor window's menu bar) already handed to
    // AppKit, so re-activation of the same window skips the objc round trip.
    private static readonly HashSet<IntPtr> _registeredMenus = [];

    /// <summary>
    /// Finds the top-level menu named <paramref name="windowMenuTitle"/> in the current
    /// NSMenuBar, registers it as NSApplication.windowsMenu, and asks AppKit to list the
    /// given already-open windows in it. Returns true when AppKit now owns the windows
    /// menu (the caller should then leave its contents to AppKit).
    /// </summary>
    public static bool TryRegisterWindowsMenu(string windowMenuTitle, IEnumerable<Window> openWindows)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return false;
        }

        try
        {
            var nsAppClass = objc_getClass("NSApplication");
            if (nsAppClass == IntPtr.Zero)
            {
                return false;
            }

            var nsApp = SendPtr(nsAppClass, SelSharedApplication);
            if (nsApp == IntPtr.Zero)
            {
                return false;
            }

            var mainMenu = SendPtr(nsApp, SelMainMenu);
            if (mainMenu == IntPtr.Zero)
            {
                return false;
            }

            // Locate our "Window" item in the currently installed menu bar (each editor
            // window has its own NSMenuBar, so this runs again per activation).
            var submenu = IntPtr.Zero;
            var count = SendLong(mainMenu, SelNumberOfItems);
            for (long i = 0; i < count; i++)
            {
                var item = SendPtrLong(mainMenu, SelItemAtIndex, i);
                if (item == IntPtr.Zero)
                {
                    continue;
                }

                var titlePtr = SendPtr(item, SelTitle);
                var utf8 = titlePtr == IntPtr.Zero ? IntPtr.Zero : SendPtr(titlePtr, SelUtf8String);
                var title = utf8 == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(utf8);
                if (string.Equals(title, windowMenuTitle, StringComparison.Ordinal))
                {
                    submenu = SendPtr(item, SelSubmenu);
                    break;
                }
            }

            if (submenu == IntPtr.Zero)
            {
                return false;
            }

            if (SendPtr(nsApp, SelWindowsMenu) != submenu)
            {
                SendPtrPtr(nsApp, SelSetWindowsMenu, submenu);
                if (SendPtr(nsApp, SelWindowsMenu) != submenu)
                {
                    return false;
                }
            }

            // AppKit adds windows that appear after registration by itself; windows that
            // were already open need introducing once per menu.
            if (_registeredMenus.Add(submenu))
            {
                var nsStringClass = objc_getClass("NSString");
                foreach (var window in openWindows)
                {
                    var handle = window.TryGetPlatformHandle();
                    if (handle == null || handle.Handle == IntPtr.Zero)
                    {
                        continue;
                    }

                    var title = string.IsNullOrWhiteSpace(window.Title) ? "Subtitle Edit" : window.Title;
                    var nsTitle = SendPtrUtf8(nsStringClass, SelStringWithUtf8, title);
                    if (nsTitle == IntPtr.Zero)
                    {
                        continue;
                    }

                    SendVoidPtrPtrBool(nsApp, SelAddWindowsItem, handle.Handle, nsTitle, 0);
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
