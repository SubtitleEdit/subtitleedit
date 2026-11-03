using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.UiLogic.Translate;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;
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
///
/// The advanced engines (llamacpp-advanced, ollama-advanced, lmstudio-advanced) run libuilogic's
/// batch/context loop - numbered batches with a rolling history of translated lines plus an
/// optional synopsis, glossary and formality - with those settings taken from the command line
/// (<see cref="CreateAdvancedSettings"/>) instead of the GUI's "Advanced..." dialog.
/// </summary>
internal sealed class AutoTranslateRunner
{
    public static readonly string[] SupportedEngines = { "llamacpp", "ollama", "lmstudio", "openai-compatible", "libretranslate", "nllb-serve", "nllb-api", "deepl", "llamacpp-advanced", "ollama-advanced", "lmstudio-advanced" };

    /// <summary>
    /// Engines that build their request from an editable prompt, i.e. the ones
    /// <c>--translate-prompt</c> can steer. The rest (LibreTranslate, NLLB) are translation
    /// services with no prompt at all. For the advanced engines the prompt is the system prompt
    /// of the batch protocol (see <see cref="CreateAdvancedSettings"/>).
    /// </summary>
    public static readonly string[] PromptEngines = { "llamacpp", "ollama", "lmstudio", "openai-compatible", "llamacpp-advanced", "ollama-advanced", "lmstudio-advanced" };

    /// <summary>
    /// Engines that run libuilogic's advanced batch/context loop, i.e. the ones the options in
    /// <see cref="AdvancedOptionNames"/> apply to.
    /// </summary>
    public static readonly string[] AdvancedEngines = { "llamacpp-advanced", "ollama-advanced", "lmstudio-advanced" };

    /// <summary>Accepted <c>--translate-formality</c> values (case-insensitive).</summary>
    public static readonly string[] FormalityValues = { "default", "formal", "informal" };

    /// <summary>The options that only the <see cref="AdvancedEngines"/> understand, for error messages.</summary>
    public const string AdvancedOptionNames = "--translate-batch-size/--translate-history/--translate-synopsis/--translate-glossary/--translate-formality";

    /// <summary>
    /// The Ollama advanced engine talks to Ollama's OpenAI-compatible endpoint, not the native
    /// /api/generate url the regular engine uses (same as the GUI's Ollama advanced engine).
    /// </summary>
    private const string OllamaAdvancedDefaultUrl = "http://localhost:11434/v1/chat/completions";

    /// <summary>Engines that send <c>--translate-api-key</c>; the local/self-hosted rest have no key.</summary>
    public static readonly string[] ApiKeyEngines = { "openai-compatible", "deepl" };

    /// <summary>File extensions that make <c>--translate-prompt</c> a file path rather than inline text.</summary>
    private static readonly string[] PromptFileExtensions = { ".txt", ".prompt", ".md" };

    /// <summary>Refuse to read a whole video/model file as a prompt when a path points at one.</summary>
    private const long MaxPromptFileBytes = 256 * 1024;

    private readonly ConversionOptions _options;
    private readonly IAutoTranslator _translator;
    private readonly LlamaCppModel? _llamaCppModel; // non-null = local server mode; start before first use
    private readonly string _engine;
    private readonly string? _prompt; // --translate-prompt, already read from file / unescaped
    private readonly SeLlamaCppAdvanced? _advancedSettings; // non-null = advanced engine (batch/context loop)

    /// <summary>The resolved local llama.cpp model, exposed for tests.</summary>
    internal LlamaCppModel? LlamaCppModel => _llamaCppModel;

    /// <summary>The engine instance, exposed for tests.</summary>
    internal IAutoTranslator Translator => _translator;

    /// <summary>The batch/context settings of an advanced engine (null for the others), exposed for tests.</summary>
    internal SeLlamaCppAdvanced? AdvancedSettings => _advancedSettings;

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
                    var target = ResolveLanguage(_translator.GetSupportedTargetLanguages(), _options.TranslateTo!, "target");
                    // The LLM engines' shared list keeps the English name as the pair's Code (their
                    // prompts want the name) and the code in TwoLetterIsoLanguageName ("de", "zh-CN").
                    _targetLanguageCode = target.Code == target.Name ? target.TwoLetterIsoLanguageName : target.Code;
                }
                catch (InvalidOperationException)
                {
                    _targetLanguageCode = string.Empty;
                }
            }

            return _targetLanguageCode.Length == 0 ? null : _targetLanguageCode;
        }
    }

    private AutoTranslateRunner(ConversionOptions options, IAutoTranslator translator, LlamaCppModel? llamaCppModel, string engine, string? prompt, SeLlamaCppAdvanced? advancedSettings)
    {
        _options = options;
        _translator = translator;
        _llamaCppModel = llamaCppModel;
        _engine = engine;
        _prompt = prompt;
        _advancedSettings = advancedSettings;
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

        var apiKey = options.TranslateApiKey?.Trim();
        if (!string.IsNullOrEmpty(apiKey) && !ApiKeyEngines.Contains(engine))
        {
            throw new InvalidOperationException(
                $"--translate-api-key is not supported by translate engine '{engine}'. Use one of: {string.Join(", ", ApiKeyEngines)}.");
        }

        var advancedSettings = AdvancedEngines.Contains(engine)
            ? CreateAdvancedSettings(options.TranslateBatchSize, options.TranslateHistory, options.TranslateSynopsis, options.TranslateGlossary, options.TranslateFormality, prompt)
            : null;
        if (advancedSettings == null && HasAdvancedOptions(options))
        {
            throw new InvalidOperationException(
                $"{AdvancedOptionNames} require an advanced translate engine: --translate-engine:{string.Join(" | ", AdvancedEngines)}.");
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
            case "llamacpp-advanced":
                // Read per request: the local server's port is only known once it has started.
                translator = MakeAdvanced("llama.cpp advanced (local LLM)", "https://github.com/ggml-org/llama.cpp",
                    () => AutoTranslateUrl.Complete(Configuration.Settings.Tools.LlamaCppApiUrl, LlamaCppTranslate.DefaultUrl), null);
                if (!string.IsNullOrEmpty(url))
                {
                    tools.LlamaCppApiUrl = CompleteChatCompletionsUrl(url);
                }
                else
                {
                    llamaCppModel = ResolveLocalLlamaCpp(options.TranslateModel, chatModelsOnly: true);
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
            case "ollama-advanced":
                if (string.IsNullOrWhiteSpace(options.TranslateModel))
                {
                    // Ollama serves many models and rejects a request without one - fail before any file is converted.
                    throw new InvalidOperationException("Translate engine 'ollama-advanced' needs a model: pass --translate-model:<name> (e.g. gemma4).");
                }
                var ollamaAdvancedUrl = AutoTranslateUrl.Complete(url, OllamaAdvancedDefaultUrl);
                translator = MakeAdvanced("Ollama advanced (local LLM)", "https://ollama.com", () => ollamaAdvancedUrl, options.TranslateModel.Trim());
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
            case "lmstudio-advanced":
                var lmStudioAdvancedUrl = AutoTranslateUrl.Complete(url, LmStudioTranslate.DefaultUrl);
                translator = MakeAdvanced("LM Studio advanced (local LLM)", "https://lmstudio.ai/", () => lmStudioAdvancedUrl,
                    string.IsNullOrWhiteSpace(options.TranslateModel) ? null : options.TranslateModel.Trim());
                break;
            case "openai-compatible":
                translator = new OpenAiCompatibleTranslate();
                if (!string.IsNullOrEmpty(url))
                {
                    tools.OpenAiCompatibleTranslateUrl = url; // the engine completes a bare host:port or ".../v1" base (AutoTranslateUrl.Complete)
                }
                if (!string.IsNullOrEmpty(apiKey))
                {
                    tools.OpenAiCompatibleTranslateApiKey = apiKey;
                }
                if (!string.IsNullOrWhiteSpace(options.TranslateModel))
                {
                    tools.OpenAiCompatibleTranslateModel = options.TranslateModel.Trim();
                }
                if (prompt != null)
                {
                    tools.OpenAiCompatibleTranslatePrompt = prompt;
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
            case "deepl":
                translator = new DeepLTranslate();
                if (!string.IsNullOrEmpty(url))
                {
                    tools.AutoTranslateDeepLUrl = url;
                }
                if (!string.IsNullOrEmpty(apiKey))
                {
                    tools.AutoTranslateDeepLApiKey = apiKey;
                }
                if (string.IsNullOrWhiteSpace(tools.AutoTranslateDeepLApiKey))
                {
                    // DeepLTranslate.Initialize silently skips setup without a key - fail up front instead.
                    throw new InvalidOperationException("Translate engine 'deepl' needs an API key: pass --translate-api-key:<key>.");
                }
                break;
            default:
                throw new InvalidOperationException(
                    $"Translate engine '{options.TranslateEngine}' is not supported. Use one of: {string.Join(", ", SupportedEngines)}.");
        }

        var runner = new AutoTranslateRunner(options, translator, llamaCppModel, engine, prompt, advancedSettings);
        runner.ApplyPromptOverride();
        return runner;

        AdvancedTranslateEngine MakeAdvanced(string name, string homepage, Func<string> apiUrl, string? model)
        {
            return new AdvancedTranslateEngine(name, homepage, apiUrl, model) { Settings = advancedSettings! };
        }
    }

    /// <summary>
    /// Canonical engine id: empty means the default (llamacpp); llama.cpp/llama and
    /// llama.cpp-advanced/llama-advanced are aliases.
    /// </summary>
    internal static string NormalizeEngine(string? engine)
    {
        var name = string.IsNullOrWhiteSpace(engine) ? "llamacpp" : engine.Trim().ToLowerInvariant();
        return name switch
        {
            "llama.cpp" or "llama" => "llamacpp",
            "llama.cpp-advanced" or "llama-advanced" => "llamacpp-advanced",
            _ => name,
        };
    }

    /// <summary>True when the engine builds its request from an editable prompt (see <see cref="PromptEngines"/>).</summary>
    public static bool SupportsPrompt(string? engine)
    {
        return PromptEngines.Contains(NormalizeEngine(engine));
    }

    /// <summary>True for the engines that run the advanced batch/context loop (see <see cref="AdvancedEngines"/>).</summary>
    public static bool IsAdvancedEngine(string? engine)
    {
        return AdvancedEngines.Contains(NormalizeEngine(engine));
    }

    private static bool HasAdvancedOptions(ConversionOptions options)
    {
        return options.TranslateBatchSize.HasValue ||
               options.TranslateHistory.HasValue ||
               !string.IsNullOrWhiteSpace(options.TranslateSynopsis) ||
               !string.IsNullOrWhiteSpace(options.TranslateGlossary) ||
               !string.IsNullOrWhiteSpace(options.TranslateFormality);
    }

    /// <summary>
    /// The batch/context settings of an advanced engine from the command line: what the GUI's
    /// "Advanced..." dialog edits, with the dialog's defaults (10 lines per batch, 12 history
    /// pairs, no synopsis/glossary, default formality) for everything not given. The synopsis
    /// and glossary are read like <c>--translate-prompt</c> (inline text or a file). Throws
    /// <see cref="InvalidOperationException"/> with an actionable message.
    /// </summary>
    internal static SeLlamaCppAdvanced CreateAdvancedSettings(int? batchSize, int? history, string? synopsis, string? glossary, string? formality, string? prompt)
    {
        var settings = new SeLlamaCppAdvanced();
        if (prompt != null)
        {
            // The protocol sends the lines as JSON in the user message and the prompt is the
            // system prompt, so a completion template with the text at {2} has nowhere to put it.
            if (prompt.Contains("{2}"))
            {
                throw new InvalidOperationException(
                    "--translate-prompt is the system prompt for an advanced engine ({0}=source language, {1}=target language) and cannot contain {2}; " +
                    "use --translate-engine:llamacpp for completion-format models.");
            }

            settings.Prompt = prompt;
        }

        if (batchSize.HasValue)
        {
            if (batchSize < 1 || batchSize > 50)
            {
                throw new InvalidOperationException($"--translate-batch-size must be between 1 and 50 (got {batchSize}).");
            }

            settings.BatchSize = batchSize.Value;
        }

        if (history.HasValue)
        {
            if (history < 0 || history > 50)
            {
                throw new InvalidOperationException($"--translate-history must be between 0 and 50 (got {history}).");
            }

            settings.HistoryPairs = history.Value;
        }

        settings.Synopsis = ReadPromptOption(synopsis, "--translate-synopsis", string.Empty) ?? string.Empty;
        settings.Glossary = ReadPromptOption(glossary, "--translate-glossary", string.Empty) ?? string.Empty;
        settings.Formality = ParseFormality(formality);
        return settings;
    }

    internal static string ParseFormality(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return SeLlamaCppAdvanced.FormalityDefault;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "default" => SeLlamaCppAdvanced.FormalityDefault,
            "formal" => SeLlamaCppAdvanced.FormalityFormal,
            "informal" => SeLlamaCppAdvanced.FormalityInformal,
            _ => throw new InvalidOperationException($"--translate-formality '{value}' is not valid. Use one of: {string.Join(", ", FormalityValues)}."),
        };
    }

    /// <summary>
    /// Resolves a prompt option's value to the prompt text, or null when the option was not
    /// given. A value ending in <c>.txt</c>/<c>.prompt</c>/<c>.md</c>, or naming a file that
    /// exists, is read from disk - completion templates are multi-line and a shell cannot always
    /// pass those as one argument. Inline text gets <c>\n</c>, <c>\r</c>, <c>\t</c> and
    /// <c>\\</c> unescaped for the same reason.
    /// <para>
    /// Shared by <c>--translate-prompt</c>, <c>--ocr-prompt</c> and the advanced engines'
    /// <c>--translate-synopsis</c>/<c>--translate-glossary</c>; <paramref name="optionName"/> and
    /// <paramref name="placeholders"/> (empty for an option without any) only shape the error
    /// messages and the "is this a path or a sentence?" hint, so the options behave identically.
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
                var inlineHint = string.IsNullOrEmpty(placeholders) ? "has to contain a space" : $"has to contain a space or a {placeholders} placeholder";
                throw new InvalidOperationException(
                    $"Prompt file not found: {trimmed}. " +
                    $"A {optionName} value with no spaces, or ending in .txt/.prompt/.md, is read as a file path; " +
                    $"text passed inline {inlineHint}.");
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
            return; // ollama/lmstudio/openai-compatible are set once in Create; nothing overwrites them later
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

                if (_advancedSettings != null)
                {
                    // The advanced engine puts history, synopsis and glossary into every request,
                    // so it gets the (default larger) server context and the extra server
                    // arguments from its settings - the same rule as the GUI.
                    var contextSize = Math.Clamp(_advancedSettings.ContextSize, 2048, 262144);
                    await LlamaCppServerManager.EnsureServerRunningAsync(_llamaCppModel, cancellationToken, contextSize, _advancedSettings.ServerArguments, _advancedSettings.ServerArgumentsOnly);
                }
                else
                {
                    await LlamaCppServerManager.EnsureServerRunningAsync(_llamaCppModel, cancellationToken);
                }
            }
        }

        var sourceCode = _options.TranslateFrom;
        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            sourceCode = LanguageAutoDetect.AutoDetectGoogleLanguage(subtitle, true);
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
            doTranslate.Progress = (done, total) => ProgressLine.Report("Translated", done, total);
        }

        var rows = await doTranslate.DoTranslate(subtitle, source, target, _translator, cancellationToken);
        if (!_options.Quiet)
        {
            ProgressLine.Finish();
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
       
        if (match == null && requested == "zh-Hans")
        {
            match = languages.FirstOrDefault(p => p.Code.Equals("zh-CN", StringComparison.OrdinalIgnoreCase) || p.TwoLetterIsoLanguageName.Equals("zh-CN", StringComparison.OrdinalIgnoreCase));
        }
        else if (match == null && requested == "zh-Hant")
        {
            match = languages.FirstOrDefault(p => p.Code.Equals("zh-TW", StringComparison.OrdinalIgnoreCase) || p.TwoLetterIsoLanguageName.Equals("zh-TW", StringComparison.OrdinalIgnoreCase));
        }
        
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
    /// <paramref name="chatModelsOnly"/> (the advanced engine) refuses completion-only models.
    /// </summary>
    private static LlamaCppModel ResolveLocalLlamaCpp(string? requestedModel, bool chatModelsOnly = false)
    {
        LlamaCppLocal.EnsureServerBinary("Auto-translate > llama.cpp", "--translate-url");
        return ResolveLlamaCppModel(requestedModel, chatModelsOnly);
    }

    private static LlamaCppModel ResolveLlamaCppModel(string? requestedModel, bool chatModelsOnly)
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

            if (chatModelsOnly && model.CompletionOnly)
            {
                // Same rule as the GUI's model list for the advanced engine: a raw-completion model
                // answers the JSON batch protocol with well-formed JSON holding the untranslated
                // source lines, which would be written out as a "successful" translation.
                throw new InvalidOperationException(
                    $"Translate model '{name}' is a completion-only model and cannot follow the advanced engine's batch protocol. " +
                    "Use it with --translate-engine:llamacpp, or pick a chat model for the advanced engine.");
            }

            return model;
        }

        // No model given: pick the first installed translate model (curated order, then custom);
        // the advanced engine skips completion-only models (see above).
        var installed = LlamaCppServerManager.GetAllTranslateModels()
            .FirstOrDefault(m => LlamaCppServerManager.IsModelInstalled(m) && !(chatModelsOnly && m.CompletionOnly));
        if (installed == null)
        {
            throw new InvalidOperationException(
                $"No llama.cpp {(chatModelsOnly ? "chat " : string.Empty)}translate model found in {LlamaCppServerManager.GetAndCreateModelsFolder()}. " +
                "Download one in Subtitle Edit (Auto-translate > llama.cpp), drop a .gguf into that folder, " +
                "or pass --translate-model:<path.gguf>.");
        }

        return installed;
    }
}
