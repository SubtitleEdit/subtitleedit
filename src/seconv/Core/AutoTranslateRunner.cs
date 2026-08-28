using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

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

    /// <summary>
    /// Engines that build their request from an editable prompt, i.e. the ones
    /// <c>--translate-prompt</c> can steer. The rest (LibreTranslate, NLLB) are translation
    /// services with no prompt at all.
    /// </summary>
    public static readonly string[] PromptEngines = { "llamacpp", "ollama", "lmstudio" };

    /// <summary>File extensions that make <c>--translate-prompt</c> a file path rather than inline text.</summary>
    private static readonly string[] PromptFileExtensions = { ".txt", ".prompt", ".md" };

    /// <summary>Refuse to read a whole video/model file as a prompt when a path points at one.</summary>
    private const long MaxPromptFileBytes = 256 * 1024;

    private readonly ConversionOptions _options;
    private readonly IAutoTranslator _translator;
    private readonly LlamaCppModel? _llamaCppModel; // non-null = local server mode; start before first use
    private readonly string _engine;
    private readonly string? _prompt; // --translate-prompt, already read from file / unescaped

    /// <summary>The resolved local llama.cpp model, exposed for tests.</summary>
    internal LlamaCppModel? LlamaCppModel => _llamaCppModel;

    private string? _targetLanguageCode;

    /// <summary>
    /// The target language code as the engine knows it ("de", "zh-CN") - --translate-to
    /// also accepts English names ("German"), and the output file name needs the code.
    /// Null when the requested language is unknown; TranslateAsync reports that error.
    /// </summary>
    public string? TargetLanguageCode
    {
        get
        {
            if (_targetLanguageCode == null)
            {
                try
                {
                    _targetLanguageCode = ResolveLanguage(_translator.GetSupportedTargetLanguages(), _options.TranslateTo!, "target").Code;
                }
                catch (InvalidOperationException)
                {
                    _targetLanguageCode = string.Empty;
                }
            }

            return _targetLanguageCode.Length == 0 ? null : _targetLanguageCode;
        }
    }

    private AutoTranslateRunner(ConversionOptions options, IAutoTranslator translator, LlamaCppModel? llamaCppModel, string engine, string? prompt)
    {
        _options = options;
        _translator = translator;
        _llamaCppModel = llamaCppModel;
        _engine = engine;
        _prompt = prompt;
    }

    /// <summary>
    /// Validates the engine choice, applies URL/model options to libse's Configuration, and
    /// (for local llama.cpp) resolves the server binary + model up front so a broken setup
    /// fails before any file is converted. Throws <see cref="InvalidOperationException"/>
    /// with an actionable message.
    /// </summary>
    public static AutoTranslateRunner Create(ConversionOptions options)
    {
        var engine = NormalizeEngine(options.TranslateEngine);
        var url = options.TranslateUrl?.Trim();
        var tools = Configuration.Settings.Tools;
        LlamaCppModel? llamaCppModel = null;
        var prompt = ReadPromptOption(options.TranslatePrompt);
        if (prompt != null && !SupportsPrompt(engine))
        {
            throw new InvalidOperationException(
                $"--translate-prompt is not supported by translate engine '{engine}'. Use one of: {string.Join(", ", PromptEngines)}.");
        }

        if (options.Verbose)
        {
            LlamaCppServerManager.LogAction = m => Console.WriteLine("  " + m);
        }

        IAutoTranslator translator;
        switch (engine)
        {
            case "llamacpp":
                translator = new LlamaCppTranslate();
                if (!string.IsNullOrEmpty(url))
                {
                    // User-managed server. LlamaCppTranslate posts to the URL as-is, so accept a
                    // bare host:port or an OpenAI-style ".../v1" base and complete either to the
                    // chat/completions endpoint (without doubling an already-present /v1).
                    tools.LlamaCppApiUrl = CompleteChatCompletionsUrl(url);
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
                if (prompt != null)
                {
                    tools.OllamaPrompt = prompt;
                }
                break;
            case "lmstudio":
                translator = new LmStudioTranslate();
                tools.LmStudioApiUrl = string.IsNullOrEmpty(url) ? "http://localhost:1234/v1/chat/completions" : url;
                if (!string.IsNullOrWhiteSpace(options.TranslateModel))
                {
                    tools.LmStudioModel = options.TranslateModel.Trim();
                }
                if (prompt != null)
                {
                    tools.LmStudioPrompt = prompt;
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

        var runner = new AutoTranslateRunner(options, translator, llamaCppModel, engine, prompt);
        runner.ApplyPromptOverride();
        return runner;
    }

    /// <summary>Canonical engine id: empty means the default (llamacpp), and llama.cpp/llama are aliases for it.</summary>
    internal static string NormalizeEngine(string? engine)
    {
        var name = string.IsNullOrWhiteSpace(engine) ? "llamacpp" : engine.Trim().ToLowerInvariant();
        return name is "llama.cpp" or "llama" ? "llamacpp" : name;
    }

    /// <summary>True when the engine builds its request from an editable prompt (see <see cref="PromptEngines"/>).</summary>
    public static bool SupportsPrompt(string? engine)
    {
        return PromptEngines.Contains(NormalizeEngine(engine));
    }

    /// <summary>
    /// Resolves a prompt option's value to the prompt text, or null when the option was not
    /// given. A value ending in <c>.txt</c>/<c>.prompt</c>/<c>.md</c>, or naming a file that
    /// exists, is read from disk - completion templates are multi-line and a shell cannot always
    /// pass those as one argument. Inline text gets <c>\n</c>, <c>\r</c>, <c>\t</c> and
    /// <c>\\</c> unescaped for the same reason.
    /// <para>
    /// Shared by <c>--translate-prompt</c> and <c>--ocr-prompt</c>; <paramref name="optionName"/>
    /// and <paramref name="placeholders"/> only shape the error messages and the "is this a path
    /// or a sentence?" hint, so both options behave identically.
    /// </para>
    /// </summary>
    internal static string? ReadPromptOption(string? value, string optionName = "--translate-prompt", string placeholders = "{0}/{1}/{2}")
    {
        if (value == null)
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            throw new InvalidOperationException($"{optionName} is empty. Pass the prompt text or the path to a text file.");
        }

        var exists = FileExistsSafe(trimmed);
        if (exists || LooksLikePromptFile(trimmed))
        {
            if (!exists)
            {
                throw new InvalidOperationException(
                    $"Prompt file not found: {trimmed}. " +
                    $"A {optionName} value with no spaces, or ending in .txt/.prompt/.md, is read as a file path; " +
                    $"prompt text passed inline has to contain a space or a {placeholders} placeholder.");
            }

            var size = new FileInfo(trimmed).Length;
            if (size > MaxPromptFileBytes)
            {
                throw new InvalidOperationException(
                    $"Prompt file is too large ({size / 1024} KB, max {MaxPromptFileBytes / 1024} KB): {trimmed}. " +
                    $"{optionName} takes the prompt itself, not a data file.");
            }

            var fromFile = File.ReadAllText(trimmed).Trim();
            if (fromFile.Length == 0)
            {
                throw new InvalidOperationException($"Prompt file is empty: {trimmed}");
            }

            return fromFile;
        }

        return Unescape(value.Trim('\r', '\n'));
    }

    /// <summary>
    /// Whether a value that is not an existing file was still meant as one - a typo'd path must
    /// fail loudly instead of being handed to the model as the prompt, which silently translates
    /// a whole batch under "prompts/mine.tmpl". A real prompt is a sentence: it contains a space,
    /// a line break, or at least a <c>{0}</c>/<c>{1}</c>/<c>{2}</c> placeholder. Anything else -
    /// or anything with a prompt-file extension - is treated as a path.
    /// </summary>
    private static bool LooksLikePromptFile(string value)
    {
        if (value.Any(char.IsWhiteSpace))
        {
            // Spaces or line breaks: only a prompt-file extension still makes it a path
            // ("my prompts/milmmt.txt"), never a sentence.
            return !value.Contains('\n') && !value.Contains('\r') && !value.Contains('{') &&
                   PromptFileExtensions.Any(e => value.EndsWith(e, StringComparison.OrdinalIgnoreCase));
        }

        return !value.Contains('{');
    }

    private static bool FileExistsSafe(string path)
    {
        try
        {
            return File.Exists(path);
        }
        catch (Exception)
        {
            // A prompt sentence is not a path - too long, invalid characters, ...
            return false;
        }
    }

    private static string Unescape(string text)
    {
        if (!text.Contains('\\'))
        {
            return text;
        }

        var sb = new System.Text.StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '\\' || i == text.Length - 1)
            {
                sb.Append(text[i]);
                continue;
            }

            i++;
            switch (text[i])
            {
                case 'n': sb.Append('\n'); break;
                case 'r': sb.Append('\r'); break;
                case 't': sb.Append('\t'); break;
                case '\\': sb.Append('\\'); break;
                default: sb.Append('\\').Append(text[i]); break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Writes <c>--translate-prompt</c> into the settings field the selected engine reads.
    /// For llama.cpp that means <em>both</em> prompt fields: the engine prefers the per-model
    /// template (<c>Tools.LlamaCppModelPrompt</c>, set from the curated model by
    /// <see cref="LlamaCppServerManager.ApplyTranslatePromptSettings"/> before every file), so
    /// setting only the plain prompt would be silently ignored for MiLMMT/Hy-MT2 and friends.
    /// Called again after each ApplyTranslatePromptSettings for that reason.
    /// </summary>
    /// <summary>
    /// Prompt/sampling settings for the upcoming file. The engine reads the per-model
    /// prompt/sampling (e.g. Hy-MT2's or MiLMMT-46's trained-in prompt) from settings, which
    /// nothing in a console run sets otherwise - and an explicit <c>--translate-prompt</c>
    /// overrides it again. Internal so the precedence can be tested without a llama-server.
    /// </summary>
    internal void ApplyPromptSettings()
    {
        if (_llamaCppModel != null)
        {
            LlamaCppServerManager.ApplyTranslatePromptSettings(_llamaCppModel);
        }

        ApplyPromptOverride();
    }

    private void ApplyPromptOverride()
    {
        if (_prompt == null || _engine != "llamacpp")
        {
            return; // ollama/lmstudio are set once in Create; nothing overwrites them later
        }

        Configuration.Settings.Tools.LlamaCppPrompt = _prompt;
        Configuration.Settings.Tools.LlamaCppModelPrompt = _prompt;
    }

    /// <summary>
    /// Translates all paragraphs in place. Reuses the already-running llama-server across
    /// files in the same run (the server manager is a no-op when the model matches).
    /// </summary>
    public async Task TranslateAsync(Subtitle subtitle, CancellationToken cancellationToken)
    {
        ApplyPromptSettings();

        if (_llamaCppModel != null)
        {
            if (!LlamaCppServerManager.IsServerRunning)
            {
                if (!_options.Quiet)
                {
                    Console.WriteLine($"  Starting llama-server with model {Path.GetFileName(_llamaCppModel.FileName)} (stops at exit)...");
                }

                await LlamaCppServerManager.EnsureServerRunningAsync(_llamaCppModel, cancellationToken);
            }
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

    /// <summary>
    /// Completes a user-supplied --translate-url to the full chat/completions endpoint.
    /// Accepts a bare <c>host:port</c>, an OpenAI-style <c>.../v1</c> base (with or without
    /// trailing slash), or an already-complete <c>.../chat/completions</c> URL.
    /// </summary>
    internal static string CompleteChatCompletionsUrl(string url)
    {
        return AutoTranslateUrl.Complete(url, LlamaCppTranslate.DefaultUrl);
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
                return LlamaCppServerManager.CreateCustomModel(fileName, Path.GetFullPath(name), string.Empty);
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
