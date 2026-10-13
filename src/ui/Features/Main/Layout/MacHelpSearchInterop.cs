using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Main.Layout;

/// <summary>
/// Plugs the online help into the search field AppKit adds to the Help menu. Out of the
/// box that field only finds menu items; registering an NSUserInterfaceItemSearching
/// handler (NSApplication.registerUserInterfaceItemSearchHandler:) adds a "Help Topics"
/// section whose results come from <see cref="HelpTopicIndex"/> and open the matching
/// docs page (scrolled to the section) in the browser.
///
/// Avalonia has no API for this, so the handler is an Objective-C class built at runtime
/// with objc_allocateClassPair and [UnmanagedCallersOnly] method implementations. Every
/// step is defensive: on any failure the Help search simply keeps its menu-items-only
/// behavior.
///
/// AppKit only adds the search field to the menu it knows is the Help menu. Without
/// NSApplication.helpMenu it guesses from the title, which only matches "Help" in the
/// app's development language, so <see cref="TryRegisterHelpMenu"/> registers it
/// explicitly for translated menu bars (#15980).
/// </summary>
internal static unsafe partial class MacHelpSearchInterop
{
    private const string LibObjC = "/usr/lib/libobjc.A.dylib";
    private const string HandlerClassName = "SEHelpSearchHandler";

    [LibraryImport(LibObjC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr objc_getClass(string name);

    [LibraryImport(LibObjC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr objc_getProtocol(string name);

    [LibraryImport(LibObjC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr objc_allocateClassPair(IntPtr superclass, string name, nint extraBytes);

    [LibraryImport(LibObjC)]
    private static partial void objc_registerClassPair(IntPtr cls);

    [LibraryImport(LibObjC, StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool class_addMethod(IntPtr cls, IntPtr sel, IntPtr imp, string types);

    [LibraryImport(LibObjC)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool class_addProtocol(IntPtr cls, IntPtr protocol);

    [LibraryImport(LibObjC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr sel_registerName(string name);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendPtr(IntPtr receiver, IntPtr sel);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial void SendVoidPtr(IntPtr receiver, IntPtr sel, IntPtr arg);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendPtrLong(IntPtr receiver, IntPtr sel, long arg);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial long SendLong(IntPtr receiver, IntPtr sel);

    [LibraryImport(LibObjC, StringMarshalling = StringMarshalling.Utf8, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendPtrUtf8(IntPtr receiver, IntPtr sel, string arg);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendPtrArray(IntPtr receiver, IntPtr sel, IntPtr* objects, nuint count);

    private static bool _registered;
    private static IntPtr _handler;

    /// <summary>
    /// Registers the help topic search handler once per process. Safe to call on any
    /// platform; does nothing off macOS or when no help docs are embedded.
    /// </summary>
    public static void TryRegister()
    {
        if (_registered || !OperatingSystem.IsMacOS())
        {
            return;
        }

        _registered = true;
        try
        {
            if (!HelpTopicIndex.IsAvailable)
            {
                return;
            }

            var nsApp = SendPtr(objc_getClass("NSApplication"), sel_registerName("sharedApplication"));
            if (nsApp == IntPtr.Zero)
            {
                return;
            }

            var cls = objc_getClass(HandlerClassName);
            if (cls == IntPtr.Zero)
            {
                cls = objc_allocateClassPair(objc_getClass("NSObject"), HandlerClassName, 0);
                if (cls == IntPtr.Zero)
                {
                    return;
                }

                var protocol = objc_getProtocol("NSUserInterfaceItemSearching");
                if (protocol != IntPtr.Zero)
                {
                    class_addProtocol(cls, protocol);
                }

                class_addMethod(cls, sel_registerName("searchForItemsWithSearchString:resultLimit:matchedItemHandler:"),
                    (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, nint, IntPtr, void>)&SearchForItems, "v@:@q@?");
                class_addMethod(cls, sel_registerName("localizedTitlesForItem:"),
                    (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr>)&LocalizedTitlesForItem, "@@:@");
                class_addMethod(cls, sel_registerName("performActionForItem:"),
                    (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&PerformActionForItem, "v@:@");
                class_addMethod(cls, sel_registerName("showAllHelpTopicsForSearchString:"),
                    (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, void>)&ShowAllHelpTopics, "v@:@");
                objc_registerClassPair(cls);
            }

            // alloc+init leaves a +1 reference that is never released: the handler lives
            // as long as the app.
            _handler = SendPtr(SendPtr(cls, sel_registerName("alloc")), sel_registerName("init"));
            if (_handler == IntPtr.Zero)
            {
                return;
            }

            SendVoidPtr(nsApp, sel_registerName("registerUserInterfaceItemSearchHandler:"), _handler);
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Help menu search unavailable: {exception.Message}");
        }
    }

    /// <summary>
    /// Finds the top-level menu named <paramref name="helpMenuTitle"/> in the current
    /// NSMenuBar and registers it as NSApplication.helpMenu, so the Help search field
    /// shows up whatever the UI language. Each editor window has its own NSMenuBar, so
    /// this runs again per activation. Returns true when the Help menu is registered.
    /// </summary>
    public static bool TryRegisterHelpMenu(string helpMenuTitle)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return false;
        }

        try
        {
            var nsApp = SendPtr(objc_getClass("NSApplication"), sel_registerName("sharedApplication"));
            var mainMenu = nsApp == IntPtr.Zero ? IntPtr.Zero : SendPtr(nsApp, sel_registerName("mainMenu"));
            if (mainMenu == IntPtr.Zero)
            {
                return false;
            }

            var submenu = IntPtr.Zero;
            var count = SendLong(mainMenu, sel_registerName("numberOfItems"));
            for (long i = 0; i < count; i++)
            {
                var item = SendPtrLong(mainMenu, sel_registerName("itemAtIndex:"), i);
                if (item != IntPtr.Zero &&
                    string.Equals(ToManagedString(SendPtr(item, sel_registerName("title"))), helpMenuTitle, StringComparison.Ordinal))
                {
                    submenu = SendPtr(item, sel_registerName("submenu"));
                    break;
                }
            }

            if (submenu == IntPtr.Zero)
            {
                return false;
            }

            if (SendPtr(nsApp, sel_registerName("helpMenu")) != submenu)
            {
                SendVoidPtr(nsApp, sel_registerName("setHelpMenu:"), submenu);
            }

            return SendPtr(nsApp, sel_registerName("helpMenu")) == submenu;
        }
        catch
        {
            return false;
        }
    }

    // AppKit calls this on a background queue for every keystroke in the Help search
    // field. The block must be called exactly once, so failures still report "no hits".
    [UnmanagedCallersOnly]
    private static void SearchForItems(IntPtr self, IntPtr sel, IntPtr searchString, nint resultLimit, IntPtr handler)
    {
        var results = IntPtr.Zero;
        try
        {
            var query = ToManagedString(searchString);
            var hits = string.IsNullOrWhiteSpace(query)
                ? new List<HelpTopic>()
                : HelpTopicIndex.Search(query, (int)Math.Clamp(resultLimit, 0, 100));
            results = ToNSArray(hits.ConvertAll(h => h.Id));
        }
        catch
        {
            try
            {
                results = ToNSArray([]);
            }
            catch
            {
                // fall through with nil
            }
        }

        if (handler == IntPtr.Zero)
        {
            return;
        }

        // Block layout (64-bit): isa, int flags, int reserved, invoke function pointer.
        var invoke = (delegate* unmanaged<IntPtr, IntPtr, void>)Marshal.ReadIntPtr(handler, 16);
        invoke(handler, results);
    }

    [UnmanagedCallersOnly]
    private static IntPtr LocalizedTitlesForItem(IntPtr self, IntPtr sel, IntPtr item)
    {
        try
        {
            var id = ToManagedString(item);
            var topic = id == null ? null : HelpTopicIndex.Get(id);
            return ToNSArray(topic == null ? [] : [topic.Title]);
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    [UnmanagedCallersOnly]
    private static void PerformActionForItem(IntPtr self, IntPtr sel, IntPtr item)
    {
        try
        {
            var id = ToManagedString(item);
            var topic = id == null ? null : HelpTopicIndex.Get(id);
            if (topic != null)
            {
                UiUtil.ShowHelp(topic.Page, topic.Anchor);
            }
        }
        catch
        {
            // ignore - nothing sensible to show from a menu callback
        }
    }

    [UnmanagedCallersOnly]
    private static void ShowAllHelpTopics(IntPtr self, IntPtr sel, IntPtr searchString)
    {
        try
        {
            UiUtil.ShowHelp();
        }
        catch
        {
            // ignore
        }
    }

    private static string? ToManagedString(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero)
        {
            return null;
        }

        var utf8 = SendPtr(nsString, sel_registerName("UTF8String"));
        return utf8 == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(utf8);
    }

    // Returns an autoreleased NSArray of NSStrings.
    private static IntPtr ToNSArray(List<string> values)
    {
        var nsStringClass = objc_getClass("NSString");
        var selStringWithUtf8 = sel_registerName("stringWithUTF8String:");
        var objects = new IntPtr[values.Count];
        var count = 0;
        foreach (var value in values)
        {
            var nsString = SendPtrUtf8(nsStringClass, selStringWithUtf8, value);
            if (nsString != IntPtr.Zero)
            {
                objects[count++] = nsString;
            }
        }

        fixed (IntPtr* p = objects)
        {
            return SendPtrArray(objc_getClass("NSArray"), sel_registerName("arrayWithObjects:count:"), p, (nuint)count);
        }
    }
}
