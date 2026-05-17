using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IYtDlpDownloadService
{
    Task DownloadYtDlp(IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadVideo(string url, string outputPath, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class YtDlpDownloadService : IYtDlpDownloadService
{
    private readonly HttpClient _httpClient;
    internal const string CurrentVersion = "2026.03.17";
    // First-run extraction of the self-extracting yt-dlp bundle (especially
    // yt-dlp_macos) routinely needs more than 5s on slower disks; bumped to 15s
    // so the timeout doesn't masquerade as "version unknown / outdated".
    private static readonly TimeSpan VersionCheckTimeout = TimeSpan.FromSeconds(15);
    private const string WindowsUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/" + CurrentVersion + "/yt-dlp.exe";
    private const string LinuxUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/" + CurrentVersion + "/yt-dlp_linux";
    private const string LinuxArmUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/" + CurrentVersion + "/yt-dlp_linux_aarch64";
    private const string MacUrl = "https://github.com/yt-dlp/yt-dlp/releases/download/" + CurrentVersion + "/yt-dlp_macos";

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
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), GetFullFileName(), progress, cancellationToken);
    }

    private static readonly Regex PercentageRegex = new(@"(?<pct>\d+(?:\.\d+)?)\s*%", RegexOptions.Compiled);

    public async Task DownloadVideo(string url, string outputPath, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(GetFullFileName()))
        {
            throw new InvalidOperationException("yt-dlp is not installed");
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = GetFullFileName(),
                ArgumentList =
                {
                    "--newline",       // emit one progress line per update, never carriage-return rewriting
                    "--no-mtime",      // don't carry remote mtime to the local file
                    "--no-playlist",   // single video only — playlist URLs would download many files
                    "-o", outputPath,
                    url,
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = true,
        };

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

    private static readonly object _versionCheckCacheLock = new();
    private static bool? _cachedIsOutdated;

    public static async Task<bool> IsInstalledVersionOutdated(CancellationToken cancellationToken, bool forceRefresh = false)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!forceRefresh)
        {
            lock (_versionCheckCacheLock)
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

        lock (_versionCheckCacheLock)
        {
            _cachedIsOutdated = result;
        }

        return result;
    }

    public static void InvalidateInstalledVersionCache()
    {
        lock (_versionCheckCacheLock)
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
