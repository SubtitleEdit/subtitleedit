using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace SeConv.Core;

/// <summary>
/// seconv's instance of the advanced local-LLM translate engines (the GUI's "llama.cpp advanced",
/// "Ollama advanced" and "LM Studio advanced"): libuilogic's batch/context loop with the endpoint
/// and model coming from the command line instead of the GUI settings. The endpoint is read per
/// request because the local llama-server, and so its port, only starts right before the first
/// file is translated.
/// </summary>
internal sealed class AdvancedTranslateEngine : AdvancedTranslatorBase
{
    private readonly Func<string> _apiUrl;
    private readonly string? _model;

    public AdvancedTranslateEngine(string name, string url, Func<string> apiUrl, string? model)
    {
        Name = name;
        Url = url;
        _apiUrl = apiUrl;
        _model = model;
    }

    public override string Name { get; }
    public override string Url { get; }

    /// <summary>The chat-completions endpoint the next request goes to, exposed for tests.</summary>
    internal string ApiUrl => GetApiUrl();

    /// <summary>The "model" request field (null for llama-server, which serves one model), exposed for tests.</summary>
    internal string? Model => _model;

    protected override string GetApiUrl() => _apiUrl();

    protected override string? GetModel() => _model;
}
