using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

internal static class NativeMethods
{
    private static IntPtr _libdlHandle;
    private static IntPtr _libcHandle;

    // Delegate types for dynamically loaded POSIX functions
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SetlocaleDelegate(int category, string locale);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr DlopenDelegate(string filename, int flags);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int DlcloseDelegate(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr DlsymDelegate(IntPtr handle, string symbol);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr DlerrorDelegate();

    private static readonly SetlocaleDelegate? _setlocale;
    private static readonly DlopenDelegate? _dlopen;
    private static readonly DlcloseDelegate? _dlclose;
    private static readonly DlsymDelegate? _dlsym;
    private static readonly DlerrorDelegate? _dlerror;

    static NativeMethods()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // On macOS, system libraries are in libSystem.dylib
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var libSystemNames = new[] { "libSystem.dylib", "libSystem.B.dylib" };
                foreach (var name in libSystemNames)
                {
                    if (NativeLibrary.TryLoad(name, out _libcHandle))
                    {
                        _libdlHandle = _libcHandle; // On macOS, dl* functions are in libSystem
                        break;
                    }
                }
            }
            else
            {
                // Try to load libc with various names (Linux)
                var libcNames = new[] { "libc.so.6", "libc.so", "libc" };
                foreach (var name in libcNames)
                {
                    if (NativeLibrary.TryLoad(name, out _libcHandle))
                    {
                        break;
                    }
                }

                // Try to load libdl with various names (dl functions might be in libc on modern systems)
                var libdlNames = new[] { "libdl.so.2", "libdl.so", "libdl" };
                foreach (var name in libdlNames)
                {
                    if (NativeLibrary.TryLoad(name, out _libdlHandle))
                    {
                        break;
                    }
                }

                // If libdl didn't load, try using libc handle (many modern systems have dl* functions in libc)
                if (_libdlHandle == IntPtr.Zero)
                {
                    _libdlHandle = _libcHandle;
                }
            }

            // Load function pointers
            if (_libcHandle != IntPtr.Zero)
            {
                if (NativeLibrary.TryGetExport(_libcHandle, "setlocale", out var setlocalePtr))
                {
                    _setlocale = Marshal.GetDelegateForFunctionPointer<SetlocaleDelegate>(setlocalePtr);
                }
            }

            if (_libdlHandle != IntPtr.Zero)
            {
                if (NativeLibrary.TryGetExport(_libdlHandle, "dlopen", out var dlopenPtr))
                {
                    _dlopen = Marshal.GetDelegateForFunctionPointer<DlopenDelegate>(dlopenPtr);
                }
                if (NativeLibrary.TryGetExport(_libdlHandle, "dlclose", out var dlclosePtr))
                {
                    _dlclose = Marshal.GetDelegateForFunctionPointer<DlcloseDelegate>(dlclosePtr);
                }
                if (NativeLibrary.TryGetExport(_libdlHandle, "dlsym", out var dlsymPtr))
                {
                    _dlsym = Marshal.GetDelegateForFunctionPointer<DlsymDelegate>(dlsymPtr);
                }
                if (NativeLibrary.TryGetExport(_libdlHandle, "dlerror", out var dlerrorPtr))
                {
                    _dlerror = Marshal.GetDelegateForFunctionPointer<DlerrorDelegate>(dlerrorPtr);
                }
            }
        }
    }

    // Windows
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    internal static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern IntPtr LoadLibraryW(string dllToLoad);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, uint dwFlags);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool FreeLibrary(IntPtr hModule);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetDllDirectory(string lpPathName);

    [DllImport("kernel32.dll")]
    internal static extern uint GetLastError();

    // LoadLibraryEx flags
    internal const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;

    // POSIX constants
    internal const int LC_NUMERIC = 1;
    internal const int RTLD_LAZY = 0x0001;
    internal const int RTLD_NOW = 0x0002;
    internal const int RTLD_GLOBAL = 0x0100;
    internal const int RTLD_LOCAL = 0x0000;

    // POSIX function wrappers
    internal static IntPtr setlocale(int category, string locale)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("setlocale is not supported on Windows");
        }
        return _setlocale?.Invoke(category, locale) ?? IntPtr.Zero;
    }

    internal static IntPtr dlopen(string filename, int flags)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("dlopen is not supported on Windows");
        }
        return _dlopen?.Invoke(filename, flags) ?? IntPtr.Zero;
    }

    internal static IntPtr dlclose(IntPtr handle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("dlclose is not supported on Windows");
        }
        return _dlclose?.Invoke(handle) ?? IntPtr.Zero;
    }

    internal static IntPtr dlsym(IntPtr handle, string symbol)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("dlsym is not supported on Windows");
        }
        return _dlsym?.Invoke(handle, symbol) ?? IntPtr.Zero;
    }

    internal static string? dlerror()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("dlerror is not supported on Windows");
        }
        var ptr = _dlerror?.Invoke() ?? IntPtr.Zero;
        return ptr != IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) : null;
    }

    // Cross-platform wrappers
    internal static IntPtr CrossLoadLibrary(string fileName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // For VLC on Windows, we need to help LoadLibrary find dependencies
            var directory = Path.GetDirectoryName(fileName);
            var originalDirectory = Directory.GetCurrentDirectory();
            IntPtr handle = IntPtr.Zero;

            try
            {
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    // Set the current directory to the DLL's directory
                    // This helps LoadLibrary find dependencies in the same folder
                    Directory.SetCurrentDirectory(directory);

                    // Also set the DLL directory for additional search path help
                    SetDllDirectory(directory);
                }

                // First try LoadLibraryEx with LOAD_WITH_ALTERED_SEARCH_PATH
                // This makes the DLL's directory part of the search path for its dependencies
                handle = LoadLibraryEx(fileName, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH);

                if (handle == IntPtr.Zero)
                {
                    // Fall back to regular LoadLibrary
                    handle = LoadLibrary(fileName);
                }

                if (handle == IntPtr.Zero)
                {
                    var error = GetLastError();
                    System.Diagnostics.Debug.WriteLine($"Failed to load {fileName}, error code: {error}");
                }
            }
            finally
            {
                // Always restore the original current directory
                try
                {
                    Directory.SetCurrentDirectory(originalDirectory);
                }
                catch
                {
                    // Ignore errors when restoring directory
                }
            }

            return handle;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // On macOS, try using .NET's NativeLibrary.TryLoad first which handles @rpath and other macOS-specific features
            if (NativeLibrary.TryLoad(fileName, out var handle))
            {
                System.Diagnostics.Debug.WriteLine($"Successfully loaded {fileName} using NativeLibrary.TryLoad");
                return handle;
            }

            System.Diagnostics.Debug.WriteLine($"NativeLibrary.TryLoad failed for {fileName}, trying dlopen strategies");

            // Strategy 1: Try RTLD_LAZY | RTLD_GLOBAL (more permissive, allows undefined symbols)
            var result = dlopen(fileName, RTLD_LAZY | RTLD_GLOBAL);
            if (result != IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine($"Successfully loaded {fileName} with dlopen (RTLD_LAZY | RTLD_GLOBAL)");
                return result;
            }
            var error1 = dlerror();
            System.Diagnostics.Debug.WriteLine($"dlopen with RTLD_LAZY | RTLD_GLOBAL failed: {error1}");

            // Strategy 2: Try RTLD_NOW | RTLD_GLOBAL (resolve all symbols immediately)
            result = dlopen(fileName, RTLD_NOW | RTLD_GLOBAL);
            if (result != IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine($"Successfully loaded {fileName} with dlopen (RTLD_NOW | RTLD_GLOBAL)");
                return result;
            }
            var error2 = dlerror();
            System.Diagnostics.Debug.WriteLine($"dlopen with RTLD_NOW | RTLD_GLOBAL failed: {error2}");

            // Strategy 3: Try with RTLD_LAZY | RTLD_LOCAL
            result = dlopen(fileName, RTLD_LAZY | RTLD_LOCAL);
            if (result != IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine($"Successfully loaded {fileName} with dlopen (RTLD_LAZY | RTLD_LOCAL)");
                return result;
            }
            var error3 = dlerror();
            System.Diagnostics.Debug.WriteLine($"dlopen with RTLD_LAZY | RTLD_LOCAL failed: {error3}");

            System.Diagnostics.Debug.WriteLine($"All dlopen strategies failed for {fileName}");
            return IntPtr.Zero;
        }
        else
        {
            return dlopen(fileName, RTLD_NOW | RTLD_GLOBAL);
        }
    }

    internal static void CrossFreeLibrary(IntPtr handle)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            FreeLibrary(handle);
        }
        else
        {
            dlclose(handle);
        }
    }

    internal static IntPtr CrossGetProcAddress(IntPtr handle, string name)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetProcAddress(handle, name);
        }
        else
        {
            return dlsym(handle, name);
        }
    }

    internal static object? GetDllType(IntPtr handle, Type type, string name)
    {
        var address = CrossGetProcAddress(handle, name);
        return address != IntPtr.Zero ? Marshal.GetDelegateForFunctionPointer(address, type) : null;
    }
}
