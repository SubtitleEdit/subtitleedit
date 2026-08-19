using Nikse.SubtitleEdit.Logic.Config;
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

    // The Base pair is the versioned V3 artifact: upstream's production multilingual checkpoint
    // (t3_mtl23ls_v3 + the original s3gen), pinned to ResembleAI revision 5bb1f6ee and recorded
    // as such in the GGUF's own general.source.revision. The unversioned chatterbox-t3-*.gguf
    // names it replaces are back-compat aliases that upstream reserves the right to rebuild in
    // place — which is exactly what happened on 2026-06-18 and cost SE a byte-size hack to
    // detect. A versioned file name is the durable fix: a rebuild lands on a new name instead of
    // silently changing what is already on disk.
    public const string BaseT3FileName = "chatterbox-v3-t3-q8_0.gguf";
    public const string BaseS3GenFileName = "chatterbox-v3-s3gen-q8_0.gguf";
    public const string BaseF16T3FileName = "chatterbox-v3-t3-f16.gguf";
    public const string BaseF16S3GenFileName = "chatterbox-v3-s3gen-f16.gguf";
    public const string BaseQ4KT3FileName = "chatterbox-v3-t3-q4_k.gguf";
    public const string BaseQ4KS3GenFileName = "chatterbox-v3-s3gen-q4_k.gguf";
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
    /// Measured from the actual files on cstr/chatterbox-GGUF (2026-08-16). The Q4_K pair grew
    /// with the move to the V3 artifacts - its S3Gen half is 314 MB rather than 255 MB.
    /// </summary>
    public static string GetDownloadSizeText(string? modelKey) => ResolveModelKey(modelKey) switch
    {
        ModelKeyTurbo => "~1 GB",
        ModelKeyBaseF16 => "~1.8 GB",
        ModelKeyBaseQ4K => "~700 MB",
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

    /// <summary>
    /// The unversioned Base GGUFs the <c>chatterbox-v3-*</c> pair replaced. Nothing reads these
    /// any more, so they are pure dead weight on disk — up to ~3.4 GB for a user who had all
    /// three quantizations. Turbo is absent on purpose: it keeps its own unversioned names.
    /// </summary>
    private static readonly string[] SupersededBaseFileNames =
    {
        "chatterbox-t3-q8_0.gguf",
        "chatterbox-s3gen-q8_0.gguf",
        "chatterbox-t3-f16.gguf",
        "chatterbox-s3gen-f16.gguf",
        "chatterbox-t3-q4_k.gguf",
        "chatterbox-s3gen-q4_k.gguf",
    };

    /// <summary>
    /// Deletes the unversioned Base GGUFs once their replacement is in place. Best effort: a file
    /// that cannot be deleted (in use, read-only) just stays, since it costs disk rather than
    /// correctness. Only ever called with the replacement already downloaded, so this cannot
    /// leave the engine without a model.
    /// </summary>
    public static void RemoveSupersededBaseModels(string modelsFolder, Action<string>? log = null)
    {
        foreach (var fileName in SupersededBaseFileNames)
        {
            try
            {
                var path = Path.Combine(modelsFolder, fileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    log?.Invoke($"Chatterbox: removed superseded model '{fileName}'");
                }
            }
            catch (Exception exception)
            {
                log?.Invoke($"Chatterbox: could not remove superseded model '{fileName}': {exception.Message}");
            }
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

        // The V3 pair replaced the unversioned Base GGUFs, so once it is on disk the old ones are
        // unreachable bytes. Only after a successful download of the Base pair - Turbo keeps its
        // own names, and a failed download must not take the user's existing models with it.
        if (resolved != ModelKeyTurbo && File.Exists(t3Path) && File.Exists(s3genPath))
        {
            RemoveSupersededBaseModels(modelsFolder, message => Se.WriteToolsLog(message));
        }
    }
}
