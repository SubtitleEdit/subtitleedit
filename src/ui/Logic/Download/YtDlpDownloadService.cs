using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IYtDlpDownloadService
{
    Task DownloadYtDlp(IProgress<float>? progress, CancellationToken cancellationToken);

    /// <summary>
    /// Downloads <paramref name="url"/> to <paramref name="outputPath"/>. When
    /// <paramref name="downloadAllSubtitles"/> is true, also writes every
    /// human-uploaded subtitle track as a sibling file
    /// (<c>{stem}.{lang}.{ext}</c>) using the same single yt-dlp invocation.
    /// </summary>
    Task DownloadVideo(string url, string outputPath, bool downloadAllSubtitles, IProgress<float>? progress, CancellationToken cancellationToken);

    /// <summary>
    /// Subtitle-only download (no video). Writes every human-uploaded subtitle
    /// track to <paramref name="outputStem"/>.<c>{lang}.{ext}</c> via a single
    /// yt-dlp invocation. Caller is responsible for cleaning up the directory.
    /// </summary>
    Task DownloadAllSubtitlesAsync(string url, string outputStem, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches the video's title via <c>yt-dlp --print title</c> so downloads
    /// can be suggested under the video's real name instead of a URL fragment
    /// (a YouTube link's last path segment is just "watch"). Best-effort:
    /// returns null on any failure or after a short internal timeout so
    /// callers can fall back to a URL-derived name.
    /// </summary>
    Task<string?> GetVideoTitleAsync(string url, CancellationToken cancellationToken);
}

/// <summary>
/// A subtitle file that yt-dlp wrote to disk. Constructed from the resulting
/// filename — yt-dlp uses <c>{stem}.{lang}.{ext}</c>.
/// </summary>
public sealed record DownloadedSubtitleInfo(string FilePath, string LanguageCode, string Format);

public class YtDlpDownloadService : IYtDlpDownloadService
{
    private readonly HttpClient _httpClient;
    internal const string CurrentVersion = "2026.07.04";
    // First-run extraction of the self-extracting yt-dlp bundle (especially
    // yt-dlp_macos) routinely needs more than 5s on slower disks; bumped to 15s
    // so the timeout doesn't masquerade as "version unknown / outdated".
    private static readonly TimeSpan VersionCheckTimeout = TimeSpan.FromSeconds(15);
    private const string WindowsUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/" + CurrentVersion + "/yt-dlp.exe";
    private const string LinuxUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/" + CurrentVersion + "/yt-dlp_linux";
    private const string LinuxArmUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/" + CurrentVersion + "/yt-dlp_linux_aarch64";
    private const string MacUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/" + CurrentVersion + "/yt-dlp_macos";

    // Official SHA-256 checksums copied verbatim from each release's
    // SHA2-256SUMS file (https://github.com/yt-dlp/yt-dlp/releases). Keyed by
    // version then by asset file name. The previous version is retained
    // alongside the current one so a binary already on disk can be validated
    // regardless of which of the two it is, not just freshly downloaded ones.
    internal static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> KnownSha256 =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            ["2026.06.09"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["yt-dlp.exe"] = "3a48cb955d55c8821b60ccbdbbc6f61bc958f2f3d3b7ad5eaf3d83a543293a27",
                ["yt-dlp_linux"] = "bf8aac79b72287a6d2043074415132558b43743a8f9461a22b0141e90f16ce66",
                ["yt-dlp_linux_aarch64"] = "cabd246445bdfde0eda0dfe68bbe90354be83f3fdbbf077df11a2ea55f41cdbd",
                ["yt-dlp_macos"] = "b82c3626952e6c14eaf654cc565866775ffd0b9ffb7021628ac59b42c2f4f244",
            },
            ["2026.07.04"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["yt-dlp.exe"] = "52fe3c26dcf71fbdc85b528589020bb0b8e383155cfa81b64dd447bbe35e24b8",
                ["yt-dlp_linux"] = "6bbb3d314cde4febe36e5fa1d55462e29c974f63444e707871834f6d8cc210ae",
                ["yt-dlp_linux_aarch64"] = "b6ce97646773070d7a7ffd6bbbdcaecb47c48483909c54c915bf08a7a9b5e0b1",
                ["yt-dlp_macos"] = "498bd0dae17855c599d371d68ec5bafc439a9d8640e838be25c765a9792f261b",
            },
        };

    public YtDlpDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public static string GetFullFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Se.DataFolder, "yt-dlp.exe");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? Path.Combine(Se.DataFolder, "yt-dlp_linux_aarch64")
                : Path.Combine(Se.DataFolder, "yt-dlp_linux");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return Path.Combine(Se.DataFolder, "yt-dlp_macos");
        }

        throw new PlatformNotSupportedException("Unsupported OS platform");
    }
    private static string GetUrl()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? LinuxArmUrl : LinuxUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacUrl;
        }

        throw new PlatformNotSupportedException("Unsupported OS platform");
    }

    public async Task DownloadYtDlp(IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var fileName = GetFullFileName();
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), fileName, progress, cancellationToken);
        await VerifyChecksumAsync(fileName, CurrentVersion, cancellationToken);
    }

    /// <summary>
    /// Verifies <paramref name="filePath"/> against the official SHA-256
    /// checksum for <paramref name="version"/>. A tampered, truncated, or
    /// otherwise corrupt download is deleted and surfaced as an error instead
    /// of being executed. If no checksum is on record for the asset, this is a
    /// no-op — we don't block on data we don't have.
    /// </summary>
    internal static async Task VerifyChecksumAsync(string filePath, string version, CancellationToken cancellationToken)
    {
        var assetName = Path.GetFileName(filePath);
        if (!KnownSha256.TryGetValue(version, out var byAsset) ||
            !byAsset.TryGetValue(assetName, out var expected))
        {
            return;
        }

        var actual = await ComputeSha256Async(filePath, cancellationToken);
        if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
        {
            TryDeleteFile(filePath);
            throw new InvalidOperationException(
                $"Downloaded yt-dlp ({assetName}) failed SHA-256 verification — expected {expected}, got {actual}. The file has been removed.");
        }
    }

    internal static async Task<string> ComputeSha256Async(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // Best-effort cleanup; a leftover bad file is re-checked on next download.
        }
    }

    private static readonly Regex PercentageRegex = new(@"(?<pct>\d+(?:\.\d+)?)\s*%", RegexOptions.Compiled);

    public async Task DownloadVideo(string url, string outputPath, bool downloadAllSubtitles, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(GetFullFileName()))
        {
            throw new InvalidOperationException("yt-dlp is not installed");
        }

        var args = new List<string>
        {
            "--newline",       // emit one progress line per update, never carriage-return rewriting
            "--no-mtime",      // don't carry remote mtime to the local file
            "--no-playlist",   // single video only — playlist URLs would download many files
            "-o", outputPath,
        };

        if (downloadAllSubtitles)
        {
            // Ride along with the video download: yt-dlp writes every available
            // human-uploaded subtitle track as a sibling file using the same
            // <stem>.<lang>.<ext> naming so the caller can find them by globbing.
            args.Add("--write-subs");
            args.Add("--sub-langs");
            args.Add("all");
            args.Add("--sub-format");
            args.Add("vtt/best");
        }

        args.Add(url);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GetFullFileName(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };
        foreach (var a in args) process.StartInfo.ArgumentList.Add(a);

        var stderrBuffer = new System.Text.StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null)
            {
                return;
            }

            ReportProgressFromLine(e.Data, progress);
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null)
            {
                return;
            }

            stderrBuffer.AppendLine(e.Data);
            ReportProgressFromLine(e.Data, progress);
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start yt-dlp");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            TryKillProcess(process);
            throw;
        }

        if (process.ExitCode != 0)
        {
            var details = stderrBuffer.ToString().Trim();
            throw new InvalidOperationException(
                $"yt-dlp exited with code {process.ExitCode}." +
                (string.IsNullOrEmpty(details) ? string.Empty : Environment.NewLine + details));
        }

        progress?.Report(1f);
    }

    public async Task DownloadAllSubtitlesAsync(string url, string outputStem, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(GetFullFileName()))
        {
            throw new InvalidOperationException("yt-dlp is not installed");
        }

        var directory = Path.GetDirectoryName(outputStem);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var args = new List<string>
        {
            "--skip-download",
            "--no-warnings",
            "--no-playlist",
            "--write-subs",
            "--sub-langs", "all",
            "--sub-format", "vtt/best",
            "-o", outputStem,
            url,
        };

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GetFullFileName(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };
        foreach (var a in args) process.StartInfo.ArgumentList.Add(a);

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start yt-dlp");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            TryKillProcess(process);
            throw;
        }

        await stdoutTask;
        if (process.ExitCode != 0)
        {
            var details = (await stderrTask).Trim();
            throw new InvalidOperationException(
                $"yt-dlp subtitle download exited with code {process.ExitCode}." +
                (string.IsNullOrEmpty(details) ? string.Empty : Environment.NewLine + details));
        }
    }

    // The title fetch is a live network round-trip to the video site; cap it so
    // a stalled extractor can't leave the save-as flow stuck behind a spinner.
    private static readonly TimeSpan TitleFetchTimeout = TimeSpan.FromSeconds(15);

    public async Task<string?> GetVideoTitleAsync(string url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!File.Exists(GetFullFileName()))
        {
            return null;
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GetFullFileName(),
                // --print implies --simulate/--quiet, so nothing is downloaded.
                ArgumentList = { "--no-playlist", "--no-warnings", "--print", "title", url },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        if (!process.Start())
        {
            return null;
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TitleFetchTimeout);
        var operationToken = timeoutCts.Token;
        var stdoutTask = process.StandardOutput.ReadToEndAsync(operationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(operationToken);

        try
        {
            try
            {
                await process.WaitForExitAsync(operationToken);
            }
            catch (OperationCanceledException)
            {
                TryKillProcess(process);
                cancellationToken.ThrowIfCancellationRequested(); // user cancellation propagates; timeout falls through
                return null;
            }

            if (process.ExitCode != 0)
            {
                return null;
            }

            return ExtractTitle(await stdoutTask);
        }
        finally
        {
            await ObserveProcessOutput(stdoutTask);
            await ObserveProcessOutput(stderrTask);
        }
    }

    /// <summary>
    /// Pulls the title from <c>--print title</c> output. The last non-empty
    /// line wins — the macOS self-extracting bundle can print bootstrap noise
    /// before the actual value (same caveat as <see cref="ExtractVersion"/>).
    /// </summary>
    internal static string? ExtractTitle(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        var lines = output.Split('\n');
        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var line = lines[i].Trim();
            if (line.Length > 0)
            {
                return line;
            }
        }

        return null;
    }

    /// <summary>
    /// Scans <paramref name="directory"/> for files yt-dlp produced via its
    /// <c>{stem}.{lang}.{ext}</c> sidecar naming. Filters to subtitle extensions
    /// so the video file (and any unrelated stem.* files) are excluded.
    /// </summary>
    public static IReadOnlyList<DownloadedSubtitleInfo> EnumerateDownloadedSubtitles(string directory, string stem)
    {
        var results = new List<DownloadedSubtitleInfo>();
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory) || string.IsNullOrEmpty(stem))
        {
            return results;
        }

        var prefix = stem + ".";
        foreach (var path in Directory.EnumerateFiles(directory, stem + ".*"))
        {
            var fileName = Path.GetFileName(path);
            if (!fileName.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            var rest = fileName.Substring(prefix.Length);
            var dotIdx = rest.LastIndexOf('.');
            if (dotIdx <= 0)
            {
                // No language segment — that's the video file (or other) itself.
                continue;
            }

            var lang = rest.Substring(0, dotIdx);
            var ext = rest.Substring(dotIdx + 1);
            if (!IsSubtitleExtension(ext))
            {
                continue;
            }

            results.Add(new DownloadedSubtitleInfo(path, lang, ext));
        }

        results.Sort(static (a, b) => string.Compare(a.LanguageCode, b.LanguageCode, StringComparison.OrdinalIgnoreCase));
        return results;
    }

    private static bool IsSubtitleExtension(string ext) => ext switch
    {
        "vtt" or "srt" or "ttml" or "ass" or "ssa" or "sbv" or "lrc"
            or "srv1" or "srv2" or "srv3" or "json3" or "ytt" => true,
        _ => false,
    };

    private static void ReportProgressFromLine(string line, IProgress<float>? progress)
    {
        if (progress is null || !line.Contains('%'))
        {
            return;
        }

        var match = PercentageRegex.Match(line);
        if (!match.Success)
        {
            return;
        }

        if (!float.TryParse(match.Groups["pct"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var pct))
        {
            return;
        }

        var clamped = Math.Clamp(pct / 100f, 0f, 1f);
        progress.Report(clamped);
    }

    private static readonly Lock VersionCheckCacheLock = new();
    private static bool? _cachedIsOutdated;

    public static async Task<bool> IsInstalledVersionOutdated(CancellationToken cancellationToken, bool forceRefresh = false)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!forceRefresh)
        {
            lock (VersionCheckCacheLock)
            {
                if (_cachedIsOutdated is { } cached)
                {
                    return cached;
                }
            }
        }

        bool result;
        if (!File.Exists(GetFullFileName()))
        {
            result = true;
        }
        else
        {
            string? installedVersion;
            try
            {
                installedVersion = await GetInstalledVersion(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Se.LogError(ex, "Failed to determine installed yt-dlp version; treating as outdated.");
                return true; // don't cache failures — antivirus blips etc. shouldn't poison the session
            }

            result = IsVersionOutdated(installedVersion);
        }

        lock (VersionCheckCacheLock)
        {
            _cachedIsOutdated = result;
        }

        return result;
    }

    public static void InvalidateInstalledVersionCache()
    {
        lock (VersionCheckCacheLock)
        {
            _cachedIsOutdated = null;
        }
    }

    private static readonly Regex VersionLineRegex = new(@"^\s*v?(\d+\.\d+(?:\.\d+){0,2})\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    /// <summary>
    /// Extracts a YYYY.MM[.DD[.N]] version string from <c>yt-dlp --version</c>
    /// output. Scans all lines instead of taking the first because the macOS
    /// self-extracting bundle can print bootstrap/extraction noise on first run
    /// before the actual version line. Returns null if no line in the output
    /// matches a version shape.
    /// </summary>
    internal static string? ExtractVersion(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        var match = VersionLineRegex.Match(output);
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Returns true only when we can confirm the installed version is older than
    /// <see cref="CurrentVersion"/>. Anything we can't parse — null, empty, weird
    /// pre-release suffix, extraction-noise output — returns false. Nagging the
    /// user to redownload every session because <c>--version</c> printed something
    /// we didn't expect is worse than trusting the binary and letting the next
    /// real yt-dlp call surface a concrete error.
    /// </summary>
    internal static bool IsVersionOutdated(string? installedVersion)
    {
        if (string.IsNullOrWhiteSpace(installedVersion) ||
            !Version.TryParse(installedVersion.Trim(), out var parsedInstalledVersion) ||
            !Version.TryParse(CurrentVersion, out var parsedCurrentVersion))
        {
            return false;
        }

        return parsedInstalledVersion < parsedCurrentVersion;
    }

    private static async Task<string?> GetInstalledVersion(CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GetFullFileName(),
                ArgumentList = { "--version" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        if (!process.Start())
        {
            return null;
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(VersionCheckTimeout);
        var operationToken = timeoutCts.Token;
        var stdoutTask = process.StandardOutput.ReadToEndAsync(operationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(operationToken);

        try
        {
            try
            {
                await process.WaitForExitAsync(operationToken);
            }
            catch (OperationCanceledException)
            {
                TryKillProcess(process);
                cancellationToken.ThrowIfCancellationRequested(); // user cancellation propagates; timeout falls through
                return null;
            }

            if (process.ExitCode != 0)
            {
                return null;
            }

            var output = await stdoutTask;
            return ExtractVersion(output);
        }
        finally
        {
            await ObserveProcessOutput(stdoutTask);
            await ObserveProcessOutput(stderrTask);
        }
    }

    private static void TryKillProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(true);
            }
        }
        catch
        {
            // Ignore failures killing an already-exited or inaccessible process.
        }
    }

    private static async Task ObserveProcessOutput(Task outputTask)
    {
        try
        {
            await outputTask;
        }
        catch
        {
            // Ignore output read failures after the process has been killed.
        }
    }
}
