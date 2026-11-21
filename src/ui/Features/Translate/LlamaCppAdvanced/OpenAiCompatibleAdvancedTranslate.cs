using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// The advanced engine against any OpenAI-style chat-completions endpoint, for example a hosted
/// provider - seconv's openai-compatible-advanced. The url, model and key are taken like the
/// regular OpenAI Compatible engine's: the same default url, and the model and key sent when set
/// (a one-model server needs no model, a local one no key). The endpoint may be a hosted provider,
/// so the llama.cpp request fields are left out (<see cref="UsesLlamaCppExtensions"/>).
/// </summary>
public class OpenAiCompatibleAdvancedTranslate : AdvancedTranslatorBase
{
    public static string StaticName { get; set; } = "OpenAI Compatible API advanced";
    public override string ToString() => StaticName;
    public override string Name => StaticName;
    public override string Url => "https://platform.openai.com/docs/api-reference/chat";

    public override SeLlamaCppAdvanced Settings
    {
        get => Se.Settings.AutoTranslate.LlamaCppAdvanced;
        set => Se.Settings.AutoTranslate.LlamaCppAdvanced = value;
    }

    protected override bool UsesLlamaCppExtensions => false;

    protected override string GetApiUrl()
    {
        return AutoTranslateUrl.Complete(Se.Settings.AutoTranslate.OpenAiCompatibleAdvancedUrl, OpenAiCompatibleTranslate.DefaultUrl);
    }

    protected override string? GetModel()
    {
        var model = Se.Settings.AutoTranslate.OpenAiCompatibleAdvancedModel;
        return string.IsNullOrWhiteSpace(model) ? null : model.Trim();
    }

    protected override string? GetApiKey()
    {
        return Se.Settings.AutoTranslate.OpenAiCompatibleAdvancedApiKey;
    }
}
