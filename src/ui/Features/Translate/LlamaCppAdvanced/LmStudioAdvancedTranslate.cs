using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// The advanced engine against LM Studio's OpenAI-compatible endpoint
/// (http://localhost:1234/v1/chat/completions). LM Studio can have several models loaded, so the
/// "model" field is sent when set (empty = the loaded model). It ignores llama.cpp's cache_prompt,
/// accepts response_format json_schema, and the client's json_object/plain fallbacks cover the rest.
/// </summary>
public class LmStudioAdvancedTranslate : AdvancedTranslatorBase
{
    public static string StaticName { get; set; } = "LM Studio advanced (local LLM)";
    public override string ToString() => StaticName;
    public override string Name => StaticName;
    public override string Url => "https://lmstudio.ai/";

    /// <summary>
    /// Endpoint used when the url in settings is only the service base - see <see cref="AutoTranslateUrl"/>.
    /// </summary>
    public const string DefaultUrl = "http://localhost:1234/v1/chat/completions";

    protected override string GetApiUrl()
    {
        return AutoTranslateUrl.Complete(Se.Settings.AutoTranslate.LmStudioAdvancedUrl, DefaultUrl);
    }

    protected override string? GetModel()
    {
        var model = Se.Settings.AutoTranslate.LmStudioAdvancedModel;
        return string.IsNullOrWhiteSpace(model) ? null : model.Trim();
    }
}
