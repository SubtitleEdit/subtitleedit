using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

/// <summary>
/// The macOS calls <see cref="LibMpvDynamicIoSurfaceControl"/> needs: an offscreen CGL context
/// for mpv's OpenGL renderer, IOSurfaces bound as GL textures, and an MTLSharedEvent that
/// Avalonia's Metal compositor waits on and signals.
/// </summary>
[SupportedOSPlatform("macos")]
internal static class MacIoSurfaceInterop
{
    private const string OpenGl = "/System/Library/Frameworks/OpenGL.framework/OpenGL";
    private const string IoSurface = "/System/Library/Frameworks/IOSurface.framework/IOSurface";
    private const string CoreFoundation = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private const string Metal = "/System/Library/Frameworks/Metal.framework/Metal";
    private const string ObjC = "/usr/lib/libobjc.A.dylib";

    public const uint GL_TEXTURE_RECTANGLE = 0x84F5;
    public const uint GL_RGBA = 0x1908;
    public const uint GL_BGRA = 0x80E1;
    public const uint GL_UNSIGNED_INT_8_8_8_8_REV = 0x8367;
    public const uint GL_FRAMEBUFFER = 0x8D40;
    public const uint GL_COLOR_ATTACHMENT0 = 0x8CE0;
    public const uint GL_FRAMEBUFFER_COMPLETE = 0x8CD5;
    public const uint GL_COLOR_BUFFER_BIT = 0x4000;

    private const int kCGLPFAAccelerated = 73;
    private const int kCGLPFAAllowOfflineRenderers = 96;
    private const int kCGLPFAOpenGLProfile = 99;
    private const int kCGLOGLPVersion_3_2_Core = 0x3200;
    private const int kCGLPFAColorSize = 8;

    private const uint PixelFormatBgra = 0x42475241; // 'BGRA'
    private const nint kCFNumberSInt32Type = 3;

    // CGL

    [DllImport(OpenGl)] private static extern int CGLChoosePixelFormat(int[] attributes, out IntPtr pixelFormat, out int count);
    [DllImport(OpenGl)] private static extern int CGLCreateContext(IntPtr pixelFormat, IntPtr share, out IntPtr context);
    [DllImport(OpenGl)] private static extern int CGLDestroyPixelFormat(IntPtr pixelFormat);
    [DllImport(OpenGl)] public static extern int CGLDestroyContext(IntPtr context);
    [DllImport(OpenGl)] public static extern int CGLSetCurrentContext(IntPtr context);
    [DllImport(OpenGl)] private static extern IntPtr CGLGetCurrentContext();
    [DllImport(OpenGl)] public static extern int CGLTexImageIOSurface2D(IntPtr context, uint target, uint internalFormat, int width, int height, uint format, uint type, IntPtr ioSurface, uint plane);

    // OpenGL (OpenGL.framework exports the 3.2 core entry points directly)

    [DllImport(OpenGl)] public static extern void glGenTextures(int n, out uint texture);
    [DllImport(OpenGl)] public static extern void glDeleteTextures(int n, ref uint texture);
    [DllImport(OpenGl)] public static extern void glBindTexture(uint target, uint texture);
    [DllImport(OpenGl)] public static extern void glGenFramebuffers(int n, out uint framebuffer);
    [DllImport(OpenGl)] public static extern void glDeleteFramebuffers(int n, ref uint framebuffer);
    [DllImport(OpenGl)] public static extern void glBindFramebuffer(uint target, uint framebuffer);
    [DllImport(OpenGl)] public static extern void glFramebufferTexture2D(uint target, uint attachment, uint textarget, uint texture, int level);
    [DllImport(OpenGl)] public static extern uint glCheckFramebufferStatus(uint target);
    [DllImport(OpenGl)] public static extern void glViewport(int x, int y, int width, int height);
    [DllImport(OpenGl)] public static extern void glClearColor(float r, float g, float b, float a);
    [DllImport(OpenGl)] public static extern void glClear(uint mask);
    [DllImport(OpenGl)] public static extern void glFinish();
    [DllImport(OpenGl)] public static extern void glReadPixels(int x, int y, int width, int height, uint format, uint type, byte[] data);

    // IOSurface / CoreFoundation

    [DllImport(IoSurface)] private static extern IntPtr IOSurfaceCreate(IntPtr properties);
    [DllImport(CoreFoundation)] private static extern IntPtr CFDictionaryCreate(IntPtr allocator, IntPtr[] keys, IntPtr[] values, nint count, IntPtr keyCallBacks, IntPtr valueCallBacks);
    [DllImport(CoreFoundation)] private static extern IntPtr CFNumberCreate(IntPtr allocator, nint type, ref int value);
    [DllImport(CoreFoundation)] public static extern void CFRelease(IntPtr cf);

    // Metal / Objective-C

    [DllImport(Metal)] private static extern IntPtr MTLCreateSystemDefaultDevice();
    [DllImport(ObjC)] private static extern IntPtr sel_registerName(string name);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern IntPtr SendIntPtr(IntPtr receiver, IntPtr selector);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern ulong SendUInt64(IntPtr receiver, IntPtr selector);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void SendVoidUInt64(IntPtr receiver, IntPtr selector, ulong value);
    [DllImport(ObjC, EntryPoint = "objc_msgSend")] private static extern void SendVoid(IntPtr receiver, IntPtr selector);

    private static readonly IntPtr SelNewSharedEvent = sel_registerName("newSharedEvent");
    private static readonly IntPtr SelSignaledValue = sel_registerName("signaledValue");
    private static readonly IntPtr SelSetSignaledValue = sel_registerName("setSignaledValue:");
    private static readonly IntPtr SelRegistryId = sel_registerName("registryID");
    private static readonly IntPtr SelRelease = sel_registerName("release");

    private static IntPtr _openGlLibrary;

    public static IntPtr CurrentContext() => CGLGetCurrentContext();

    /// <summary>An offscreen OpenGL 3.2 core context, not shared with anything.</summary>
    public static IntPtr CreateOffscreenContext()
    {
        int[] attributes =
        [
            kCGLPFAOpenGLProfile, kCGLOGLPVersion_3_2_Core,
            kCGLPFAAccelerated,
            kCGLPFAAllowOfflineRenderers,
            kCGLPFAColorSize, 24,
            0,
        ];

        var error = CGLChoosePixelFormat(attributes, out var pixelFormat, out _);
        if (error != 0 || pixelFormat == IntPtr.Zero)
        {
            throw new InvalidOperationException($"CGLChoosePixelFormat failed: {error}");
        }

        try
        {
            error = CGLCreateContext(pixelFormat, IntPtr.Zero, out var context);
            if (error != 0 || context == IntPtr.Zero)
            {
                throw new InvalidOperationException($"CGLCreateContext failed: {error}");
            }

            return context;
        }
        finally
        {
            CGLDestroyPixelFormat(pixelFormat);
        }
    }

    /// <summary>For mpv's get_proc_address: every GL entry point lives in OpenGL.framework.</summary>
    public static IntPtr GetGlProcAddress(string name)
    {
        if (_openGlLibrary == IntPtr.Zero)
        {
            _openGlLibrary = NativeLibrary.Load(OpenGl);
        }

        return NativeLibrary.TryGetExport(_openGlLibrary, name, out var address) ? address : IntPtr.Zero;
    }

    /// <summary>A BGRA IOSurface; release it with <see cref="CFRelease"/>.</summary>
    public static IntPtr CreateIoSurface(int width, int height)
    {
        var library = NativeLibrary.Load(IoSurface);
        var cf = NativeLibrary.Load(CoreFoundation);
        var keys = new[]
        {
            ReadGlobal(library, "kIOSurfaceWidth"),
            ReadGlobal(library, "kIOSurfaceHeight"),
            ReadGlobal(library, "kIOSurfaceBytesPerElement"),
            ReadGlobal(library, "kIOSurfacePixelFormat"),
        };

        var widthValue = width;
        var heightValue = height;
        var bytesPerElement = 4;
        var pixelFormat = unchecked((int)PixelFormatBgra);
        var values = new[]
        {
            CFNumberCreate(IntPtr.Zero, kCFNumberSInt32Type, ref widthValue),
            CFNumberCreate(IntPtr.Zero, kCFNumberSInt32Type, ref heightValue),
            CFNumberCreate(IntPtr.Zero, kCFNumberSInt32Type, ref bytesPerElement),
            CFNumberCreate(IntPtr.Zero, kCFNumberSInt32Type, ref pixelFormat),
        };

        var dictionary = CFDictionaryCreate(
            IntPtr.Zero, keys, values, keys.Length,
            NativeLibrary.GetExport(cf, "kCFTypeDictionaryKeyCallBacks"),
            NativeLibrary.GetExport(cf, "kCFTypeDictionaryValueCallBacks"));
        try
        {
            var surface = IOSurfaceCreate(dictionary);
            if (surface == IntPtr.Zero)
            {
                throw new InvalidOperationException($"IOSurfaceCreate failed for {width}x{height}");
            }

            return surface;
        }
        finally
        {
            CFRelease(dictionary);
            foreach (var value in values)
            {
                CFRelease(value);
            }
        }
    }

    private static IntPtr ReadGlobal(IntPtr library, string name) => Marshal.ReadIntPtr(NativeLibrary.GetExport(library, name));

    /// <summary>The system default MTLDevice (retained) - the one Avalonia's compositor uses on a single-GPU Mac.</summary>
    public static IntPtr CreateDefaultMetalDevice() => MTLCreateSystemDefaultDevice();

    /// <summary>The device's IORegistry id in the byte order Avalonia reports as DeviceLuid.</summary>
    public static byte[] GetDeviceLuid(IntPtr device)
    {
        var bytes = BitConverter.GetBytes(SendUInt64(device, SelRegistryId));
        Array.Reverse(bytes);
        return bytes;
    }

    public static IntPtr NewSharedEvent(IntPtr device) => SendIntPtr(device, SelNewSharedEvent);

    public static ulong GetSignaledValue(IntPtr sharedEvent) => SendUInt64(sharedEvent, SelSignaledValue);

    public static void SetSignaledValue(IntPtr sharedEvent, ulong value) => SendVoidUInt64(sharedEvent, SelSetSignaledValue, value);

    public static void ReleaseObject(IntPtr obj)
    {
        if (obj != IntPtr.Zero)
        {
            SendVoid(obj, SelRelease);
        }
    }
}
