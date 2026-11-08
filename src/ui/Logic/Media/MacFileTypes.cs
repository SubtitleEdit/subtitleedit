using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Looks up how macOS sees a file extension, via UTType (macOS 11+). The save panel behaves
/// differently depending on whether an installed app has registered the extension's file type.
/// Only call this on macOS.
/// </summary>
internal static partial class MacFileTypes
{
    private const string LibObjC = "/usr/lib/libobjc.A.dylib";
    private const string UniformTypeIdentifiers = "/System/Library/Frameworks/UniformTypeIdentifiers.framework/UniformTypeIdentifiers";

    [LibraryImport(LibObjC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr objc_getClass(string name);

    [LibraryImport(LibObjC, StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr sel_registerName(string name);

    [LibraryImport(LibObjC)]
    private static partial IntPtr objc_autoreleasePoolPush();

    [LibraryImport(LibObjC)]
    private static partial void objc_autoreleasePoolPop(IntPtr pool);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendPtr(IntPtr receiver, IntPtr selector);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendPtrPtr(IntPtr receiver, IntPtr selector, IntPtr argument);

    [LibraryImport(LibObjC, StringMarshalling = StringMarshalling.Utf8, EntryPoint = "objc_msgSend")]
    private static partial IntPtr SendPtrString(IntPtr receiver, IntPtr selector, string argument);

    [LibraryImport(LibObjC, EntryPoint = "objc_msgSend")]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool SendBool(IntPtr receiver, IntPtr selector);

    /// <summary>
    /// True when an installed app has registered <paramref name="extension"/> with a file type
    /// that prefers another extension, e.g. ".ssa" with io.mpv.ass, which prefers ".ass".
    /// False for an unregistered extension and when the lookup fails.
    /// </summary>
    public static bool PrefersOtherExtension(string extension)
    {
        try
        {
            NativeLibrary.Load(UniformTypeIdentifiers);
            var pool = objc_autoreleasePoolPush();
            try
            {
                var ext = extension.TrimStart('.');
                var nsExtension = SendPtrString(objc_getClass("NSString"), sel_registerName("stringWithUTF8String:"), ext);
                var type = SendPtrPtr(objc_getClass("UTType"), sel_registerName("typeWithFilenameExtension:"), nsExtension);
                if (type == IntPtr.Zero || SendBool(type, sel_registerName("isDynamic")))
                {
                    return false;
                }

                var preferred = SendPtr(type, sel_registerName("preferredFilenameExtension"));
                if (preferred == IntPtr.Zero)
                {
                    return false;
                }

                var preferredExtension = Marshal.PtrToStringUTF8(SendPtr(preferred, sel_registerName("UTF8String")));
                return !string.Equals(preferredExtension, ext, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                objc_autoreleasePoolPop(pool);
            }
        }
        catch (Exception exception) when (exception is DllNotFoundException or EntryPointNotFoundException)
        {
            Debug.WriteLine($"File type lookup unavailable: {exception.Message}");
            return false;
        }
    }
}
