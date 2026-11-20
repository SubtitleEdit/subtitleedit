using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Formats.Tar;
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
    private static readonly HttpClient Client = new();

    private static bool IsWindowsX64 => OperatingSystem.IsWindows() && RuntimeInformation.OSArchitecture == Architecture.X64;
    private static string InstallFolder => Path.Combine(Se.DataFolder, "ProPainter",
        IsWindowsX64 ? "0.2.0-windows-x64" : "0.2.0");
    private static string ExecutableRelativePath => IsWindowsX64 ? Path.Combine("python", "python.exe") : "propainter";

    public static bool IsSupported => IsWindowsX64 ||
        OperatingSystem.IsMacOSVersionAtLeast(15) && RuntimeInformation.OSArchitecture == Architecture.Arm64;
    public static string Executable => Path.Combine(InstallFolder, ExecutableRelativePath);
    public static int DownloadSizeMegabytes => IsWindowsX64 ? 400 : 350;
    public static string Script => IsWindowsX64 ? Path.Combine(InstallFolder, "stream.py") : string.Empty;
    public static bool IsInstalled => HasLauncher(InstallFolder);

    private static bool HasLauncher(string folder) =>
        File.Exists(Path.Combine(folder, ExecutableRelativePath)) && File.Exists(Path.Combine(folder, "stream.py"));

    public static async Task InstallAsync(IProgress<float> progress, CancellationToken cancellationToken)
    {
        if (IsInstalled)
        {
            return;
        }

        var parent = Path.GetDirectoryName(InstallFolder)!;
        Directory.CreateDirectory(parent);
        var staging = Directory.CreateTempSubdirectory("subtitle-edit-propainter-").FullName;
        var localInstall = Path.Combine(parent, ".install-" + Guid.NewGuid().ToString("N"));
        try
        {
            var archivePath = Path.Combine(staging, "propainter.tar.gz");
            var archiveName = IsWindowsX64 ? WindowsArchive : MacArchive;
            var expectedSha256 = IsWindowsX64 ? WindowsSha256 : MacSha256;
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
            var source = Path.Combine(localInstall, IsWindowsX64 ? "propainter-windows-x64" : "propainter-macos-arm64");
            if (!HasLauncher(source))
            {
                throw new InvalidDataException("ProPainter archive is missing its launcher.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                Directory.Move(source, InstallFolder);
            }
            catch (IOException) when (IsInstalled)
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

    private static void DeleteTemporaryFolder(string folder)
    {
        try
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
        catch (IOException)
        {
            Se.LogError("Could not remove temporary ProPainter files: " + folder);
        }
        catch (UnauthorizedAccessException)
        {
            Se.LogError("Could not remove temporary ProPainter files: " + folder);
        }
    }
}
