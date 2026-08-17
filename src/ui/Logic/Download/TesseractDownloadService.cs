using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ITesseractDownloadService
{
    Task DownloadTesseract(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadTesseractModel(string modelUrl, Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class TesseractDownloadService : ITesseractDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/tesseract553/Tesseract553.zip";

    /// <summary>Tesseract version behind <see cref="WindowsUrl"/>; stamped into the install folder.</summary>
    public const string WindowsVersion = "5.5.3";

    private const string VersionFileName = "version.txt";

    // Set when the user says no to an update, so the prompt does not come back on every OCR run.
    private static bool _windowsUpdateDeclined;

    private readonly IZipUnpacker _zipUnpacker;

    public TesseractDownloadService(HttpClient httpClient, IZipUnpacker zipUnpacker)
    {
        _httpClient = httpClient;
        _zipUnpacker = zipUnpacker;
    }

    private static string GetTesseractUrl()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsUrl;
        }

        // macOS/Linux use a package-manager install ("brew install tesseract" /
        // "apt install tesseract-ocr") - see OcrViewModel.CheckAndDownloadTesseract.
        throw new PlatformNotSupportedException();
    }

    public async Task DownloadTesseract(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetTesseractUrl(), stream, progress, cancellationToken);
    }

    public async Task DownloadTesseractModel(string modelUrl, Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, modelUrl, stream, progress, cancellationToken);
    }

    /// <summary>
    /// True when Tesseract is installed but older than <see cref="WindowsVersion"/>. Windows only:
    /// elsewhere the binary comes from brew/apt and is not ours to update.
    /// </summary>
    public static bool IsWindowsBuildOutdated()
    {
        if (!OperatingSystem.IsWindows() || _windowsUpdateDeclined)
        {
            return false;
        }

        if (!File.Exists(Path.Combine(Se.TesseractFolder, "tesseract.exe")))
        {
            return false; // nothing installed - that is a plain download, not an update
        }

        var versionFileName = Path.Combine(Se.TesseractFolder, VersionFileName);
        if (!File.Exists(versionFileName))
        {
            return true; // installed before SE stamped a version, so pre-5.5.3
        }

        try
        {
            var installed = new SemanticVersion(File.ReadAllText(versionFileName));
            return installed.IsLessThan(new SemanticVersion(WindowsVersion));
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"Could not read Tesseract version from \"{versionFileName}\".");
            return false; // an unreadable stamp should not nag the user on every OCR run
        }
    }

    public static void DeclineWindowsUpdate()
    {
        _windowsUpdateDeclined = true;
    }

    /// <summary>Records the version just unpacked. Call only after a successful unpack.</summary>
    public static void WriteVersionFile()
    {
        try
        {
            File.WriteAllText(Path.Combine(Se.TesseractFolder, VersionFileName), WindowsVersion);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"Could not write Tesseract version file in \"{Se.TesseractFolder}\".");
        }
    }

    /// <summary>
    /// Clears the previous binaries so DLLs that 5.5.3 no longer ships (the ICU 75 pair, the
    /// OpenSSL libcrypto, the pango/cairo set) do not linger. Leaves <c>tessdata</c> alone - the
    /// user's downloaded models live there.
    /// </summary>
    public static void RemoveOldWindowsBinaries()
    {
        if (!OperatingSystem.IsWindows())
        {
            // TesseractFolder can be a system bin folder (/usr/bin, brew) on macOS/Linux - never
            // delete anything there. Only the Windows build is downloaded into a folder we own.
            return;
        }

        try
        {
            var folder = Se.TesseractFolder;
            if (!Directory.Exists(folder))
            {
                return;
            }

            // GetFiles, not EnumerateFiles: the list must be materialized before deleting from it.
            foreach (var fileName in Directory.GetFiles(folder, "*.dll", SearchOption.TopDirectoryOnly))
            {
                File.Delete(fileName);
            }

            var exeFileName = Path.Combine(folder, "tesseract.exe");
            if (File.Exists(exeFileName))
            {
                File.Delete(exeFileName);
            }
        }
        catch (Exception exception)
        {
            // Not fatal: the unpack below overwrites everything it ships anyway.
            Se.LogError(exception, "Could not remove old Tesseract binaries.");
        }
    }
}