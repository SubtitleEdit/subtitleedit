using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ICrispEmbedDownloadService
{
    Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineWindowsCuda(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineWindowsVulkan(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineWindowsCpu(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineLinuxCuda(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadModel(string url, string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class CrispEmbedDownloadService : ICrispEmbedDownloadService
{
    private readonly HttpClient _httpClient;

    private const string WindowsCudaUrl = "https://github.com/CrispStrobe/CrispEmbed/releases/download/v0.15.0/crispembed-windows-x86_64-cuda.zip";
    private const string WindowsVulkanUrl = "https://github.com/CrispStrobe/CrispEmbed/releases/download/v0.15.0/crispembed-windows-x86_64-vulkan.zip";
    private const string WindowsCpuUrl = "https://github.com/CrispStrobe/CrispEmbed/releases/download/v0.15.0/crispembed-windows-x86_64.zip";
    private const string MacUrl = "https://github.com/CrispStrobe/CrispEmbed/releases/download/v0.15.0/crispembed-macos-arm64.tar.gz";
    private const string LinuxUrl = "https://github.com/CrispStrobe/CrispEmbed/releases/download/v0.15.0/crispembed-linux-x86_64.tar.gz";
    private const string LinuxCudaUrl = "https://github.com/CrispStrobe/CrispEmbed/releases/download/v0.15.0/crispembed-linux-x86_64-cuda.tar.gz";
    private const string LinuxArmUrl = "https://github.com/CrispStrobe/CrispEmbed/releases/download/v0.15.0/crispembed-linux-arm64.tar.gz";

    public CrispEmbedDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    public async Task DownloadEngineWindowsCuda(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, WindowsCudaUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadEngineWindowsVulkan(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, WindowsVulkanUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadEngineWindowsCpu(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, WindowsCpuUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadEngineLinuxCuda(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, LinuxCudaUrl, stream, progress, cancellationToken);
    }

    public async Task DownloadModel(string url, string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, url, destinationFileName, progress, cancellationToken);
    }

    private static string GetUrl()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsVulkanUrl;
        }

        if (OperatingSystem.IsLinux())
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? LinuxArmUrl : LinuxUrl;
        }

        if (OperatingSystem.IsMacOS())
        {
            return MacUrl;
        }

        throw new PlatformNotSupportedException();
    }
}
