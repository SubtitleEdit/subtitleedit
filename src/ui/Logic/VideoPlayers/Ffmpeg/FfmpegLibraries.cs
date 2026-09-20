using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

/// <summary>
/// Locates and initializes the FFmpeg shared libraries (avformat, avcodec, avutil, swscale,
/// swresample) used by <see cref="FfmpegPlayer"/>. The bindings come from FFmpeg.AutoGen, which
/// loads the libraries lazily on the first call, so "can we play at all" is answered by trying
/// one harmless call and catching the load failure.
/// <para>
/// The bindings are generated against one FFmpeg major version (see <see cref="MajorVersion"/>),
/// so the library files must be from that release line - the version is part of the file name
/// (<c>avcodec-63.dll</c>, <c>libavcodec.so.63</c>, <c>libavcodec.63.dylib</c>), which is also
/// what the folder probing looks for.
/// </para>
/// </summary>
public static class FfmpegLibraries
{
    /// <summary>FFmpeg release line the bundled bindings are generated for.</summary>
    public const string MajorVersion = "9.0";

    /// <summary>libavcodec major the bindings were generated for (63 for FFmpeg 9), as used in the library file names.</summary>
    public static int AvCodecMajor => ffmpeg.LIBAVCODEC_VERSION_MAJOR;

    /// <summary>
    /// Set this path (directory only) to override the default search paths - the same idea as
    /// <c>LibVlcDynamicPlayer.LibVlcPath</c>.
    /// </summary>
    public static string LibraryPath { get; set; } = string.Empty;

    private static readonly Lock InitLock = new();
    private static bool _initialized;
    private static bool _available;
    private static string _resolvedPath = string.Empty;

    /// <summary>The folder the libraries were loaded from, or empty when the system loader found them.</summary>
    public static string ResolvedPath => _resolvedPath;

    /// <summary>
    /// The file name of the avcodec library on this platform, e.g. <c>avcodec-63.dll</c>.
    /// Used both to probe candidate folders and to tell the user what is expected.
    /// </summary>
    public static string AvCodecFileName
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return $"avcodec-{AvCodecMajor}.dll";
            }

            if (OperatingSystem.IsMacOS())
            {
                return $"libavcodec.{AvCodecMajor}.dylib";
            }

            return $"libavcodec.so.{AvCodecMajor}";
        }
    }

    /// <summary>
    /// True when the FFmpeg libraries can be loaded. The first call resolves the search path and
    /// makes a probe call into libavutil; the result is cached until <see cref="Reset"/>.
    /// </summary>
    public static bool IsAvailable()
    {
        lock (InitLock)
        {
            if (_initialized)
            {
                return _available;
            }

            _initialized = true;
            _available = TryInitialize();
            return _available;
        }
    }

    /// <summary>
    /// Forget the cached probe result - after the libraries were downloaded, for example.
    /// </summary>
    public static void Reset()
    {
        lock (InitLock)
        {
            _initialized = false;
            _available = false;
        }
    }

    /// <summary>Candidate folders, most specific first; only existing folders that hold avcodec are used.</summary>
    public static IEnumerable<string> GetSearchPaths()
    {
        if (!string.IsNullOrWhiteSpace(LibraryPath))
        {
            yield return LibraryPath;
        }

        yield return Config.Se.FfmpegLibFolder;
        yield return Config.Se.FfmpegFolder;
        yield return AppContext.BaseDirectory;

        if (OperatingSystem.IsWindows())
        {
            yield break;
        }

        if (OperatingSystem.IsMacOS())
        {
            yield return "/opt/homebrew/lib";
            yield return "/usr/local/lib";
            yield return "/opt/local/lib";
            yield break;
        }

        // Linux: distro multiarch folders, Flatpak's /app/lib, then the generic ones.
        var arch = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x86_64-linux-gnu",
            Architecture.Arm64 => "aarch64-linux-gnu",
            Architecture.Arm => "arm-linux-gnueabihf",
            _ => string.Empty,
        };

        if (!string.IsNullOrEmpty(arch))
        {
            yield return Path.Combine("/usr/lib", arch);
        }

        yield return "/app/lib";
        yield return "/usr/lib64";
        yield return "/usr/lib";
        yield return "/usr/local/lib";
    }

    private static bool TryInitialize()
    {
        _resolvedPath = string.Empty;
        foreach (var folder in GetSearchPaths())
        {
            try
            {
                if (Directory.Exists(folder) && File.Exists(Path.Combine(folder, AvCodecFileName)))
                {
                    _resolvedPath = folder;
                    break;
                }
            }
            catch
            {
                // unreadable folder - try the next one
            }
        }

        try
        {
            // An empty RootPath leaves the lookup to the system loader (PATH / LD_LIBRARY_PATH /
            // dyld), which is the normal case on Linux where FFmpeg is a distro package.
            ffmpeg.RootPath = _resolvedPath;
            var version = ffmpeg.av_version_info();
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }

            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_QUIET);
            return true;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"FFmpeg libraries not available: {exception.Message}");
            return false;
        }
    }

    /// <summary>Human readable text for an FFmpeg error code.</summary>
    public static unsafe string ErrorText(int error)
    {
        const int bufferSize = 1024;
        var buffer = stackalloc byte[bufferSize];
        ffmpeg.av_strerror(error, buffer, bufferSize);
        return Marshal.PtrToStringAnsi((IntPtr)buffer) ?? $"error {error}";
    }
}
