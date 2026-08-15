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
    // We hand crispasr explicit paths for both halves rather than relying on its `-m auto`
    // codec auto-discovery, which only finds *-s3gen-f16.gguf — that is also what lets the
    // non-f16 quantizations below be offered at all.
    public const string ModelKeyBase = "Base";
    public const string ModelKeyBaseF16 = "Base F16";
    public const string ModelKeyBaseQ4K = "Base Q4_K";
    public const string ModelKeyTurbo = "Turbo";
    public const string DefaultModelKey = ModelKeyBase;

    /// <summary>Model keys in the order the TTS model combo should list them.</summary>
    public static string[] GetAllModelKeys() =>
        new[] { ModelKeyBase, ModelKeyBaseF16, ModelKeyBaseQ4K, ModelKeyTurbo };

    public const string BaseT3FileName = "chatterbox-t3-q8_0.gguf";
    public const string BaseS3GenFileName = "chatterbox-s3gen-q8_0.gguf";
    public const string BaseF16T3FileName = "chatterbox-t3-f16.gguf";
    public const string BaseF16S3GenFileName = "chatterbox-s3gen-f16.gguf";
    public const string BaseQ4KT3FileName = "chatterbox-t3-q4_k.gguf";
    public const string BaseQ4KS3GenFileName = "chatterbox-s3gen-q4_k.gguf";
    public const string TurboT3FileName = "chatterbox-turbo-t3-q8_0.gguf";
    public const string TurboS3GenFileName = "chatterbox-turbo-s3gen-q8_0.gguf";

    private const string BaseRepoUrl = "https://huggingface.co/cstr/chatterbox-GGUF/resolve/main";
    private const string TurboRepoUrl = "https://huggingface.co/cstr/chatterbox-turbo-GGUF/resolve/main";

    // Back-compat aliases for callers that still want the Base file names without
    // resolving via ResolveModelKey.
    public const string T3ModelFileName = BaseT3FileName;
    public const string S3GenModelFileName = BaseS3GenFileName;

    public static string ResolveModelKey(string? modelKey)
    {
        foreach (var key in GetAllModelKeys())
        {
            if (string.Equals(modelKey, key, StringComparison.OrdinalIgnoreCase))
            {
                return key;
            }
        }

        return ModelKeyBase;
    }

    public static string GetT3FileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyTurbo => TurboT3FileName,
        ModelKeyBaseF16 => BaseF16T3FileName,
        ModelKeyBaseQ4K => BaseQ4KT3FileName,
        _ => BaseT3FileName,
    };

    public static string GetS3GenFileName(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyTurbo => TurboS3GenFileName,
        ModelKeyBaseF16 => BaseF16S3GenFileName,
        ModelKeyBaseQ4K => BaseQ4KS3GenFileName,
        _ => BaseS3GenFileName,
    };

    /// <summary>
    /// Approximate on-disk size of the T3 + S3Gen pair, for the "download these now?" prompt.
    /// Measured from the actual files on cstr/chatterbox-GGUF (2026-08-12).
    /// </summary>
    public static string GetDownloadSizeText(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyTurbo => "~1 GB",
        ModelKeyBaseF16 => "~1.8 GB",
        ModelKeyBaseQ4K => "~640 MB",
        _ => "~1 GB",
    };

    /// <summary>
    /// CrispASR exposes Chatterbox Turbo as a *separate* backend (chatterbox-turbo) rather
    /// than as a -m switch on the chatterbox backend; passing Turbo GGUFs to the plain
    /// chatterbox backend triggers an upstream ggml tensor read out of bounds crash. The
    /// Base quantizations are all the same backend — only the GGUF paths differ.
    /// </summary>
    public static string GetBackendName(string? modelKey) =>
        ResolveModelKey(modelKey) == ModelKeyTurbo ? "chatterbox-turbo" : "chatterbox";

    // cstr/chatterbox-GGUF was rebuilt IN PLACE around 2026-06-18: the same file names went
    // from the English-only v1 weights (T3 text vocab 704) to the multilingual weights
    // (text vocab 2454, with the [de]/[fr]/... language tokens the `language` request field
    // needs). Anyone who downloaded before the rebuild still has the English-only files and a
    // bare File.Exists check would keep them forever — language selection then changes nothing
    // and non-English text comes out as accented gibberish. These are the exact byte sizes of
    // the legacy files, used to force a re-download; the Turbo repo was not part of the rebuild.
    private const long LegacyBaseT3Size = 630_177_120;
    private const long LegacyBaseS3GenSize = 358_278_528;

    /// <summary>
    /// True when <paramref name="path"/> is one of the pre-multilingual (English-only) Base
    /// GGUFs, identified by exact byte size. Such a file works for English synthesis but lacks
    /// the language tokens, so it is treated as not installed to trigger a re-download.
    /// Only the q8_0 Base pair was ever shipped English-only; the f16 / q4_k pairs were
    /// published after the rebuild and their sizes do not collide with these two.
    /// </summary>
    public static bool IsLegacyEnglishOnlyModel(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            var length = new FileInfo(path).Length;
            return length == LegacyBaseT3Size || length == LegacyBaseS3GenSize;
        }
        catch
        {
            return false;
        }
    }

    public ChatterboxTtsCppDownloadService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadModels(string modelsFolder, string modelKey, IProgress<float>? progress, Action<string>? titleProgress, CancellationToken cancellationToken)
    {
        var resolved = ResolveModelKey(modelKey);
        var t3FileName = GetT3FileName(resolved);
        var s3genFileName = GetS3GenFileName(resolved);
        var repoUrl = resolved == ModelKeyTurbo ? TurboRepoUrl : BaseRepoUrl;
        var t3Url = $"{repoUrl}/{t3FileName}";
        var s3genUrl = $"{repoUrl}/{s3genFileName}";

        var t3Path = Path.Combine(modelsFolder, t3FileName);
        var s3genPath = Path.Combine(modelsFolder, s3genFileName);
        var needT3 = !File.Exists(t3Path) || IsLegacyEnglishOnlyModel(t3Path);
        var needS3Gen = !File.Exists(s3genPath) || IsLegacyEnglishOnlyModel(s3genPath);
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
