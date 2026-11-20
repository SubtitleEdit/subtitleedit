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
    private const string ArchiveUrl = "https://github.com/SubtitleEdit/support-files/releases/download/propainter-0.2.0/propainter-0.2.0-macos-arm64.tar.gz";
    private const string ArchiveSha256 = "7d1711f39c1d8bf77d04de95a9bd8fb9d2610b95cdcd360508af016f0456fd82";
    private const string ArchiveRoot = "propainter-macos-arm64";
    private static readonly HttpClient Client = new();
    private static string InstallFolder => Path.Combine(Se.DataFolder, "ProPainter", "0.2.0");

    public static bool IsSupported =>
        OperatingSystem.IsMacOSVersionAtLeast(15) && RuntimeInformation.OSArchitecture == Architecture.Arm64;
    public static string Executable => Path.Combine(InstallFolder, "propainter");
    public static bool IsInstalled => File.Exists(Executable) && File.Exists(Path.Combine(InstallFolder, "stream.py"));

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
            await DownloadHelper.DownloadFileAsync(Client, ArchiveUrl, archivePath, progress, cancellationToken);

            await using var archive = File.OpenRead(archivePath);
            var hash = Convert.ToHexString(await SHA256.HashDataAsync(archive, cancellationToken));
            if (!hash.Equals(ArchiveSha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("ProPainter download checksum mismatch.");
            }

            archive.Position = 0;
            Directory.CreateDirectory(localInstall);
            await using var gzip = new GZipStream(archive, CompressionMode.Decompress);
            await TarFile.ExtractToDirectoryAsync(gzip, localInstall, overwriteFiles: false, cancellationToken);
            var source = Path.Combine(localInstall, ArchiveRoot);
            if (!File.Exists(Path.Combine(source, "propainter")) || !File.Exists(Path.Combine(source, "stream.py")))
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
