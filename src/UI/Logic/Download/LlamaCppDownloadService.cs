using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ILlamaCppDownloadService
{
    Task DownloadllamaCpp(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadllamaCpp(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class LlamaCppDownloadService(HttpClient httpClient) : ILlamaCppDownloadService
{
    private const string WindowsUrl = "https://github.com/ggml-org/llama.cpp/releases/download/b7489/llama-b7489-bin-win-cpu-x64.zip";
    private const string MacArmUrl = "https://github.com/ggml-org/llama.cpp/releases/download/b7489/llama-b7489-bin-macos-arm64.tar.gz";
    private const string MacX64Url = "https://github.com/ggml-org/llama.cpp/releases/download/b7489/llama-b7489-bin-macos-x64.tar.gz";
    private const string LinuxUrl = "https://github.com/ggml-org/llama.cpp/releases/download/b7489/llama-b7489-bin-ubuntu-x64.tar.gz";

    public async Task DownloadllamaCpp(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadllamaCpp(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    private string GetUrl()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsUrl;
        }

        if (OperatingSystem.IsLinux())
        {
            return LinuxUrl;
        }

        if (OperatingSystem.IsMacOS())
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm64:
                    return MacArmUrl; // e.g., for M1, M2, M3, M4, M5 chips
                case Architecture.X64:
                    return MacX64Url;
            }
        }

        throw new PlatformNotSupportedException("Llama.cpp download is not support on this platform.");
    }
}