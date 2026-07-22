using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Translate;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.Translate;

namespace SeConv.Core;

/// <summary>
/// Headless auto-translate for <c>--translate-to</c>. Wraps libse's <see cref="IAutoTranslator"/>
/// engines plus libuilogic's merge/split translate loop (shared with the UI's batch convert).
///
/// llama.cpp gets special treatment: with no <c>--translate-url</c> the runner finds the
/// llama-server binary (Subtitle Edit's data folder next to seconv, the installed SE data
/// folder, then the system PATH) and an installed <c>.gguf</c> model, starts the server on a
/// free loopback port, and lets <see cref="LlamaCppServerManager"/> kill it at process exit.
/// seconv never downloads engines or models (consistent with its Tesseract/Paddle policy) —
/// missing pieces fail fast with instructions instead.
/// </summary>
internal sealed class AutoTranslateRunner
{
    public static readonly string[] SupportedEngines = { "llamacpp", "ollama", "lmstudio", "libretranslate", "nllb-serve", "nllb-api" };

    private readonly ConversionOptions _options;
    private readonly IAutoTranslator _translator;
    private readonly LlamaCppModel? _llamaCppModel; // non-null = local server mode; start before first use

    /// <summary>The resolved local llama.cpp model, exposed for tests.</summary>
    internal LlamaCppModel? LlamaCppModel => _llamaCppModel;

    private AutoTranslateRunner(ConversionOptions options, IAutoTranslator translator, LlamaCppModel? llamaCppModel)
    {
        _options = options;
        _translator = translator;
        _llamaCppModel = llamaCppModel;
    }

    /// <summary>
    /// Validates the engine choice, applies URL/model options to libse's Configuration, and
    /// (for local llama.cpp) resolves the server binary + model up front so a broken setup
    /// fails before any file is converted. Throws <see cref="InvalidOperationException"/>
    /// with an actionable message.
    /// </summary>
    public static AutoTranslateRunner Create(ConversionOptions options)
    {
        var engine = string.IsNullOrWhiteSpace(options.TranslateEngine) ? "llamacpp" : options.TranslateEngine.Trim().ToLowerInvariant();
        var url = options.TranslateUrl?.Trim();
        var tools = Configuration.Settings.Tools;
        LlamaCppModel? llamaCppModel = null;

        if (options.Verbose)
        {
            LlamaCppServerManager.LogAction = m => Console.WriteLine("  " + m);
        }

        IAutoTranslator translator;
        switch (engine)
        {
            case "llamacpp" or "llama.cpp" or "llama":
                translator = new LlamaCppTranslate();
                if (!string.IsNullOrEmpty(url))
                {
                    // User-managed server. LlamaCppTranslate posts to the URL as-is, so accept a
                    // bare host:port and complete it to the chat/completions endpoint.
                    tools.LlamaCppApiUrl = url.Contains("/v1/", StringComparison.OrdinalIgnoreCase)
                        ? url
                        : url.TrimEnd('/') + "/v1/chat/completions";
                }
                else
                {
                    llamaCppModel = ResolveLocalLlamaCpp(options.TranslateModel);
                }
                break;
            case "ollama":
                translator = new OllamaTranslate();
                if (!string.IsNullOrEmpty(url))
                {
                    tools.OllamaApiUrl = url;
                }
                if (!string.IsNullOrWhiteSpace(options.TranslateModel))
                {
                    tools.OllamaModel = options.TranslateModel.Trim();
                }
                break;
            case "lmstudio":
                translator = new LmStudioTranslate();
                tools.LmStudioApiUrl = string.IsNullOrEmpty(url) ? "http://localhost:1234/v1/chat/completions" : url;
                if (!string.IsNullOrWhiteSpace(options.TranslateModel))
                {
                    tools.LmStudioModel = options.TranslateModel.Trim();
                }
                break;
            case "libretranslate":
                translator = new LibreTranslate();
                if (!string.IsNullOrEmpty(url))
                {
                    tools.AutoTranslateLibreUrl = url;
                }
                break;
            case "nllb-serve":
                translator = new NoLanguageLeftBehindServe();
                if (!string.IsNullOrEmpty(url))
                {
                    tools.AutoTranslateNllbServeUrl = url;
                }
                break;
            case "nllb-api":
                translator = new NoLanguageLeftBehindApi();
                if (!string.IsNullOrEmpty(url))
                {
                    tools.AutoTranslateNllbApiUrl = url;
                }
                break;
            default:
                throw new InvalidOperationException(
                    $"Translate engine '{options.TranslateEngine}' is not supported. Use one of: {string.Join(", ", SupportedEngines)}.");
        }

        return new AutoTranslateRunner(options, translator, llamaCppModel);
    }

    /// <summary>
    /// Translates all paragraphs in place. Reuses the already-running llama-server across
    /// files in the same run (the server manager is a no-op when the model matches).
    /// </summary>
    public async Task TranslateAsync(Subtitle subtitle, CancellationToken cancellationToken)
    {
        if (_llamaCppModel != null && !LlamaCppServerManager.IsServerRunning)
        {
            if (!_options.Quiet)
            {
                Console.WriteLine($"  Starting llama-server with model {Path.GetFileName(_llamaCppModel.FileName)} (stops at exit)...");
            }

            await LlamaCppServerManager.EnsureServerRunningAsync(_llamaCppModel, cancellationToken);
        }

        var sourceCode = _options.TranslateFrom;
        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            sourceCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle);
        }

        var source = ResolveLanguage(_translator.GetSupportedSourceLanguages(), sourceCode!, "source");
        var target = ResolveLanguage(_translator.GetSupportedTargetLanguages(), _options.TranslateTo!, "target");

        if (!_options.Quiet)
        {
            Console.WriteLine($"  Translating {source.Name} -> {target.Name} via {_translator.Name}...");
        }

        var doTranslate = new DoAutoTranslate();
        if (!_options.Quiet)
        {
            doTranslate.Progress = (done, total) => Console.Write($"\r  Translated {done}/{total} lines...");
        }

        var rows = await doTranslate.DoTranslate(subtitle, source, target, _translator, cancellationToken);
        if (!_options.Quiet)
        {
            Console.WriteLine();
        }

        for (var i = 0; i < subtitle.Paragraphs.Count && i < rows.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(rows[i].TranslatedText))
            {
                subtitle.Paragraphs[i].Text = rows[i].TranslatedText;
            }
        }
    }

    internal static TranslationPair ResolveLanguage(List<TranslationPair> languages, string requested, string kind)
    {
        var match = languages.FirstOrDefault(p => p.Code.Equals(requested, StringComparison.OrdinalIgnoreCase))
                    ?? languages.FirstOrDefault(p => p.TwoLetterIsoLanguageName.Equals(requested, StringComparison.OrdinalIgnoreCase))
                    ?? languages.FirstOrDefault(p => p.Name.Equals(requested, StringComparison.OrdinalIgnoreCase));
        if (match == null)
        {
            var known = string.Join(", ", languages.Select(p => p.Code).Take(30));
            throw new InvalidOperationException(
                $"Unknown {kind} language '{requested}' for this translate engine. Use a code or English name; codes include: {known}...");
        }

        return match;
    }

    /// <summary>
    /// Local llama.cpp mode: resolve the llama-server binary and a translate model without
    /// downloading anything. The Subtitle Edit data folders are probed first so an install
    /// done via the SE GUI (Auto-translate &gt; llama.cpp) is picked up automatically.
    /// </summary>
    private static LlamaCppModel ResolveLocalLlamaCpp(string? requestedModel)
    {
        LlamaCppLocal.EnsureServerBinary("Auto-translate > llama.cpp", "--translate-url");
        return ResolveLlamaCppModel(requestedModel);
    }

    private static LlamaCppModel ResolveLlamaCppModel(string? requestedModel)
    {
        if (!string.IsNullOrWhiteSpace(requestedModel))
        {
            var name = requestedModel.Trim();

            // Full path to a .gguf: use it directly, but infer the chat-template flags from the file
            // name (TranslateGemma/Qwen need them, whether or not we curate that exact quant).
            if (Path.IsPathRooted(name) || name.Contains(Path.DirectorySeparatorChar) || name.Contains(Path.AltDirectorySeparatorChar))
            {
                if (!File.Exists(name))
                {
                    throw new InvalidOperationException($"Translate model file not found: {name}");
                }

                var fileName = Path.GetFileName(name);
                var (chatTemplate, noJinja) = LlamaCppServerManager.InferChatTemplate(fileName);
                return new LlamaCppModel(fileName, Path.GetFullPath(name), string.Empty, Url: string.Empty,
                    ChatTemplate: chatTemplate, NoJinja: noJinja);
            }

            // Name: match curated + custom models in the models folder (with or without .gguf).
            var all = LlamaCppServerManager.GetAllTranslateModels();
            var model = all.FirstOrDefault(m => m.FileName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        ?? all.FirstOrDefault(m => m.FileName.Equals(name + ".gguf", StringComparison.OrdinalIgnoreCase))
                        ?? all.FirstOrDefault(m => m.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (model == null || !LlamaCppServerManager.IsModelInstalled(model))
            {
                throw new InvalidOperationException(
                    $"Translate model '{name}' not found in {LlamaCppServerManager.GetAndCreateModelsFolder()}. " +
                    "Download one in Subtitle Edit (Auto-translate > llama.cpp), drop a .gguf into that folder, " +
                    "or pass a full path via --translate-model.");
            }

            return model;
        }

        // No model given: pick the first installed translate model (curated order, then custom).
        var installed = LlamaCppServerManager.GetAllTranslateModels().FirstOrDefault(LlamaCppServerManager.IsModelInstalled);
        if (installed == null)
        {
            throw new InvalidOperationException(
                $"No llama.cpp translate model found in {LlamaCppServerManager.GetAndCreateModelsFolder()}. " +
                "Download one in Subtitle Edit (Auto-translate > llama.cpp), drop a .gguf into that folder, " +
                "or pass --translate-model:<path.gguf>.");
        }

        return installed;
    }
}
