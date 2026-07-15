using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.UiLogic;

/// <summary>
/// Works around a native HarfBuzz symbol clash on Linux (issue #11864).
///
/// The app ships its own <c>libHarfBuzzSharp.so</c> (used by SkiaSharp.HarfBuzz / SKShaper for
/// text shaping in the image-based subtitle export). On a real Linux desktop the GL / font stack
/// also pulls in the system <c>libharfbuzz.so.0</c>, and both export the global <c>hb_*</c> symbols.
/// ELF symbol interposition can then route libHarfBuzzSharp's internal <c>hb_face_destroy</c> call
/// to the mismatched system copy, crashing the shaper on teardown (SIGSEGV in <c>hb_face_destroy</c>).
///
/// Loading libHarfBuzzSharp with <c>RTLD_DEEPBIND</c> makes it resolve its own <c>hb_*</c> symbols
/// first, so create and destroy always use the same HarfBuzz. Call <see cref="Apply"/> once, as early
/// as possible at startup, before any SkiaSharp.HarfBuzz use.
/// </summary>
public static class HarfBuzzNativeFix
{
    private const int RtldNow = 2;
    private const int RtldDeepBind = 8;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr DlopenDelegate(string fileName, int flags);

    private static bool _applied;
    private static IntPtr _handle;

    public static void Apply()
    {
        if (_applied || !OperatingSystem.IsLinux())
        {
            return;
        }

        // Escape hatch in case deep-binding ever misbehaves on an exotic distro.
        if (Environment.GetEnvironmentVariable("SE_DISABLE_HARFBUZZ_FIX") == "1")
        {
            return;
        }

        _applied = true;

        // Resolve dlopen at runtime: it lives in libdl.so.2 on glibc < 2.34 (e.g. Debian 10)
        // and in libc on newer glibc and musl. A static DllImport("libc") crashed at startup
        // on older distros with EntryPointNotFoundException.
        var dlopen = ResolveDlopen();
        if (dlopen == null)
        {
            return; // Cannot deep-bind on this system; keep default resolution (same as the escape hatch).
        }

        NativeLibrary.SetDllImportResolver(typeof(HarfBuzzSharp.Blob).Assembly, (name, _, _) =>
        {
            try
            {
                if (name != "libHarfBuzzSharp")
                {
                    return IntPtr.Zero;
                }

                if (_handle != IntPtr.Zero)
                {
                    return _handle;
                }

                foreach (var path in CandidatePaths())
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    var handle = dlopen(path, RtldNow | RtldDeepBind);
                    if (handle != IntPtr.Zero)
                    {
                        _handle = handle;
                        return handle;
                    }
                }
            }
            catch
            {
                // An exception thrown from a DllImport resolver during layout is unrecoverable.
            }

            // Could not deep-bind; fall back to the default resolution so behaviour is unchanged.
            return IntPtr.Zero;
        });
    }

    private static DlopenDelegate? ResolveDlopen()
    {
        foreach (var libraryName in new[] { "libdl.so.2", "libdl.so", "libc.so.6", "libc" })
        {
            try
            {
                if (NativeLibrary.TryLoad(libraryName, out var library) &&
                    NativeLibrary.TryGetExport(library, "dlopen", out var export))
                {
                    return Marshal.GetDelegateForFunctionPointer<DlopenDelegate>(export);
                }
            }
            catch
            {
                // Try the next candidate.
            }
        }

        return null;
    }

    private static string[] CandidatePaths()
    {
        var baseDir = AppContext.BaseDirectory;
        return
        [
            Path.Combine(baseDir, "libHarfBuzzSharp.so"),
            Path.Combine(baseDir, "runtimes", "linux-x64", "native", "libHarfBuzzSharp.so"),
        ];
    }
}
