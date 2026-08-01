using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// The advanced engine against Ollama's OpenAI-compatible endpoint
/// (http://localhost:11434/v1/chat/completions - not the native /api/generate the classic
/// Ollama engine uses). Ollama serves many models, so the "model" field is required. It ignores
/// llama.cpp's cache_prompt (it manages its own KV cache) and recent versions accept
/// response_format json_schema; older ones get the json_object/plain fallbacks.
/// </summary>
public class OllamaAdvancedTranslate : AdvancedTranslatorBase
{
    public static string StaticName { get; set; } = "Ollama advanced (local LLM)";
    public override string ToString() => StaticName;
    public override string Name => StaticName;
    public override string Url => "https://ollama.com";

    /// <summary>
    /// Endpoint used when the url in settings is only the service base - see <see cref="AutoTranslateUrl"/>.
    /// </summary>
    public const string DefaultUrl = "http://localhost:11434/v1/chat/completions";

    protected override string GetApiUrl()
    {
        return AutoTranslateUrl.Complete(Se.Settings.AutoTranslate.OllamaAdvancedUrl, DefaultUrl);
    }

    protected override string? GetModel()
    {
        var model = Se.Settings.AutoTranslate.OllamaAdvancedModel;
        return string.IsNullOrWhiteSpace(model) ? null : model.Trim();
    }
}
