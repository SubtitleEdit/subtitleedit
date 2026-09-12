using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IFfmpegLibsDownloadService
{
    Task DownloadFfmpegLibs(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
}

/// <summary>
/// Downloads the FFmpeg shared libraries (avcodec/avformat/... DLLs) the ffmpeg video player
/// needs. The static ffmpeg.exe Subtitle Edit already downloads contains no DLLs, so this is a
/// separate package: the LGPL shared build of the release line the bindings are generated for
/// (<see cref="FfmpegLibraries.MajorVersion"/>) from BtbN's FFmpeg-Builds.
/// </summary>
public class FfmpegLibsDownloadService(HttpClient httpClient) : IFfmpegLibsDownloadService
{
    private static readonly string WindowsX64Url =
        $"https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-n{FfmpegLibraries.MajorVersion}-latest-win64-lgpl-shared-{FfmpegLibraries.MajorVersion}.zip";

    public async Task DownloadFfmpegLibs(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), destinationFileName, progress, cancellationToken);
    }

    private static string GetUrl()
    {
        if (OperatingSystem.IsWindows() && RuntimeInformation.ProcessArchitecture == Architecture.X64)
        {
            return WindowsX64Url;
        }

        throw new PlatformNotSupportedException("FFmpeg shared library download is only available for Windows x64; install FFmpeg from your package manager instead.");
    }
}
