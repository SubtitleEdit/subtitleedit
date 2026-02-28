using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ILibVlcDownloadService
{
    Task DownloadLibVlc(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadLibVlc(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class LibVlcDownloadService(HttpClient httpClient) : ILibVlcDownloadService
{
    private const string WindowsUrl = "https://get.videolan.org/vlc/3.0.23/win32/vlc-3.0.23-win32.7z";
    private const string MacX64Url = "https://github.com/SubtitleEdit/support-files/releases/download/vlc3/libvlc-osx64.7z";

    public async Task DownloadLibVlc(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadLibVlc(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    private string GetUrl()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsUrl;
        }

        if (OperatingSystem.IsMacOS())
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                // case Architecture.Arm64:
                //     return MacArmUrl; // e.g., for M1, M2, M3, M4, M5 chips
                case Architecture.X64:
                    return MacX64Url;
            }
        }

        throw new PlatformNotSupportedException("LibVLC download is not supported on this platform");
    }
}