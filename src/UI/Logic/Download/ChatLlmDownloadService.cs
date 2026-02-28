using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IChatLlmDownloadService
{
    Task DownloadModelQwen3AsrSmall(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadModelQwen3AsrLarge(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class ChatLlmDownloadService : IChatLlmDownloadService
{
    private readonly HttpClient _httpClient;

    private const string ModelsQwen3AsrSmallUrl = "https://modelscope.cn/models/judd2024/chatllm_quantized_qwen3/resolve/master/qwen3-asr-0.6b.bin";
    private const string ModelsQwen3AsrLargeUrl = "https://modelscope.cn/models/judd2024/chatllm_quantized_qwen3/resolve/master/qwen3-asr-1.7b.bin";

    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/chat-llm-2026-2/chatllm-2025-02-04.zip";
    private const string MacArmUrl = "https://github.com/SubtitleEdit/support-files/releases/download/chat-llm-2026-2/chatllm-cpp-mac-arm.zip";
    //private const string MacX64Url = "https://github.com/SubtitleEdit/support-files/releases/download/whispercpp-183/WhisperCppMacX64.zip";
    private const string LinuxUrl = "https://github.com/SubtitleEdit/support-files/releases/download/chat-llm-2026-2/chatllm-cpp-linux64.zip";

    public ChatLlmDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModelQwen3AsrSmall(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, ModelsQwen3AsrSmallUrl, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadModelQwen3AsrLarge(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, ModelsQwen3AsrLargeUrl, destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadEngine(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(_httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    private static string GetUrl()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return LinuxUrl;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm64:
                    return MacArmUrl; // e.g., for M1, M2, M3, M4, M5 chips
                // case Architecture.X64:
                //     return MacX64Url;
                default:
                    throw new PlatformNotSupportedException("Unsupported macOS architecture.");
            }
        }
        throw new PlatformNotSupportedException();
    }
}