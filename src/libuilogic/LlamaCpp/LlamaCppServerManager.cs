using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.LlamaCpp;

/// <summary>
/// A curated llama.cpp model that can be downloaded and served. Optional <paramref name="MmprojFileName"/>
/// / <paramref name="MmprojUrl"/> are set for multimodal (vision) models that need a separate vision
/// projector. <paramref name="ChatTemplate"/> and <paramref name="NoJinja"/> override the llama-server
/// launch flags when the bundled chat template needs replacing (TranslateGemma ships a non-standard
/// Jinja template).
/// </summary>
public sealed record LlamaCppModel(
    string DisplayName,
    string FileName,
    string Size,
    string Url,
    string? MmprojFileName = null,
    string? MmprojUrl = null,
    string? ChatTemplate = null,
    bool NoJinja = false,
    // Translation prompt this model was trained on ({0} = source language English name,
    // {1} = target language English name); null = the user's generic llama.cpp prompt.
    // Needed for Hy-MT2, which answers in Chinese when given the generic prompt.
    string? PromptTemplate = null,
    // Model-recommended sampling; -1 = leave the server default.
    double Temperature = -1,
    double TopP = -1,
    int TopK = -1,
    double RepeatPenalty = -1);

/// <summary>
/// Manages the local <c>llama-server</c> process used by the llama.cpp auto-translate and OCR
/// engines: folder/executable paths, the curated model lists, and the server lifecycle (start,
/// health probe, stop, kill on app exit). Modeled on the CrispASR/Chatterbox server handling.
/// </summary>
public static class LlamaCppServerManager
{
    // Tencent's official Hy-MT2 prompt - the model is trained on exactly this phrasing and
    // expects the TARGET language's English name ({1}); {0} (source) is unused by design.
    // The line-break sentence is our addition to Tencent's official wording: the 7B honors it
    // reliably, the 1.8B only sometimes - lost breaks are restored by SE's auto-break like for
    // the other LLM engines.
    private const string HyMt2PromptTemplate =
        "Translate the following text into {1}. Keep line breaks exactly the same. Note that you should only output the translated result without any additional explanation:";

    public static readonly IReadOnlyList<LlamaCppModel> TranslateModels = new[]
    {
        new LlamaCppModel("TranslateGemma 4B (Q4_K_M)", "translategemma-4b_Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/SandLogicTechnologies/translategemma-4b-it-GGUF/resolve/main/translategemma-4b_Q4_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 4B (Q5_K_M)", "translategemma-4b_Q5_K_M.gguf", "2.8 GB",
            "https://huggingface.co/SandLogicTechnologies/translategemma-4b-it-GGUF/resolve/main/translategemma-4b_Q5_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 4B (Q8_0)", "translategemma-4b-it-q8_0.gguf", "4.1 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-4b-it-Q8_0-GGUF/resolve/main/translategemma-4b-it-q8_0.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 12B (Q4_K_M)", "translategemma-12b-it-q4_k_m.gguf", "7.3 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-12b-it-Q4_K_M-GGUF/resolve/main/translategemma-12b-it-q4_k_m.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("TranslateGemma 12B (Q5_K_M)", "translategemma-12b-it-q5_k_m.gguf", "8.5 GB",
            "https://huggingface.co/NikolayKozloff/translategemma-12b-it-Q5_K_M-GGUF/resolve/main/translategemma-12b-it-q5_k_m.gguf",
            ChatTemplate: "gemma", NoJinja: true),

        // Gemma 4 (Google, 2026) - 140+ languages, the strongest general model here for translation
        // into non-English targets. NOTE: unlike Gemma 2/3 this must use its own embedded Jinja
        // template - Gemma 4 replaced the <start_of_turn> scheme with <|turn>role ... <turn|>, so
        // llama.cpp's built-in "gemma" template does NOT apply and forcing it produces garbage.
        // Its template defaults enable_thinking to false, so output is clean translation.
        new LlamaCppModel("Gemma 4 E4B it (Q4_K_M)", "google_gemma-4-E4B-it-Q4_K_M.gguf", "5.4 GB",
            "https://huggingface.co/bartowski/google_gemma-4-E4B-it-GGUF/resolve/main/google_gemma-4-E4B-it-Q4_K_M.gguf"),
        new LlamaCppModel("Gemma 4 E4B it (Q8_0)", "google_gemma-4-E4B-it-Q8_0.gguf", "8.0 GB",
            "https://huggingface.co/bartowski/google_gemma-4-E4B-it-GGUF/resolve/main/google_gemma-4-E4B-it-Q8_0.gguf"),
        // The 12B repo (and its file names) drop the "google_" prefix the E4B repo uses.
        new LlamaCppModel("Gemma 4 12B it (Q4_K_M)", "gemma-4-12B-it-Q4_K_M.gguf", "7.6 GB",
            "https://huggingface.co/bartowski/gemma-4-12B-it-GGUF/resolve/main/gemma-4-12B-it-Q4_K_M.gguf"),

        // Alternative model family. Qwen 3 is the strongest open model for CJK
        // (Chinese/Japanese/Korean) and competitive elsewhere — useful fallback
        // when Gemma's quirks bite (occasional refusals, formatting drift, etc).
        // --no-jinja + chatml bypasses the embedded Jinja template's
        // enable_thinking logic on the hybrid Qwen3-8B so output is clean
        // translation, not <think>...</think> reasoning blocks.
        new LlamaCppModel("Qwen 3 4B Instruct (Q4_K_M)", "Qwen_Qwen3-4B-Instruct-2507-Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3-4B-Instruct-2507-GGUF/resolve/main/Qwen_Qwen3-4B-Instruct-2507-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3 8B (Q4_K_M)", "Qwen_Qwen3-8B-Q4_K_M.gguf", "4.7 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3-8B-GGUF/resolve/main/Qwen_Qwen3-8B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),

        // Qwen 3.5 - newer Qwen generation. Same chatml + --no-jinja handling as Qwen 3 (bypasses the
        // embedded thinking template so the output is clean translation). Kept to <= 8 GB.
        new LlamaCppModel("Qwen 3.5 4B (Q4_K_M)", "Qwen_Qwen3.5-4B-Q4_K_M.gguf", "2.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 4B (Q8_0)", "Qwen_Qwen3.5-4B-Q8_0.gguf", "4.3 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q8_0.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 9B (Q4_K_M)", "Qwen_Qwen3.5-9B-Q4_K_M.gguf", "5.7 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 9B (Q8_0)", "Qwen_Qwen3.5-9B-Q8_0.gguf", "9.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q8_0.gguf",
            ChatTemplate: "chatml", NoJinja: true),

        // Hy-MT2 (Tencent Hunyuan-MT 2, 2026) - translation-specialized, official GGUFs, Apache-2.0.
        // Excellent for its 33+5 supported languages (CJK, major European/Asian) but has NO Nordic
        // languages (Danish/Swedish/Norwegian/Finnish), no Greek/Romanian/Hungarian - generation in
        // unsupported languages produces garbage, hence the coverage note in the display name.
        // Trained on a fixed prompt (PromptTemplate below, with language NAMES - the generic prompt
        // makes it answer in Chinese) and Tencent-recommended sampling. Embedded chat template works
        // as-is, so no ChatTemplate/NoJinja overrides.
        new LlamaCppModel("Hy-MT2 7B (Q4_K_M) - 33 languages, no Nordic", "Hy-MT2-7B-Q4_K_M.gguf", "4.6 GB",
            "https://huggingface.co/tencent/Hy-MT2-7B-GGUF/resolve/main/Hy-MT2-7B-Q4_K_M.gguf",
            PromptTemplate: HyMt2PromptTemplate, Temperature: 0.7, TopP: 0.6, TopK: 20, RepeatPenalty: 1.05),
        new LlamaCppModel("Hy-MT2 7B (Q8_0) - 33 languages, no Nordic", "HY-MT2-7B-Q8_0.gguf", "8.0 GB",
            "https://huggingface.co/tencent/Hy-MT2-7B-GGUF/resolve/main/HY-MT2-7B-Q8_0.gguf",
            PromptTemplate: HyMt2PromptTemplate, Temperature: 0.7, TopP: 0.6, TopK: 20, RepeatPenalty: 1.05),
        new LlamaCppModel("Hy-MT2 1.8B (Q8_0) - 33 languages, no Nordic", "Hy-MT2-1.8B-Q8_0.gguf", "1.9 GB",
            "https://huggingface.co/tencent/Hy-MT2-1.8B-GGUF/resolve/main/Hy-MT2-1.8B-Q8_0.gguf",
            PromptTemplate: HyMt2PromptTemplate, Temperature: 0.7, TopP: 0.6, TopK: 20, RepeatPenalty: 1.05),

        // Aya Expanse 8B (Cohere) - a dedicated multilingual model (23 languages), a good translation
        // alternative to the Gemma/Qwen families. Uses its own embedded (Cohere) chat template, so we
        // leave ChatTemplate/NoJinja at their defaults instead of forcing gemma/chatml. Kept to <= 8 GB.
        new LlamaCppModel("Aya Expanse 8B (Q4_K_M)", "aya-expanse-8b-Q4_K_M.gguf", "4.7 GB",
            "https://huggingface.co/bartowski/aya-expanse-8b-GGUF/resolve/main/aya-expanse-8b-Q4_K_M.gguf"),
        new LlamaCppModel("Aya Expanse 8B (Q5_K_M)", "aya-expanse-8b-Q5_K_M.gguf", "5.4 GB",
            "https://huggingface.co/bartowski/aya-expanse-8b-GGUF/resolve/main/aya-expanse-8b-Q5_K_M.gguf"),
        new LlamaCppModel("Aya Expanse 8B (Q8_0)", "aya-expanse-8b-Q8_0.gguf", "7.9 GB",
            "https://huggingface.co/bartowski/aya-expanse-8b-GGUF/resolve/main/aya-expanse-8b-Q8_0.gguf"),
    };

    // Models for the AI review tool (proofreading). Translation-tuned models (TranslateGemma,
    // Aya) are deliberately absent - proofreading needs general instruction-following and strict
    // JSON output, where the plain instruct models are much stronger. Kept to <= 10 GB.
    public static readonly IReadOnlyList<LlamaCppModel> ReviewModels = new[]
    {
        new LlamaCppModel("Qwen 3.5 4B (Q4_K_M)", "Qwen_Qwen3.5-4B-Q4_K_M.gguf", "2.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 4B (Q8_0)", "Qwen_Qwen3.5-4B-Q8_0.gguf", "4.3 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-4B-GGUF/resolve/main/Qwen_Qwen3.5-4B-Q8_0.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 9B (Q4_K_M)", "Qwen_Qwen3.5-9B-Q4_K_M.gguf", "5.7 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q4_K_M.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Qwen 3.5 9B (Q8_0)", "Qwen_Qwen3.5-9B-Q8_0.gguf", "9.8 GB",
            "https://huggingface.co/bartowski/Qwen_Qwen3.5-9B-GGUF/resolve/main/Qwen_Qwen3.5-9B-Q8_0.gguf",
            ChatTemplate: "chatml", NoJinja: true),
        new LlamaCppModel("Gemma 3 4B it (Q4_K_M)", "google_gemma-3-4b-it-Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/bartowski/google_gemma-3-4b-it-GGUF/resolve/main/google_gemma-3-4b-it-Q4_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        new LlamaCppModel("Gemma 3 12B it (Q4_K_M)", "google_gemma-3-12b-it-Q4_K_M.gguf", "7.3 GB",
            "https://huggingface.co/bartowski/google_gemma-3-12b-it-GGUF/resolve/main/google_gemma-3-12b-it-Q4_K_M.gguf",
            ChatTemplate: "gemma", NoJinja: true),
        // Gemma 4 uses its own embedded Jinja template - see the note in TranslateModels; the
        // built-in "gemma" template above is the Gemma 2/3 format and must not be forced here.
        new LlamaCppModel("Gemma 4 E4B it (Q4_K_M)", "google_gemma-4-E4B-it-Q4_K_M.gguf", "5.4 GB",
            "https://huggingface.co/bartowski/google_gemma-4-E4B-it-GGUF/resolve/main/google_gemma-4-E4B-it-Q4_K_M.gguf"),
        new LlamaCppModel("Gemma 4 E4B it (Q8_0)", "google_gemma-4-E4B-it-Q8_0.gguf", "8.0 GB",
            "https://huggingface.co/bartowski/google_gemma-4-E4B-it-GGUF/resolve/main/google_gemma-4-E4B-it-Q8_0.gguf"),
        new LlamaCppModel("Gemma 4 12B it (Q4_K_M)", "gemma-4-12B-it-Q4_K_M.gguf", "7.6 GB",
            "https://huggingface.co/bartowski/gemma-4-12B-it-GGUF/resolve/main/gemma-4-12B-it-Q4_K_M.gguf"),

        // Different families for second opinions. Llama 3.1 is the strongest English
        // proofreader of its size; EuroLLM is trained specifically on the European languages
        // (all 24 official EU languages incl. the Nordics); Phi-4 mini is the small/fast option.
        // All use their embedded chat templates.
        new LlamaCppModel("Llama 3.1 8B Instruct (Q4_K_M)", "Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf", "4.9 GB",
            "https://huggingface.co/bartowski/Meta-Llama-3.1-8B-Instruct-GGUF/resolve/main/Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf"),
        new LlamaCppModel("EuroLLM 9B Instruct (Q4_K_M)", "EuroLLM-9B-Instruct-Q4_K_M.gguf", "5.6 GB",
            "https://huggingface.co/bartowski/EuroLLM-9B-Instruct-GGUF/resolve/main/EuroLLM-9B-Instruct-Q4_K_M.gguf"),
        new LlamaCppModel("Phi-4 mini 3.8B (Q4_K_M)", "microsoft_Phi-4-mini-instruct-Q4_K_M.gguf", "2.5 GB",
            "https://huggingface.co/bartowski/microsoft_Phi-4-mini-instruct-GGUF/resolve/main/microsoft_Phi-4-mini-instruct-Q4_K_M.gguf"),
    };

    public static readonly IReadOnlyList<LlamaCppModel> OcrModels = new[]
    {
        new LlamaCppModel("GLM-OCR 0.9B (Q8_0)", "GLM-OCR-Q8_0.gguf", "1.4 GB",
            "https://huggingface.co/ggml-org/GLM-OCR-GGUF/resolve/main/GLM-OCR-Q8_0.gguf",
            MmprojFileName: "mmproj-GLM-OCR-Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/ggml-org/GLM-OCR-GGUF/resolve/main/mmproj-GLM-OCR-Q8_0.gguf"),
        new LlamaCppModel("LightOnOCR 1B (Q8_0)", "LightOnOCR-1B-1025-Q8_0.gguf", "1.2 GB",
            "https://huggingface.co/ggml-org/LightOnOCR-1B-1025-GGUF/resolve/main/LightOnOCR-1B-1025-Q8_0.gguf",
            MmprojFileName: "mmproj-LightOnOCR-1B-1025-Q8_0.gguf",
            MmprojUrl: "https://huggingface.co/ggml-org/LightOnOCR-1B-1025-GGUF/resolve/main/mmproj-LightOnOCR-1B-1025-Q8_0.gguf"),
        // PaddlePaddle's official llama.cpp package - 109 languages (NaViT + ERNIE-4.5).
        new LlamaCppModel("PaddleOCR-VL 1.6", "PaddleOCR-VL-1.6-GGUF.gguf", "1.8 GB",
            "https://huggingface.co/PaddlePaddle/PaddleOCR-VL-1.6-GGUF/resolve/main/PaddleOCR-VL-1.6-GGUF.gguf",
            MmprojFileName: "PaddleOCR-VL-1.6-GGUF-mmproj.gguf",
            MmprojUrl: "https://huggingface.co/PaddlePaddle/PaddleOCR-VL-1.6-GGUF/resolve/main/PaddleOCR-VL-1.6-GGUF-mmproj.gguf"),
    };

    /// <summary>
    /// Root folder holding the llama-server executable and the <c>models</c> subfolder.
    /// Defaults to <c>&lt;data folder&gt;/llama.cpp</c> via <see cref="Configuration.DataDirectory"/>
    /// (which the UI's Se config syncs at startup). Headless hosts (seconv) that resolve the
    /// folder differently set this before first use.
    /// </summary>
    public static string? FolderOverride { get; set; }

    /// <summary>
    /// Optional info-level log sink. The UI wires this to the tools log
    /// (<c>Se.WriteToolsLog</c>); seconv wires it to --verbose console output.
    /// </summary>
    public static Action<string>? LogAction { get; set; }

    /// <summary>
    /// Full path to the llama-server executable when it lives outside
    /// <see cref="GetAndCreateFolder"/> - e.g. seconv falling back to a llama.cpp
    /// install on the system PATH (brew/winget/apt). Models still resolve against
    /// the folder.
    /// </summary>
    public static string? ExecutableOverride { get; set; }

    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromMinutes(5) };
    private static readonly SemaphoreSlim ServerLock = new(1, 1);
    private static Process? _serverProcess;
    private static int _serverPort;
    private static string? _serverModelPath;
    private static bool _processExitHooked;
    private static readonly StringBuilder _serverLog = new();

    public static bool IsServerRunning => _serverProcess is { HasExited: false } && _serverPort != 0;

    public static string? RunningModelPath => IsServerRunning ? _serverModelPath : null;

    public static string ApiUrl => $"http://127.0.0.1:{_serverPort}/v1/chat/completions";

    public static string GetAndCreateFolder()
    {
        var folder = FolderOverride;
        if (string.IsNullOrEmpty(folder))
        {
            var dataFolder = string.IsNullOrEmpty(Configuration.DataDirectory)
                ? AppContext.BaseDirectory
                : Configuration.DataDirectory;
            folder = Path.Combine(dataFolder, "llama.cpp");
        }

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetAndCreateModelsFolder()
    {
        var folder = Path.Combine(GetAndCreateFolder(), "models");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string GetExecutable()
    {
        if (!string.IsNullOrEmpty(ExecutableOverride))
        {
            return ExecutableOverride;
        }

        return Path.Combine(GetAndCreateFolder(), OperatingSystem.IsWindows() ? "llama-server.exe" : "llama-server");
    }

    public static bool IsEngineInstalled()
    {
        return File.Exists(GetExecutable());
    }

    public static string GetModelPath(string fileName)
    {
        return Path.Combine(GetAndCreateModelsFolder(), fileName);
    }

    public static bool IsModelInstalled(string fileName)
    {
        var path = GetModelPath(fileName);
        return File.Exists(path) && new FileInfo(path).Length > 10_000_000;
    }

    /// <summary>
    /// Picks the llama-server chat-template flags for a <c>.gguf</c> we do not curate (a file the user
    /// downloaded themselves, e.g. a TranslateGemma quant or size we do not offer). A curated entry with
    /// the same file name wins; otherwise the family is guessed from the file name, because getting this
    /// wrong is not cosmetic: every Gemma we ship needs <c>gemma</c> + <c>--no-jinja</c> (TranslateGemma's
    /// embedded Jinja template is non-standard) and every Qwen needs <c>chatml</c> + <c>--no-jinja</c> (to
    /// bypass the embedded template's thinking mode, which otherwise emits &lt;think&gt; blocks instead of a
    /// translation). Families with a usable embedded template (Aya, Llama, EuroLLM, Phi) fall through to
    /// the default of no override.
    /// </summary>
    public static (string? ChatTemplate, bool NoJinja) InferChatTemplate(string fileName)
    {
        var curated = TranslateModels.Concat(ReviewModels).Concat(OcrModels)
            .FirstOrDefault(m => m.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        if (curated != null)
        {
            return (curated.ChatTemplate, curated.NoJinja);
        }

        // Gemma 4 dropped the <start_of_turn> scheme for <|turn>role ... <turn|>, so the built-in
        // "gemma" template does not apply - fall through and let its embedded Jinja template win.
        var isGemma4 = fileName.Contains("gemma-4", StringComparison.OrdinalIgnoreCase) ||
                       fileName.Contains("gemma4", StringComparison.OrdinalIgnoreCase);

        // Matches "translategemma-27b-it.Q4_K_M.gguf", "google_gemma-3-27b-it-Q4_K_M.gguf", etc.
        if (!isGemma4 && fileName.Contains("gemma", StringComparison.OrdinalIgnoreCase))
        {
            return ("gemma", true);
        }

        if (fileName.Contains("qwen", StringComparison.OrdinalIgnoreCase))
        {
            return ("chatml", true);
        }

        return (null, false);
    }

    /// <summary>
    /// Returns the curated <see cref="TranslateModels"/> plus any other <c>*.gguf</c> the user has
    /// dropped into the llama.cpp models folder. Custom entries are emitted with an empty <c>Url</c>
    /// (no download needed - already on disk), the file name as <c>DisplayName</c>, a
    /// human-readable file size, and chat-template flags from <see cref="InferChatTemplate"/> so a
    /// self-supplied TranslateGemma/Qwen quant is served with the same flags as the curated ones.
    /// <c>mmproj-*.gguf</c> sidecars are skipped because they're not standalone translation models.
    /// </summary>
    public static IReadOnlyList<LlamaCppModel> GetAllTranslateModels()
    {
        return GetCuratedPlusCustomModels(TranslateModels);
    }

    public static IReadOnlyList<LlamaCppModel> GetAllReviewModels()
    {
        return GetCuratedPlusCustomModels(ReviewModels);
    }

    private static IReadOnlyList<LlamaCppModel> GetCuratedPlusCustomModels(IReadOnlyList<LlamaCppModel> curated)
    {
        var folder = GetAndCreateModelsFolder();
        if (!Directory.Exists(folder))
        {
            return curated;
        }

        // "known" spans all curated lists so e.g. a downloaded review model does not show up
        // as a custom entry in the translate list (and vice versa).
        var knownNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in TranslateModels.Concat(ReviewModels).Concat(OcrModels))
        {
            knownNames.Add(m.FileName);
            if (!string.IsNullOrEmpty(m.MmprojFileName))
            {
                knownNames.Add(m.MmprojFileName);
            }
        }

        var custom = new List<LlamaCppModel>();
        try
        {
            foreach (var path in Directory.EnumerateFiles(folder, "*.gguf", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(path);
                if (knownNames.Contains(name))
                {
                    continue;
                }
                if (name.StartsWith("mmproj-", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var size = FormatFileSize(new FileInfo(path).Length);
                var (chatTemplate, noJinja) = InferChatTemplate(name);
                custom.Add(new LlamaCppModel(name, name, size, Url: string.Empty,
                    ChatTemplate: chatTemplate, NoJinja: noJinja));
            }
        }
        catch
        {
            // ignore - if the folder can't be scanned (locked / IO error) just fall back to curated only.
            return curated;
        }

        if (custom.Count == 0)
        {
            return curated;
        }

        custom.Sort((a, b) => string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase));
        return curated.Concat(custom).ToList();
    }

    private static string FormatFileSize(long bytes)
    {
        const double gb = 1024d * 1024d * 1024d;
        const double mb = 1024d * 1024d;
        if (bytes >= gb)
        {
            return (bytes / gb).ToString("0.#", CultureInfo.InvariantCulture) + " GB";
        }
        if (bytes >= mb)
        {
            return (bytes / mb).ToString("0", CultureInfo.InvariantCulture) + " MB";
        }
        return (bytes / 1024d).ToString("0", CultureInfo.InvariantCulture) + " KB";
    }

    public static bool IsModelInstalled(LlamaCppModel model)
    {
        if (!IsModelInstalled(model.FileName))
        {
            return false;
        }

        if (model.MmprojFileName == null)
        {
            return true;
        }

        return IsModelInstalled(model.MmprojFileName);
    }

    /// <summary>
    /// Starts (or reuses) a llama-server for the given model and points
    /// <see cref="Core.Settings.ToolsSettings.LlamaCppApiUrl"/> at it. Throws on failure.
    /// </summary>
    public static async Task EnsureServerRunningAsync(LlamaCppModel model, CancellationToken cancellationToken)
    {
        var modelPath = GetModelPath(model.FileName);
        if (IsServerRunning && _serverModelPath == modelPath)
        {
            Configuration.Settings.Tools.LlamaCppApiUrl = ApiUrl;
            return;
        }

        await ServerLock.WaitAsync(cancellationToken);
        try
        {
            if (IsServerRunning && _serverModelPath == modelPath)
            {
                Configuration.Settings.Tools.LlamaCppApiUrl = ApiUrl;
                return;
            }

            // Server not running, or running with a different model - (re)start.
            if (_serverProcess != null)
            {
                StopServerInternal();
            }

            var exe = GetExecutable();
            if (!File.Exists(exe))
            {
                throw new FileNotFoundException("llama-server executable not found - please download llama.cpp first.", exe);
            }

            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("llama.cpp model not found - please download a model first.", modelPath);
            }

            string? mmprojPath = null;
            if (model.MmprojFileName != null)
            {
                mmprojPath = GetModelPath(model.MmprojFileName);
                if (!File.Exists(mmprojPath))
                {
                    throw new FileNotFoundException("llama.cpp vision projector not found - please download the model first.", mmprojPath);
                }
            }

            var port = FindFreeLoopbackPort();
            var psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(exe) ?? GetAndCreateFolder(),
                FileName = exe,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };
            psi.ArgumentList.Add("-m");
            psi.ArgumentList.Add(modelPath);
            if (mmprojPath != null)
            {
                psi.ArgumentList.Add("--mmproj");
                psi.ArgumentList.Add(mmprojPath);
            }
            psi.ArgumentList.Add("--host");
            psi.ArgumentList.Add("127.0.0.1");
            psi.ArgumentList.Add("--port");
            psi.ArgumentList.Add(port.ToString(CultureInfo.InvariantCulture));
            // Offload all layers to the GPU when a GPU build is in use; ignored by the CPU build.
            psi.ArgumentList.Add("-ngl");
            psi.ArgumentList.Add("99");
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("8192");
            if (model.NoJinja)
            {
                psi.ArgumentList.Add("--no-jinja");
            }
            if (model.ChatTemplate != null)
            {
                psi.ArgumentList.Add("--chat-template");
                psi.ArgumentList.Add(model.ChatTemplate);
            }

            var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start llama-server");

            LogAction?.Invoke($"llama-server starting - PID: {process.Id}, Cmd: {FormatLaunchCommand(exe, psi.ArgumentList)}");

            lock (_serverLog)
            {
                _serverLog.Clear();
            }

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    lock (_serverLog) _serverLog.AppendLine(e.Data);
                }
            };
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    lock (_serverLog) _serverLog.AppendLine(e.Data);
                }
            };
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();

            _serverProcess = process;
            _serverPort = port;
            _serverModelPath = modelPath;
            HookProcessExitOnce();

            var deadline = DateTime.UtcNow.AddMinutes(5);
            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (process.HasExited)
                {
                    var tail = SnapshotServerLog();
                    _serverProcess = null;
                    _serverPort = 0;
                    _serverModelPath = null;
                    throw new InvalidOperationException(
                        $"llama-server exited during startup (code {process.ExitCode}). Output: {tail}");
                }

                if (await ProbeHealthAsync(port, TimeSpan.FromSeconds(2), cancellationToken))
                {
                    Configuration.Settings.Tools.LlamaCppApiUrl = ApiUrl;
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }

            var lastOutput = SnapshotServerLog();
            StopServerInternal();
            throw new TimeoutException(
                $"llama-server did not report healthy within 5 minutes. Last output: {lastOutput}");
        }
        finally
        {
            ServerLock.Release();
        }
    }

    public static void StopServer()
    {
        ServerLock.Wait();
        try
        {
            StopServerInternal();
        }
        finally
        {
            ServerLock.Release();
        }
    }

    private static void StopServerInternal()
    {
        var p = _serverProcess;
        _serverProcess = null;
        _serverPort = 0;
        _serverModelPath = null;
        if (p == null)
        {
            return;
        }

        try
        {
            if (!p.HasExited)
            {
                p.Kill(entireProcessTree: true);
                p.WaitForExit(2000);
            }
        }
        catch
        {
            // best effort
        }
        finally
        {
            p.Dispose();
        }
    }

    public static string SnapshotServerLog()
    {
        lock (_serverLog)
        {
            var s = _serverLog.ToString().TrimEnd();
            return s.Length > 2000 ? s[^2000..] : s;
        }
    }

    private static async Task<bool> ProbeHealthAsync(int port, TimeSpan timeout, CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            using var resp = await HttpClient.GetAsync($"http://127.0.0.1:{port}/health", cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static int FindFreeLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private static void HookProcessExitOnce()
    {
        if (_processExitHooked)
        {
            return;
        }

        _processExitHooked = true;
        AppDomain.CurrentDomain.ProcessExit += (_, _) => StopServerInternal();
    }

    private static string FormatLaunchCommand(string exe, System.Collections.ObjectModel.Collection<string> args)
    {
        static string Quote(string s) =>
            !string.IsNullOrEmpty(s) && s.IndexOfAny(new[] { ' ', '\t' }) >= 0
                ? "\"" + s.Replace("\"", "\\\"") + "\""
                : s;

        var sb = new StringBuilder(Quote(exe));
        foreach (var a in args)
        {
            sb.Append(' ').Append(Quote(a));
        }

        return sb.ToString();
    }
}
