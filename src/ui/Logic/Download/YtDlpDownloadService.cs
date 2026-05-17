using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IYtDlpDownloadService
{
    Task DownloadYtDlp(IProgress<float>? progress, CancellationToken cancellationToken);
}

public class YtDlpDownloadService : IYtDlpDownloadService
{
    private readonly HttpClient _httpClient;
    internal const string CurrentVersion = "2026.03.17";
    private static readonly TimeSpan VersionCheckTimeout = TimeSpan.FromSeconds(5);
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
        var path = Path.Combine(GetFullFileName());
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), path, progress, cancellationToken);
    }

    public static async Task<bool> IsInstalledVersionOutdated(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!File.Exists(GetFullFileName()))
        {
            return true;
        }

        string? installedVersion;
        try
        {
            installedVersion = await GetInstalledVersion(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return true;
        }

        return IsVersionOutdated(installedVersion);
    }

    internal static bool IsVersionOutdated(string? installedVersion)
    {
        if (string.IsNullOrWhiteSpace(installedVersion) ||
            !Version.TryParse(installedVersion.Trim(), out var parsedInstalledVersion) ||
            !Version.TryParse(CurrentVersion, out var parsedCurrentVersion))
        {
            return true;
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
            var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return lines.Length == 0 ? null : lines[0];
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
