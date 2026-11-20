using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.RemoveBurnedIn;

internal static class ProPainterPackage
{
    private const string ArchiveBase = "https://github.com/SubtitleEdit/support-files/releases/download/";
    private const string MacSha256 = "5a129b65700a0f085d6d959a6aa0b1884e334e62e124c74e0041bcbf0a9514c8";
    private const string WindowsSha256 = "a0d98f69636a1a71791b6c51a77dbb9c2d539a0801ffd37d9946b519f3654748";
    private static readonly HttpClient Client = new();
    private static string PackageVersion => IsWindowsX64 ? "0.3.0" : "0.3.1";
    private static string InstallFolder => Path.Combine(Se.DataFolder, "ProPainter", PackageVersion + "-native");
    private static bool IsWindowsX64 => OperatingSystem.IsWindows() && RuntimeInformation.OSArchitecture == Architecture.X64;
    private static string ArchiveRoot => IsWindowsX64 ? "propainter-native-windows-x64" : "propainter-native-macos-arm64";
    private static string ExecutableName => IsWindowsX64 ? "propainter-native.exe" : "propainter-native";

    public static bool IsSupported => IsWindowsX64
        ? Avx2.IsSupported && Fma.IsSupported && Bmi2.IsSupported
        : OperatingSystem.IsMacOSVersionAtLeast(15) && RuntimeInformation.OSArchitecture == Architecture.Arm64;
    public static string Executable => Path.Combine(InstallFolder, ExecutableName);
    public static bool IsInstalled => HasNativeFiles(InstallFolder);

    private static bool HasNativeFiles(string folder) =>
        File.Exists(Path.Combine(folder, ExecutableName)) &&
        File.Exists(Path.Combine(folder, "models", "raft-things.gguf")) &&
        File.Exists(Path.Combine(folder, "models", "recurrent_flow_completion.gguf")) &&
        File.Exists(Path.Combine(folder, "models", "ProPainter.gguf"));

    public static async Task InstallAsync(IProgress<float> progress, CancellationToken cancellationToken)
    {
        if (!IsSupported)
        {
            throw new PlatformNotSupportedException();
        }

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
            var target = IsWindowsX64 ? "windows-x64" : "macos-arm64";
            await DownloadHelper.DownloadFileAsync(Client, $"{ArchiveBase}propainter-native-{PackageVersion}/propainter-native-{PackageVersion}-{target}.tar.gz",
                archivePath, progress, cancellationToken);

            await using var archive = File.OpenRead(archivePath);
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(archive, cancellationToken));
            var expectedHash = IsWindowsX64 ? WindowsSha256 : MacSha256;
            if (!hash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("ProPainter download checksum mismatch.");
            }

            archive.Position = 0;
            Directory.CreateDirectory(localInstall);
            await using var gzip = new GZipStream(archive, CompressionMode.Decompress);
            await TarFile.ExtractToDirectoryAsync(gzip, localInstall, overwriteFiles: false, cancellationToken);
            var source = Path.Combine(localInstall, ArchiveRoot);
            if (!HasNativeFiles(source))
            {
                throw new InvalidDataException("ProPainter archive is missing its executable or model files.");
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
