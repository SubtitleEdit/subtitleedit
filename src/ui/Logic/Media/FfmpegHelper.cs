using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Logic.Media;

public static class FfmpegHelper
{
    // SE bundles/downloads ffmpeg 7/8 and is verified against it, so require a system/PATH ffmpeg
    // to be at least this major version. Older builds are ignored (SE offers to download instead)
    // rather than used and failing later.
    internal const int MinimumFfmpegMajorVersion = 7;

    // "ffmpeg version 6.1.1", "ffmpeg version n7.0", "ffmpeg version 4.4.2-0ubuntu0.22.04.1".
    private static readonly Regex FfmpegVersionRegex =
        new(@"ffmpeg version n?(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // A line of "ffmpeg -encoders": six flag columns ("V....D", "A..X.."), then the encoder name,
    // then a description. The legend lines above the list ("V..... = Video") have "=" where the
    // name would be and so do not match.
    private static readonly Regex FfmpegEncoderRegex =
        new(@"^\s*[VAS][A-Z.]{5}\s+([A-Za-z0-9_.\-]+)(\s|$)", RegexOptions.Compiled);

    private static readonly IReadOnlySet<string> EmptyEncoders = new HashSet<string>(StringComparer.Ordinal);
    private static readonly object AvailableEncodersLock = new();
    private static string? _availableEncodersFor;
    private static HashSet<string>? _availableEncoders;

    public static bool IsFfmpegInstalled()
    {
        if (OperatingSystem.IsLinux())
        {
            return true;
        }


        if (File.Exists(Se.Settings.General.FfmpegPath))
        {
            return true;
        }

        // ffmpeg may already be installed and on the system PATH (e.g. the bundled download
        // failed but the user has ffmpeg). Use it instead of hanging or forcing another
        // download - issue #11760.
        var systemFfmpeg = GetSystemFfmpegPath();
        if (!string.IsNullOrEmpty(systemFfmpeg))
        {
            SetFfmpegPath(systemFfmpeg);
            return true;
        }

        return false;
    }

    /// <summary>
    /// The ffmpeg that will actually be run: the configured path, falling back to plain "ffmpeg"
    /// (resolved through PATH) on macOS/Linux, where a system ffmpeg is the normal case.
    /// </summary>
    public static string GetFfmpegLocation()
    {
        var ffmpegLocation = Configuration.Settings.General.FFmpegLocation;
        if (!Configuration.IsRunningOnWindows && (string.IsNullOrEmpty(ffmpegLocation) || !File.Exists(ffmpegLocation)))
        {
            ffmpegLocation = "ffmpeg";
        }

        return ffmpegLocation;
    }

    /// <summary>
    /// The encoder names the running ffmpeg was built with, as reported by "ffmpeg -encoders", or
    /// an empty set when the probe fails. Callers must treat "empty" as "unknown", not as "nothing
    /// is available" - see <see cref="GetAvailableEncoders"/> callers.
    /// <para>
    /// Which encoders exist varies a lot per build: the Flatpak bundles its own ffmpeg without
    /// x265/NVENC/AMF/QSV, and distro packages differ again. Offering a codec that is not there
    /// only produces an "Unknown encoder" failure once the job is already running.
    /// </para>
    /// The result is cached per ffmpeg path, since it cannot change under a running ffmpeg and
    /// every probe costs a process launch.
    /// </summary>
    public static IReadOnlySet<string> GetAvailableEncoders()
    {
        var ffmpegPath = GetFfmpegLocation();
        if (string.IsNullOrEmpty(ffmpegPath))
        {
            // No ffmpeg configured yet (SE offers to download it) - probing would only log a
            // failure every time a burn-in window opens.
            return EmptyEncoders;
        }

        lock (AvailableEncodersLock)
        {
            if (_availableEncoders != null && _availableEncodersFor == ffmpegPath)
            {
                return _availableEncoders;
            }
        }

        var encoders = ParseEncoderNames(ProbeEncodersOutput(ffmpegPath));
        lock (AvailableEncodersLock)
        {
            // Only cache a successful probe - a transient failure should not hide encoders for
            // the rest of the session.
            if (encoders.Count > 0)
            {
                _availableEncoders = encoders;
                _availableEncodersFor = ffmpegPath;
            }
        }

        return encoders;
    }

    private static string ProbeEncodersOutput(string ffmpegPath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("-hide_banner");
            psi.ArgumentList.Add("-encoders");

            using var process = Process.Start(psi);
            if (process == null)
            {
                return string.Empty;
            }

            // The encoder list is tens of kilobytes, far past the 4 KB Windows pipe buffer, so
            // both streams must be drained while ffmpeg is still writing - waiting first and
            // reading after would deadlock.
            var stdOut = process.StandardOutput.ReadToEndAsync();
            var stdErr = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(5000))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
                return string.Empty;
            }

            // Lets the async readers finish after the timed wait returned (documented pattern).
            process.WaitForExit();

            return stdOut.Result + "\n" + stdErr.Result;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"FfmpegHelper: failed to probe ffmpeg encoders at '{ffmpegPath}'");
            return string.Empty;
        }
    }

    /// <summary>
    /// Parses the encoder names out of an "ffmpeg -encoders" listing. Pure - exposed for testing.
    /// </summary>
    internal static HashSet<string> ParseEncoderNames(string encodersOutput)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrEmpty(encodersOutput))
        {
            return names;
        }

        foreach (var line in encodersOutput.Split('\n'))
        {
            var match = FfmpegEncoderRegex.Match(line);
            if (match.Success)
            {
                names.Add(match.Groups[1].Value);
            }
        }

        return names;
    }

    /// <summary>
    /// Sets the ffmpeg path everywhere it is read from: the SE 5 setting and the libse
    /// <see cref="Configuration"/> mirror that the ffmpeg process launchers use. Keeping the two in
    /// sync avoids the case where ffmpeg is resolved but the launcher still sees the old/empty path.
    /// </summary>
    public static void SetFfmpegPath(string path)
    {
        Se.Settings.General.FfmpegPath = path;
        Configuration.Settings.General.FFmpegLocation = path;
    }

    /// <summary>
    /// Returns the full path of a usable "ffmpeg" / "ffmpeg.exe" found on the system PATH, or an
    /// empty string. Scans the PATH variable directly (never spawns a process to find candidates,
    /// so the scan cannot hang) and accepts a candidate only if its version meets the minimum.
    /// </summary>
    public static string GetSystemFfmpegPath()
    {
        var exeName = OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";
        var pathValue = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathValue))
        {
            return string.Empty;
        }

        foreach (var dir in pathValue.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(dir))
            {
                continue;
            }

            try
            {
                var candidate = Path.Combine(dir.Trim(), exeName);
                if (File.Exists(candidate) && IsSupportedVersion(candidate))
                {
                    return candidate;
                }
            }
            catch
            {
                // Ignore malformed PATH entries (illegal path characters, etc.).
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Runs "&lt;ffmpeg&gt; -version" and returns true unless we positively detect a major version
    /// below <see cref="MinimumFfmpegMajorVersion"/>. If the probe fails or the version can't be
    /// parsed (e.g. a nightly "N-..." build or custom banner) we assume it's usable rather than
    /// reject a working ffmpeg. The probe is bounded by a timeout and kills the process on overrun,
    /// so it cannot hang.
    /// </summary>
    public static bool IsSupportedVersion(string ffmpegPath)
    {
        var major = GetMajorVersion(ffmpegPath);
        return major == null || major.Value >= MinimumFfmpegMajorVersion;
    }

    private static int? GetMajorVersion(string ffmpegPath)
    {
        return ParseMajorVersion(ProbeVersionOutput(ffmpegPath));
    }

    /// <summary>
    /// Returns the first line of "&lt;ffmpeg&gt; -version" (e.g. "ffmpeg version 7.1.1-full_build ..."),
    /// or an empty string when the probe fails. Intended for diagnostics logging, so a bug report's
    /// tools log shows which ffmpeg build actually ran (#12093).
    /// </summary>
    public static string GetVersionBanner(string ffmpegPath)
    {
        var output = ProbeVersionOutput(ffmpegPath);
        foreach (var line in output.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0)
            {
                return trimmed;
            }
        }

        return string.Empty;
    }

    private static string ProbeVersionOutput(string ffmpegPath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("-version");

            using var process = Process.Start(psi);
            if (process == null)
            {
                return string.Empty;
            }

            // "-version" prints a short banner and exits immediately; 4 s is generous.
            if (!process.WaitForExit(4000))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* best-effort */ }
                return string.Empty;
            }

            return process.StandardOutput.ReadToEnd() + "\n" + process.StandardError.ReadToEnd();
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"FfmpegHelper: failed to probe ffmpeg version at '{ffmpegPath}'");
            return string.Empty;
        }
    }

    /// <summary>
    /// Parses the ffmpeg major version out of an "ffmpeg -version" banner, or null if it cannot be
    /// determined (nightly "N-..." builds, custom banners). Pure - exposed for testing.
    /// </summary>
    internal static int? ParseMajorVersion(string versionOutput)
    {
        if (string.IsNullOrEmpty(versionOutput))
        {
            return null;
        }

        var match = FfmpegVersionRegex.Match(versionOutput);
        return match.Success && int.TryParse(match.Groups[1].Value, out var major) ? major : null;
    }
}
