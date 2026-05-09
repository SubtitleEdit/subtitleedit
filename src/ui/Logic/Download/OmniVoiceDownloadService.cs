using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IOmniVoiceDownloadService
{
    Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);

    Task DownloadEngine(Stream stream, string windowsVariant, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadVoices(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class OmniVoiceDownloadService : IOmniVoiceDownloadService
{
    private readonly HttpClient _httpClient;

    public const string ModelBaseFileName = "omnivoice-base-Q8_0.gguf";
    public const string ModelTokenizerFileName = "omnivoice-tokenizer-F32.gguf";

    public const string WindowsVariantCpu = "cpu";
    public const string WindowsVariantVulkan = "vulkan";
    public const string WindowsVariantCuda = "cuda";

    private const string ModelBaseUrl = "https://huggingface.co/Serveurperso/OmniVoice-GGUF/resolve/main/omnivoice-base-Q8_0.gguf";
    private const string ModelTokenizerUrl = "https://huggingface.co/Serveurperso/OmniVoice-GGUF/resolve/main/omnivoice-tokenizer-F32.gguf";

    private const string WindowsCpuUrl = "https://github.com/SubtitleEdit/support-files/releases/download/omnivoice-26-06/omnivoice-win64-cpu.zip";
    private const string WindowsVulkanUrl = "https://github.com/SubtitleEdit/support-files/releases/download/omnivoice-26-06/omnivoice-win64-vulkan.zip";
    private const string WindowsCudaUrl = "https://github.com/SubtitleEdit/support-files/releases/download/omnivoice-26-06/omnivoice-win64-cuda.zip";
    private const string MacOsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/omnivoice-26-06/omnivoice-macos-universal-cpu-metal.zip";
    private const string LinuxUrl = "https://github.com/SubtitleEdit/support-files/releases/download/omnivoice-26-06/omnivoice-linux-x64-cpu.zip";

    private const string VoicesUrl = "https://github.com/SubtitleEdit/support-files/releases/download/omnivoice-26-06/OmniVoices.zip";

    public OmniVoiceDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var basePath = Path.Combine(modelsFolder, ModelBaseFileName);
        var tokenizerPath = Path.Combine(modelsFolder, ModelTokenizerFileName);
        var needBase = !File.Exists(basePath);
        var needTokenizer = !File.Exists(tokenizerPath);
        var total = (needBase ? 1 : 0) + (needTokenizer ? 1 : 0);
        var step = 0;

        if (needBase)
        {
            step++;
            titleProgress?.Invoke($"Downloading OmniVoice TTS models ({step}/{total}): {ModelBaseFileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, ModelBaseUrl, basePath, progress, cancellationToken);
        }
        if (needTokenizer)
        {
            step++;
            titleProgress?.Invoke($"Downloading OmniVoice TTS models ({step}/{total}): {ModelTokenizerFileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, ModelTokenizerUrl, tokenizerPath, progress, cancellationToken);
        }
    }

    public async Task DownloadEngine(Stream stream, string windowsVariant, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(windowsVariant), stream, progress, cancellationToken);
    }

    public async Task DownloadVoices(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, VoicesUrl, stream, progress, cancellationToken);
    }

    private static string GetUrl(string windowsVariant)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return windowsVariant switch
            {
                WindowsVariantCuda => WindowsCudaUrl,
                WindowsVariantVulkan => WindowsVulkanUrl,
                _ => WindowsCpuUrl,
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return MacOsUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return LinuxUrl;
        }

        throw new PlatformNotSupportedException();
    }
}
