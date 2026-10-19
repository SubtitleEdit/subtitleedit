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
    /// <summary>No in-app update - the user gets the download page (zip, Flatpak, AUR, ...).</summary>
    None,

    /// <summary>Installed by the Inno Setup installer: run the new installer silently.</summary>
    WindowsInstaller,

    /// <summary>Unpacked Linux tarball in a user-writable folder: extract the new tarball over it.</summary>
    LinuxTarball,

    /// <summary>App bundle in a user-writable folder: swap in the bundle from the new dmg.</summary>
    MacAppBundle,
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

    /// <summary>True once the install helper has been started in this session - it is already waiting for exit.</summary>
    bool IsInstallPending { get; }

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
    internal const string MacArm64Asset = "SubtitleEdit-macOS-ARM64.dmg";
    internal const string MacX64Asset = "SubtitleEdit-macOS-x64.dmg";

    private readonly HttpClient _httpClient;
    private readonly Lazy<AutoUpdateKind> _kind;

    // Static: the dialog (and its view model) is created anew each time, but only one helper
    // may wait for this process - a second one would install the update twice.
    private static volatile bool _helperStarted;

    public AutoUpdateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _kind = new Lazy<AutoUpdateKind>(DetectCurrentKind);
    }

    public AutoUpdateKind Kind => _kind.Value;

    public bool IsInstallPending => _helperStarted;

    private static AutoUpdateKind DetectCurrentKind()
    {
        try
        {
            return DetectKind(
                OperatingSystem.IsWindows(),
                OperatingSystem.IsLinux(),
                OperatingSystem.IsMacOS(),
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
        bool isMacOs,
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

        // Swapping the bundle renames it inside its parent folder, so that must be writable -
        // which also rules out running straight from the mounted dmg and App Translocation.
        if (isMacOs &&
            architecture is Architecture.X64 or Architecture.Arm64 &&
            GetMacAppBundle(exeFolder) is { } bundle &&
            !bundle.Contains("/AppTranslocation/", StringComparison.Ordinal) &&
            isDirectoryWritable(Path.GetDirectoryName(bundle)!))
        {
            return AutoUpdateKind.MacAppBundle;
        }

        return AutoUpdateKind.None;
    }

    internal static string GetAssetName(AutoUpdateKind kind, Architecture architecture)
    {
        return kind switch
        {
            AutoUpdateKind.WindowsInstaller => WindowsInstallerAsset,
            AutoUpdateKind.LinuxTarball => architecture == Architecture.Arm64 ? LinuxArm64Asset : LinuxX64Asset,
            AutoUpdateKind.MacAppBundle => architecture == Architecture.Arm64 ? MacArm64Asset : MacX64Asset,
            _ => string.Empty,
        };
    }

    /// <summary>"/Applications/Subtitle Edit.app" for ".../Subtitle Edit.app/Contents/MacOS/", else null.</summary>
    internal static string? GetMacAppBundle(string exeFolder)
    {
        var macOsFolder = exeFolder.TrimEnd('/');
        var contents = Path.GetDirectoryName(macOsFolder);
        var bundle = contents == null ? null : Path.GetDirectoryName(contents);
        return Path.GetFileName(macOsFolder) == "MacOS" &&
               Path.GetFileName(contents) == "Contents" &&
               bundle != null &&
               bundle.EndsWith(".app", StringComparison.OrdinalIgnoreCase)
            ? bundle
            : null;
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

        var folder = CreateStagingFolder();
        var path = Path.Combine(folder, asset.Name);

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

        if (Kind == AutoUpdateKind.MacAppBundle)
        {
            var currentBundle = GetMacAppBundle(AppContext.BaseDirectory)
                                ?? throw new InvalidOperationException("Not running from an app bundle");
            var stagedApp = await PrepareMacAppBundleAsync(path, folder, currentBundle, cancellationToken);
            TryDelete(path);
            return stagedApp;
        }

        return path;
    }

    /// <summary>
    /// A fresh, per-user staging folder. Not a fixed name in the shared temp folder: on Linux
    /// that is /tmp, where another user could pre-create the folder and swap files in before
    /// the helper runs at exit. The per-user data folder is owned by the user; the staging
    /// folder is also created owner-only off Windows.
    /// </summary>
    private static string CreateStagingFolder()
    {
        var parent = OperatingSystem.IsWindows() ? Path.GetTempPath() : Se.DataFolder;
        var folder = Path.Combine(parent, "SubtitleEdit-update");
        if (Directory.Exists(folder))
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch (Exception exception)
            {
                // e.g. a file still locked by a previous attempt - stage in a new folder instead
                Se.LogError(exception, "AutoUpdateService.CreateStagingFolder");
                folder += "-" + Guid.NewGuid().ToString("N");
            }
        }

        if (OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(folder);
        }
        else
        {
            Directory.CreateDirectory(folder, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }

        return folder;
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

    /// <summary>
    /// Copies the app bundle out of the dmg into the staging folder and checks it is signed
    /// intact by the same Apple team as the running app - so the bundle swapped in after exit
    /// is the genuine, notarized build. Our own download carries no quarantine flag, so the
    /// new bundle opens without a Gatekeeper prompt.
    /// </summary>
    internal static async Task<string> PrepareMacAppBundleAsync(string dmgPath, string folder, string currentBundle, CancellationToken cancellationToken)
    {
        var currentTeam = await GetMacTeamIdentifierAsync(currentBundle, cancellationToken);
        if (string.IsNullOrEmpty(currentTeam))
        {
            throw new InvalidOperationException("The running app is not signed - cannot verify the update");
        }

        var mountPoint = Path.Combine(folder, "mount");
        var staging = Path.Combine(folder, "staged");
        if (Directory.Exists(staging))
        {
            Directory.Delete(staging, true);
        }

        Directory.CreateDirectory(mountPoint);
        Directory.CreateDirectory(staging);

        await RunAsync("hdiutil", cancellationToken, "attach", "-nobrowse", "-readonly", "-noautoopen", "-mountpoint", mountPoint, dmgPath);
        string stagedApp;
        try
        {
            var appInDmg = Directory.GetDirectories(mountPoint, "*.app").FirstOrDefault()
                           ?? throw new InvalidDataException("No app bundle in the update dmg");
            stagedApp = Path.Combine(staging, Path.GetFileName(appInDmg));
            await RunAsync("ditto", cancellationToken, appInDmg, stagedApp);
        }
        finally
        {
            await RunAsync("hdiutil", CancellationToken.None, "detach", mountPoint, "-force");
        }

        await RunAsync("codesign", cancellationToken, "--verify", "--deep", "--strict", stagedApp);
        var newTeam = await GetMacTeamIdentifierAsync(stagedApp, cancellationToken);
        if (!string.Equals(newTeam, currentTeam, StringComparison.Ordinal))
        {
            Directory.Delete(staging, true);
            throw new InvalidDataException($"Update is signed by team '{newTeam}', expected '{currentTeam}'");
        }

        return stagedApp;
    }

    private static async Task<string> GetMacTeamIdentifierAsync(string bundle, CancellationToken cancellationToken)
    {
        // codesign -d writes its details to stderr; an unsigned bundle exits non-zero.
        var (exitCode, output) = await RunAsync("codesign", cancellationToken, false, "-dv", "--verbose=2", bundle);
        return exitCode == 0 ? ParseTeamIdentifier(output) : string.Empty;
    }

    internal static string ParseTeamIdentifier(string codesignOutput)
    {
        foreach (var line in codesignOutput.Split('\n'))
        {
            const string prefix = "TeamIdentifier=";
            var trimmed = line.Trim();
            if (trimmed.StartsWith(prefix, StringComparison.Ordinal))
            {
                var team = trimmed.Substring(prefix.Length);
                return team == "not set" ? string.Empty : team;
            }
        }

        return string.Empty;
    }

    private static Task RunAsync(string fileName, CancellationToken cancellationToken, params string[] arguments) =>
        RunAsync(fileName, cancellationToken, true, arguments);

    private static async Task<(int ExitCode, string Output)> RunAsync(string fileName, CancellationToken cancellationToken, bool throwOnError, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start " + fileName);
        var stdout = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var output = await stdout + await stderr;
        if (throwOnError && process.ExitCode != 0)
        {
            throw new InvalidOperationException($"{fileName} failed with exit code {process.ExitCode}: {output.Trim()}");
        }

        return (process.ExitCode, output);
    }

    public void StartInstallAfterExit(string downloadedFile)
    {
        if (_helperStarted)
        {
            return; // a helper already waits for this process to exit
        }

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
                _helperStarted = true;
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
                _helperStarted = true;
                break;
            }
            case AutoUpdateKind.MacAppBundle:
            {
                var script = Path.Combine(Path.GetDirectoryName(scriptFolder)!, "install-update.sh");
                File.WriteAllText(script, MacScript.Replace("\r\n", "\n"));
                var startInfo = new ProcessStartInfo("/bin/sh") { UseShellExecute = false };
                foreach (var argument in new[]
                         {
                             script, processId.ToString(), downloadedFile, GetMacAppBundle(AppContext.BaseDirectory)!,
                         })
                {
                    startInfo.ArgumentList.Add(argument);
                }

                Process.Start(startInfo);
                _helperStarted = true;
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
    /// app again. Ignores SIGHUP and SIGTERM so closing the terminal Subtitle Edit was started
    /// from, or logging out, does not stop the extraction halfway and leave a half-updated install.
    /// </summary>
    internal const string LinuxScript = """
        #!/bin/sh
        trap '' HUP TERM
        pid="$1"; archive="$2"; dir="$3"; app="$4"
        while kill -0 "$pid" 2>/dev/null; do sleep 1; done
        if tar -xzf "$archive" -C "$dir"; then rm -f "$archive"; fi
        cd "$dir" || exit 1
        nohup "$app" >/dev/null 2>&1 &
        """;

    /// <summary>
    /// Waits for Subtitle Edit to exit, then swaps the verified bundle in under the same name:
    /// the old bundle is renamed aside first and restored if the move fails, so there is always
    /// a working app. Opens the app again either way.
    /// </summary>
    internal const string MacScript = """
        #!/bin/sh
        trap '' HUP
        pid="$1"; staged="$2"; target="$3"
        while kill -0 "$pid" 2>/dev/null; do sleep 1; done
        backup="$target.se-update-old"
        rm -rf "$backup"
        if mv "$target" "$backup"; then
            if mv "$staged" "$target"; then
                rm -rf "$backup"
            else
                rm -rf "$target"
                mv "$backup" "$target"
            fi
        fi
        open "$target"
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
