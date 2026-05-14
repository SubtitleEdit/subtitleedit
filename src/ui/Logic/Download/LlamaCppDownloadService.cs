using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ILlamaCppDownloadService
{
    Task DownloadEngine(Stream stream, string variant, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadCudaRuntime(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadModel(string url, string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class LlamaCppDownloadService(HttpClient httpClient) : ILlamaCppDownloadService
{
    private const string Version = "b9145";
    private const string BaseUrl = "https://github.com/ggml-org/llama.cpp/releases/download/" + Version + "/";

    public const string VariantCpu = "cpu";
    public const string VariantVulkan = "vulkan";
    public const string VariantCuda = "cuda";

    public async Task DownloadEngine(Stream stream, string variant, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetEngineUrl(variant), stream, progress, cancellationToken);
    }

    public async Task DownloadCudaRuntime(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, BaseUrl + "cudart-llama-bin-win-cuda-12.4-x64.zip", stream, progress, cancellationToken);
    }

    public async Task DownloadModel(string url, string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, url, destinationFileName, progress, cancellationToken);
    }

    /// <summary>
    /// True when the given variant ships its GPU runtime DLLs in a separate archive (Windows CUDA build).
    /// </summary>
    public static bool VariantNeedsCudaRuntime(string variant)
    {
        return OperatingSystem.IsWindows() && variant == VariantCuda;
    }

    private static string GetEngineUrl(string variant)
    {
        if (OperatingSystem.IsWindows())
        {
            return variant switch
            {
                VariantCuda => BaseUrl + "llama-" + Version + "-bin-win-cuda-12.4-x64.zip",
                VariantVulkan => BaseUrl + "llama-" + Version + "-bin-win-vulkan-x64.zip",
                _ => BaseUrl + "llama-" + Version + "-bin-win-cpu-x64.zip",
            };
        }

        if (OperatingSystem.IsLinux())
        {
            return variant == VariantVulkan
                ? BaseUrl + "llama-" + Version + "-bin-ubuntu-vulkan-x64.tar.gz"
                : BaseUrl + "llama-" + Version + "-bin-ubuntu-x64.tar.gz";
        }

        if (OperatingSystem.IsMacOS())
        {
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                ? BaseUrl + "llama-" + Version + "-bin-macos-arm64.tar.gz"
                : BaseUrl + "llama-" + Version + "-bin-macos-x64.tar.gz";
        }

        throw new PlatformNotSupportedException("llama.cpp download is not supported on this platform.");
    }
}
