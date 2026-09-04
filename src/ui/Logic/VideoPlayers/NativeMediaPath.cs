using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers;

/// <summary>
/// Prepares file paths for the native players (libmpv, libvlc) on Windows.
/// <para>
/// Both libraries open media through the Win32 file APIs with the path as given, and those
/// cap a plain path at MAX_PATH (260 chars) unless the host exe declares itself long-path
/// aware and Windows has LongPathsEnabled turned on - neither is a given on a user's machine.
/// The failure is silent: the file never loads, the duration stays 0:00 and Play does nothing.
/// ffmpeg adds the extended-length prefix itself, so the waveform still gets extracted, which
/// is what made the symptom look like a broken container (#14407).
/// </para>
/// </summary>
public static class NativeMediaPath
{
    /// <summary>Win32 MAX_PATH, counting the terminating NUL - so 259 chars is the longest plain path.</summary>
    public const int WindowsMaxPath = 260;

    private const string LongPathPrefix = @"\\?\";
    private const string UncLongPathPrefix = @"\\?\UNC\";

    /// <summary>
    /// The path to hand to libmpv's "loadfile". mpv passes it straight to CreateFileW, which
    /// accepts the <c>\\?\</c> extended-length form regardless of manifest or registry, so long
    /// local paths get that prefix. Everything else (short paths, URLs, non-Windows) is returned
    /// unchanged.
    /// </summary>
    public static string ForMpv(string path)
    {
        if (!TryGetLongLocalPath(path, out var fullPath))
        {
            return path;
        }

        return AddLongPathPrefix(fullPath);
    }

    /// <summary>
    /// The path to hand to libvlc_media_new_path. VLC converts the path to a file URI first and
    /// reads <c>\\?\C:\...</c> as a UNC path with host "?", so the prefix cannot be used there.
    /// Fall back to the 8.3 short name instead, which fits under MAX_PATH whenever the volume
    /// keeps short names (the default on the system drive). If no short enough name can be
    /// produced the original path is returned and VLC gets the same chance it always had - the
    /// app manifest is long-path aware, so it works on machines with LongPathsEnabled.
    /// </summary>
    public static string ForVlc(string path)
    {
        if (!TryGetLongLocalPath(path, out var fullPath))
        {
            return path;
        }

        var shortPath = GetShortPath(AddLongPathPrefix(fullPath));
        if (string.IsNullOrEmpty(shortPath))
        {
            return path;
        }

        var plainShortPath = RemoveLongPathPrefix(shortPath);
        return plainShortPath.Length < WindowsMaxPath ? plainShortPath : path;
    }

    /// <summary>
    /// True (with the normalized full path) for a local Windows path too long to open without
    /// the extended-length prefix. URLs and already-prefixed paths are left alone.
    /// </summary>
    private static bool TryGetLongLocalPath(string path, out string fullPath)
    {
        fullPath = path;
        if (!OperatingSystem.IsWindows() ||
            string.IsNullOrWhiteSpace(path) ||
            path.Contains("://", StringComparison.Ordinal) ||
            path.StartsWith(LongPathPrefix, StringComparison.Ordinal) ||
            path.StartsWith(@"\\.\", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            // The extended-length form turns off Win32 path normalization, so it must be
            // applied to a fully qualified path with backslashes and no "." / ".." segments -
            // GetFullPath produces exactly that. A short relative path can expand past the
            // limit, hence the length check on the full path.
            fullPath = Path.GetFullPath(path);
        }
        catch
        {
            return false;
        }

        return fullPath.Length >= WindowsMaxPath;
    }

    /// <summary>
    /// <c>C:\dir\file</c> becomes <c>\\?\C:\dir\file</c>; a UNC path <c>\\server\share\file</c>
    /// becomes <c>\\?\UNC\server\share\file</c>.
    /// </summary>
    internal static string AddLongPathPrefix(string fullPath)
    {
        if (fullPath.StartsWith(LongPathPrefix, StringComparison.Ordinal))
        {
            return fullPath;
        }

        if (fullPath.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return UncLongPathPrefix + fullPath.Substring(2);
        }

        return LongPathPrefix + fullPath;
    }

    internal static string RemoveLongPathPrefix(string path)
    {
        if (path.StartsWith(UncLongPathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return @"\\" + path.Substring(UncLongPathPrefix.Length);
        }

        if (path.StartsWith(LongPathPrefix, StringComparison.Ordinal))
        {
            return path.Substring(LongPathPrefix.Length);
        }

        return path;
    }

    /// <summary>
    /// 8.3 short form of a path, or null when Windows cannot produce one (short names disabled
    /// on the volume, file missing, API failure). The input should carry the <c>\\?\</c> prefix
    /// so the lookup itself is not subject to MAX_PATH; the result carries it too.
    /// </summary>
    private static string? GetShortPath(string prefixedFullPath)
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

        try
        {
            var buffer = new char[prefixedFullPath.Length + 1];
            var length = GetShortPathNameW(prefixedFullPath, buffer, (uint)buffer.Length);
            if (length > buffer.Length)
            {
                buffer = new char[length];
                length = GetShortPathNameW(prefixedFullPath, buffer, (uint)buffer.Length);
            }

            if (length == 0 || length > buffer.Length)
            {
                return null;
            }

            return new string(buffer, 0, (int)length);
        }
        catch
        {
            return null;
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetShortPathNameW(string lpszLongPath, [Out] char[] lpszShortPath, uint cchBuffer);
}
