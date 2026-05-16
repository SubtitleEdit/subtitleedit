using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IChatterboxTtsCppDownloadService
{
    Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken);
}

public class ChatterboxTtsCppDownloadService : IChatterboxTtsCppDownloadService
{
    private readonly HttpClient _httpClient;

    // Chatterbox is a two-GGUF runtime: T3 AR talker + S3Gen flow-matching codec.
    // We download the q8_0 variants directly so we can hand explicit paths to crispasr
    // (its `-m auto` codec auto-discovery only finds *-s3gen-f16.gguf, not q8_0).
    public const string ModelKeyBase = "Base";
    public const string ModelKeyTurbo = "Turbo";
    public const string DefaultModelKey = ModelKeyBase;

    public const string BaseT3FileName = "chatterbox-t3-q8_0.gguf";
    public const string BaseS3GenFileName = "chatterbox-s3gen-q8_0.gguf";
    public const string TurboT3FileName = "chatterbox-turbo-t3-q8_0.gguf";
    public const string TurboS3GenFileName = "chatterbox-turbo-s3gen-q8_0.gguf";

    private const string BaseT3Url = "https://huggingface.co/cstr/chatterbox-GGUF/resolve/main/chatterbox-t3-q8_0.gguf";
    private const string BaseS3GenUrl = "https://huggingface.co/cstr/chatterbox-GGUF/resolve/main/chatterbox-s3gen-q8_0.gguf";
    private const string TurboT3Url = "https://huggingface.co/cstr/chatterbox-turbo-GGUF/resolve/main/chatterbox-turbo-t3-q8_0.gguf";
    private const string TurboS3GenUrl = "https://huggingface.co/cstr/chatterbox-turbo-GGUF/resolve/main/chatterbox-turbo-s3gen-q8_0.gguf";

    // Back-compat aliases for callers that still want the Base file names without
    // resolving via ResolveModelKey.
    public const string T3ModelFileName = BaseT3FileName;
    public const string S3GenModelFileName = BaseS3GenFileName;

    public static string ResolveModelKey(string? modelKey) =>
        string.Equals(modelKey, ModelKeyTurbo, StringComparison.OrdinalIgnoreCase) ? ModelKeyTurbo : ModelKeyBase;

    public static string GetT3FileName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyTurbo ? TurboT3FileName : BaseT3FileName;

    public static string GetS3GenFileName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyTurbo ? TurboS3GenFileName : BaseS3GenFileName;

    /// <summary>
    /// CrispASR exposes Chatterbox Turbo as a *separate* backend (chatterbox-turbo) rather
    /// than as a -m switch on the chatterbox backend; passing Turbo GGUFs to the plain
    /// chatterbox backend triggers an upstream ggml tensor read out of bounds crash.
    /// </summary>
    public static string GetBackendName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyTurbo ? "chatterbox-turbo" : "chatterbox";

    public ChatterboxTtsCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var resolved = ResolveModelKey(modelKey);
        var t3FileName = GetT3FileName(resolved);
        var s3genFileName = GetS3GenFileName(resolved);
        var t3Url = resolved == ModelKeyTurbo ? TurboT3Url : BaseT3Url;
        var s3genUrl = resolved == ModelKeyTurbo ? TurboS3GenUrl : BaseS3GenUrl;

        var t3Path = Path.Combine(modelsFolder, t3FileName);
        var s3genPath = Path.Combine(modelsFolder, s3genFileName);
        var needT3 = !File.Exists(t3Path);
        var needS3Gen = !File.Exists(s3genPath);
        var total = (needT3 ? 1 : 0) + (needS3Gen ? 1 : 0);
        var step = 0;

        if (needT3)
        {
            step++;
            titleProgress?.Invoke($"Downloading Chatterbox TTS models ({step}/{total}): {t3FileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, t3Url, t3Path, progress, cancellationToken);
        }
        if (needS3Gen)
        {
            step++;
            titleProgress?.Invoke($"Downloading Chatterbox TTS models ({step}/{total}): {s3genFileName}");
            await DownloadHelper.DownloadFileAsync(_httpClient, s3genUrl, s3genPath, progress, cancellationToken);
        }
    }
}
