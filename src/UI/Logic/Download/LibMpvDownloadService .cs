using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ILibMpvDownloadService
{
    Task DownloadLibMpv(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadLibMpv(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class LibMpvDownloadService : ILibMpvDownloadService
{
    private readonly HttpClient _httpClient;
    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/libmpv-2025-01-25/libmpv2-64.zip";
    private const string MacUrl = "";
    private const string MacUrlArm = "";

    public LibMpvDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static string GetUrl()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsUrl;
        }

        throw new PlatformNotSupportedException("Unsupported platform for libmpv download." + Environment.NewLine +
            RuntimeInformation.OSDescription);

        //if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        //{
        //    switch (RuntimeInformation.ProcessArchitecture)
        //    {
        //        case Architecture.Arm64:
        //            return MacUrlArm; // e.g., for M1, M2, M3, M4 chips
        //        case Architecture.X64:
        //            return MacUrl;
        //        default:
        //            throw new PlatformNotSupportedException("Unsupported macOS architecture.");
        //    }
        //}

        //throw new PlatformNotSupportedException();
    }

    public async Task DownloadLibMpv(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadLibMpv(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }
}