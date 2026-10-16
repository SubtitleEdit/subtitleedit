using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

/// <summary>How this copy of Subtitle Edit can update itself, if at all.</summary>
public enum AutoUpdateKind
{
    /// <summary>No in-app update - the user gets the download page (zip, dmg, Flatpak, AUR, ...).</summary>
    None,

    /// <summary>Installed by the Inno Setup installer: run the new installer silently.</summary>
    WindowsInstaller,

    /// <summary>Unpacked Linux tarball in a user-writable folder: extract the new tarball over it.</summary>
    LinuxTarball,
}

public class AutoUpdateAsset
{
    public string Name { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
}

public interface IAutoUpdateService
{
    AutoUpdateKind Kind { get; }

    /// <summary>Downloads the release file for this platform and verifies its SHA-256; returns the local path.</summary>
    Task<string> DownloadAsync(string version, IProgress<float>? progress, CancellationToken cancellationToken);

    /// <summary>
    /// Starts a detached helper that waits for this process to exit, installs the downloaded
    /// update and starts Subtitle Edit again. The caller then closes the editor windows.
    /// </summary>
    void StartInstallAfterExit(string downloadedFile);
}

/// <summary>
/// In-app update for the two packages where swapping in a new version is simple: the Windows
/// x64 installer (re-run silently - Inno keeps the install folder, language and tasks) and the
/// Linux tarball (extracted over the old folder once Subtitle Edit has exited). The file comes
/// from the GitHub release whose tag is the new version, and must match the SHA-256 digest
/// GitHub publishes for that release asset - no digest, no install.
/// </summary>
public class AutoUpdateService : IAutoUpdateService
{
    private const string ReleaseApiUrl = "https://api.github.com/repos/SubtitleEdit/subtitleedit/releases/tags/";
    internal const string DownloadUrlPrefix = "https://github.com/SubtitleEdit/subtitleedit/releases/download/";
    internal const string WindowsInstallerAsset = "SubtitleEdit-Windows-x64-Setup.exe";
    internal const string LinuxX64Asset = "SubtitleEdit-Linux-x64.tar.gz";
    internal const string LinuxArm64Asset = "SubtitleEdit-Linux-ARM64.tar.gz";
    internal const string LinuxExecutable = "SubtitleEdit";

    private readonly HttpClient _httpClient;
    private readonly Lazy<AutoUpdateKind> _kind;

    public AutoUpdateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _kind = new Lazy<AutoUpdateKind>(DetectCurrentKind);
    }

    public AutoUpdateKind Kind => _kind.Value;

    private static AutoUpdateKind DetectCurrentKind()
    {
        try
        {
            return DetectKind(
                OperatingSystem.IsWindows(),
                OperatingSystem.IsLinux(),
                RuntimeInformation.ProcessArchitecture,
                UpdateCheckService.IsStoreManagedInstall ||
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPIMAGE")),
                AppContext.BaseDirectory,
                File.Exists,
                IsDirectoryWritable);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "AutoUpdateService.DetectCurrentKind");
            return AutoUpdateKind.None;
        }
    }

    internal static AutoUpdateKind DetectKind(
        bool isWindows,
        bool isLinux,
        Architecture architecture,
        bool isPackageManaged,
        string exeFolder,
        Func<string, bool> fileExists,
        Func<string, bool> isDirectoryWritable)
    {
        if (isPackageManaged)
        {
            return AutoUpdateKind.None;
        }

        // Only the Inno Setup install leaves an uninstaller next to the exe; the portable
        // zip (and the ARM64 build, which only ships as a zip) keep the download page.
        if (isWindows)
        {
            return architecture == Architecture.X64 && fileExists(Path.Combine(exeFolder, "unins000.exe"))
                ? AutoUpdateKind.WindowsInstaller
                : AutoUpdateKind.None;
        }

        // A distro package (e.g. AUR in /usr/lib) is not writable for the user and is
        // updated by its package manager - only an unpacked tarball in a writable folder.
        if (isLinux &&
            architecture is Architecture.X64 or Architecture.Arm64 &&
            fileExists(Path.Combine(exeFolder, LinuxExecutable)) &&
            (fileExists("/usr/bin/tar") || fileExists("/bin/tar")) &&
            isDirectoryWritable(exeFolder))
        {
            return AutoUpdateKind.LinuxTarball;
        }

        return AutoUpdateKind.None;
    }

    internal static string GetAssetName(AutoUpdateKind kind, Architecture architecture)
    {
        return kind switch
        {
            AutoUpdateKind.WindowsInstaller => WindowsInstallerAsset,
            AutoUpdateKind.LinuxTarball => architecture == Architecture.Arm64 ? LinuxArm64Asset : LinuxX64Asset,
            _ => string.Empty,
        };
    }

    private static bool IsDirectoryWritable(string folder)
    {
        try
        {
            var probe = Path.Combine(folder, ".se-update-probe-" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string> DownloadAsync(string version, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        var assetName = GetAssetName(Kind, RuntimeInformation.ProcessArchitecture);
        if (string.IsNullOrEmpty(assetName))
        {
            throw new InvalidOperationException("Automatic update is not available for this installation");
        }

        var asset = await GetReleaseAssetAsync(version, assetName, cancellationToken);

        var folder = Path.Combine(Path.GetTempPath(), "SubtitleEdit-update");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, asset.Name);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        await DownloadHelper.DownloadFileAsync(_httpClient, asset.DownloadUrl, path, progress, cancellationToken);

        if (!await IsSha256MatchAsync(path, asset.Sha256, cancellationToken))
        {
            TryDelete(path);
            throw new InvalidDataException($"Checksum mismatch for downloaded update {asset.Name}");
        }

        if (Kind == AutoUpdateKind.LinuxTarball)
        {
            await Task.Run(() => ValidateLinuxTarball(path), cancellationToken);
        }

        return path;
    }

    private async Task<AutoUpdateAsset> GetReleaseAssetAsync(string version, string assetName, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        using var request = new HttpRequestMessage(HttpMethod.Get, ReleaseApiUrl + Uri.EscapeDataString(version));
        request.Headers.UserAgent.ParseAdd("SubtitleEdit/" + Se.Version.TrimStart('v'));
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        using var response = await _httpClient.SendAsync(request, cts.Token);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cts.Token);

        return ParseReleaseAsset(json, assetName)
               ?? throw new InvalidOperationException($"Release {version} has no verifiable asset {assetName}");
    }

    /// <summary>
    /// Picks the named asset from a GitHub release JSON. Returns null when it is missing, has no
    /// "sha256:" digest, or its download url is not a SubtitleEdit release url.
    /// </summary>
    internal static AutoUpdateAsset? ParseReleaseAsset(string releaseJson, string assetName)
    {
        using var document = JsonDocument.Parse(releaseJson);
        if (!document.RootElement.TryGetProperty("assets", out var assets) || assets.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (var asset in assets.EnumerateArray())
        {
            if (GetString(asset, "name") != assetName)
            {
                continue;
            }

            var url = GetString(asset, "browser_download_url");
            var digest = GetString(asset, "digest");
            const string sha256Prefix = "sha256:";
            if (!url.StartsWith(DownloadUrlPrefix, StringComparison.Ordinal) ||
                !url.EndsWith("/" + assetName, StringComparison.Ordinal) ||
                !digest.StartsWith(sha256Prefix, StringComparison.OrdinalIgnoreCase) ||
                digest.Length != sha256Prefix.Length + 64)
            {
                return null;
            }

            return new AutoUpdateAsset
            {
                Name = assetName,
                DownloadUrl = url,
                Sha256 = digest.Substring(sha256Prefix.Length).ToLowerInvariant(),
            };
        }

        return null;
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    internal static async Task<bool> IsSha256MatchAsync(string path, string expectedSha256, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return string.Equals(Convert.ToHexString(hash), expectedSha256, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The tarball is extracted straight over the install folder, so refuse one that would write
    /// outside it (absolute paths, "..", links) or that is not a Subtitle Edit build at all.
    /// </summary>
    internal static void ValidateLinuxTarball(string path)
    {
        using var fileStream = File.OpenRead(path);
        using var gzip = new GZipStream(fileStream, CompressionMode.Decompress);
        using var reader = new TarReader(gzip);
        var hasExecutable = false;
        while (reader.GetNextEntry() is { } entry)
        {
            var name = entry.Name;
            if (name.StartsWith('/') || name.Split('/').Contains(".."))
            {
                throw new InvalidDataException("Update archive contains an unsafe path: " + name);
            }

            if (entry.EntryType is TarEntryType.SymbolicLink or TarEntryType.HardLink)
            {
                throw new InvalidDataException("Update archive contains a link: " + name);
            }

            if (name is LinuxExecutable or "./" + LinuxExecutable)
            {
                hasExecutable = true;
            }
        }

        if (!hasExecutable)
        {
            throw new InvalidDataException("Update archive does not contain " + LinuxExecutable);
        }
    }

    public void StartInstallAfterExit(string downloadedFile)
    {
        var processId = Environment.ProcessId;
        var exeFolder = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        var scriptFolder = Path.GetDirectoryName(downloadedFile)!;

        switch (Kind)
        {
            case AutoUpdateKind.WindowsInstaller:
            {
                var script = Path.Combine(scriptFolder, "install-update.ps1");
                File.WriteAllText(script, WindowsScript, new UTF8Encoding(true));
                var startInfo = new ProcessStartInfo("powershell.exe")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                foreach (var argument in new[]
                         {
                             "-NoProfile", "-ExecutionPolicy", "Bypass", "-WindowStyle", "Hidden", "-File", script,
                             "-ProcessId", processId.ToString(), "-Installer", downloadedFile,
                             "-App", Path.Combine(exeFolder, "SubtitleEdit.exe"),
                         })
                {
                    startInfo.ArgumentList.Add(argument);
                }

                Process.Start(startInfo);
                break;
            }
            case AutoUpdateKind.LinuxTarball:
            {
                var script = Path.Combine(scriptFolder, "install-update.sh");
                File.WriteAllText(script, LinuxScript.Replace("\r\n", "\n"));
                var startInfo = new ProcessStartInfo("/bin/sh") { UseShellExecute = false };
                foreach (var argument in new[]
                         {
                             script, processId.ToString(), downloadedFile, exeFolder,
                             Path.Combine(exeFolder, LinuxExecutable),
                         })
                {
                    startInfo.ArgumentList.Add(argument);
                }

                Process.Start(startInfo);
                break;
            }
            default:
                throw new InvalidOperationException("Automatic update is not available for this installation");
        }
    }

    /// <summary>
    /// Waits for Subtitle Edit to exit, runs the installer elevated (one UAC prompt) and starts the
    /// app again as the normal user - also when the UAC prompt was declined, so the user is never
    /// left without a running Subtitle Edit. /SILENT shows Inno's progress window only.
    /// </summary>
    internal const string WindowsScript = """
        param([int]$ProcessId, [string]$Installer, [string]$App)
        try { Wait-Process -Id $ProcessId -ErrorAction SilentlyContinue } catch { }
        try {
            Start-Process -FilePath $Installer -ArgumentList '/SILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-' -Verb RunAs -Wait
            Remove-Item -LiteralPath $Installer -ErrorAction SilentlyContinue
        } catch { }
        Start-Process -FilePath $App
        """;

    /// <summary>
    /// Waits for Subtitle Edit to exit, extracts the tarball over the install folder and starts the
    /// app again. Ignores SIGHUP so closing the terminal Subtitle Edit was started from does not
    /// stop it halfway.
    /// </summary>
    internal const string LinuxScript = """
        #!/bin/sh
        trap '' HUP
        pid="$1"; archive="$2"; dir="$3"; app="$4"
        while kill -0 "$pid" 2>/dev/null; do sleep 1; done
        if tar -xzf "$archive" -C "$dir"; then rm -f "$archive"; fi
        cd "$dir" || exit 1
        nohup "$app" >/dev/null 2>&1 &
        """;

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // left in the temp folder - the next download overwrites it
        }
    }
}
