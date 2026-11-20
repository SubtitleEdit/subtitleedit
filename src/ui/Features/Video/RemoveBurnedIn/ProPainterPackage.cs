using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Diagnostics;
using System.Formats.Tar;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

internal static class ProPainterPackage
{
    private const string ArchiveBase = "https://github.com/SubtitleEdit/support-files/releases/download/propainter-0.2.0/";
    private const string MacArchive = "propainter-0.2.0-macos-arm64.tar.gz";
    private const string MacSha256 = "7d1711f39c1d8bf77d04de95a9bd8fb9d2610b95cdcd360508af016f0456fd82";
    private const string WindowsArchive = "propainter-0.2.0-windows-x64-cpu.tar.gz";
    private const string WindowsSha256 = "5118fc69fb290bca679a65a11cba43703e8778474659cc3031ea234692a115a3";
    private const string CudaArchive = "propainter-0.2.0-windows-x64-cuda-addon.tar.gz";
    private const string CudaSha256 = "dd4779230d027b32de6a6542ff7dabab281b27f430266748bd9f81a1550b8f3f";
    private static readonly HttpClient Client = new();

    public static bool CanUseCuda => OperatingSystem.IsWindows() && RuntimeInformation.OSArchitecture == Architecture.X64;
    private static string InstallFolder => Path.Combine(Se.DataFolder, "ProPainter",
        CanUseCuda ? "0.2.0-windows-x64" : "0.2.0");
    private static string CudaFolder => Path.Combine(InstallFolder, "cuda");
    private static string ExecutableRelativePath => CanUseCuda ? Path.Combine("python", "python.exe") : "propainter";

    public static bool IsSupported => CanUseCuda ||
        OperatingSystem.IsMacOSVersionAtLeast(15) && RuntimeInformation.OSArchitecture == Architecture.Arm64;
    public static string Executable => Path.Combine(InstallFolder, ExecutableRelativePath);
    public static int DownloadSizeMegabytes => CanUseCuda ? 400 : 350;
    public static string GetScript(bool cuda) => CanUseCuda
        ? cuda ? Path.Combine(CudaFolder, "stream-cuda.py") : Path.Combine(InstallFolder, "stream.py")
        : string.Empty;
    public static bool IsInstalled(bool cuda) => HasLauncher(InstallFolder) && (!cuda || HasCudaLauncher(CudaFolder));

    private static bool HasLauncher(string folder) =>
        File.Exists(Path.Combine(folder, ExecutableRelativePath)) && File.Exists(Path.Combine(folder, "stream.py"));

    private static bool HasCudaLauncher(string folder) =>
        File.Exists(Path.Combine(folder, "stream-cuda.py")) && File.Exists(Path.Combine(folder, "installed.txt")) &&
        File.Exists(Path.Combine(folder, "site", "torch", "__init__.py"));

    public static async Task InstallAsync(bool cuda, IProgress<float> progress, CancellationToken cancellationToken)
    {
        if (cuda && !CanUseCuda)
        {
            throw new PlatformNotSupportedException();
        }

        await InstallArchiveAsync(CanUseCuda ? WindowsArchive : MacArchive, CanUseCuda ? WindowsSha256 : MacSha256,
            CanUseCuda ? "propainter-windows-x64" : "propainter-macos-arm64", InstallFolder, HasLauncher,
            cuda ? new ScaledProgress(progress, 0.1f) : progress, cancellationToken);
        if (cuda)
        {
            await InstallArchiveAsync(CudaArchive, CudaSha256, "propainter-windows-x64-cuda", CudaFolder,
                HasCudaLauncher, null, cancellationToken, source => RunCudaInstallerAsync(source, progress, cancellationToken));
        }
    }

    private sealed class ScaledProgress(IProgress<float> progress, float scale) : IProgress<float>
    {
        public void Report(float value) => progress.Report(value * scale);
    }

    private static async Task InstallArchiveAsync(string archiveName, string expectedSha256, string archiveRoot,
        string destination, Func<string, bool> isReady, IProgress<float>? progress, CancellationToken cancellationToken,
        Func<string, Task>? setup = null)
    {
        if (isReady(destination))
        {
            return;
        }

        var parent = Path.GetDirectoryName(destination)!;
        Directory.CreateDirectory(parent);
        var staging = Directory.CreateTempSubdirectory("subtitle-edit-propainter-").FullName;
        var localInstall = Path.Combine(parent, ".install-" + Guid.NewGuid().ToString("N"));
        try
        {
            var archivePath = Path.Combine(staging, "propainter.tar.gz");
            await DownloadHelper.DownloadFileAsync(Client, ArchiveBase + archiveName, archivePath, progress, cancellationToken);
            await using var archive = File.OpenRead(archivePath);
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(archive, cancellationToken));
            if (!hash.Equals(expectedSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("ProPainter download checksum mismatch.");
            }

            archive.Position = 0;
            Directory.CreateDirectory(localInstall);
            await using var gzip = new GZipStream(archive, CompressionMode.Decompress);
            await TarFile.ExtractToDirectoryAsync(gzip, localInstall, overwriteFiles: false, cancellationToken);
            var source = Path.Combine(localInstall, archiveRoot);
            if (setup != null)
            {
                await setup(source);
            }

            if (!isReady(source))
            {
                throw new InvalidDataException("ProPainter archive is missing its launcher.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                Directory.Move(source, destination);
            }
            catch (IOException) when (isReady(destination))
            {
                // Another installation completed while this one was downloading.
            }
        }
        finally
        {
            DeleteTemporaryFolder(staging);
            DeleteTemporaryFolder(localInstall);
        }
    }

    private static async Task RunCudaInstallerAsync(string source, IProgress<float> progress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var start = new ProcessStartInfo(Executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        // pip may leave large temporary files when its process is killed.
        var pipTemp = Path.Combine(Path.GetDirectoryName(source)!, "pip-temp");
        Directory.CreateDirectory(pipTemp);
        start.Environment["TEMP"] = pipTemp;
        start.Environment["TMP"] = pipTemp;
        start.ArgumentList.Add("-I");
        start.ArgumentList.Add("-B");
        start.ArgumentList.Add("-u");
        start.ArgumentList.Add(Path.Combine(source, "install-cuda.py"));

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start the ProPainter CUDA installer.");
        using var cancel = cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                Se.LogError("Could not stop the ProPainter CUDA installer: " + exception.Message);
            }
        });
        var stderr = process.StandardError.ReadToEndAsync();
        while (await process.StandardOutput.ReadLineAsync() is { } line)
        {
            if (GetCudaDownloadProgress(line) is { } fraction)
            {
                progress.Report(0.1f + fraction * 0.88f);
            }
        }

        await process.WaitForExitAsync();
        cancellationToken.ThrowIfCancellationRequested();
        var errors = await stderr;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("ProPainter CUDA setup failed. " + errors.Trim());
        }

        progress.Report(1);
    }

    internal static float? GetCudaDownloadProgress(string line)
    {
        // pip's raw progress uses bytes. Ignore the small torchvision wheel here.
        var parts = line.Split(' ');
        return parts.Length == 4 && parts[0] == "Progress" && parts[2] == "of" &&
               long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var received) &&
               long.TryParse(parts[3], NumberStyles.None, CultureInfo.InvariantCulture, out var total) &&
               total >= 1_000_000_000
            ? Math.Clamp((float)received / total, 0, 1)
            : null;
    }

    private static void DeleteTemporaryFolder(string folder)
    {
        try
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Se.LogError("Could not remove temporary ProPainter files: " + folder);
        }
    }
}
