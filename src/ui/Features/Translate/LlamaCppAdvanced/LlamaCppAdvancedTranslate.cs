using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.Translate.LlamaCppAdvanced;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// The advanced engine against SE's own llama-server (or a remote one). No "model" field:
/// llama-server serves the single model it was started with, and sending one would only risk a
/// mismatch. The endpoint is set by LlamaCppServerManager (local) or the URL field (remote)
/// before Initialize is called.
/// </summary>
public class LlamaCppAdvancedTranslate : AdvancedTranslatorBase
{
    public static string StaticName { get; set; } = "llama.cpp advanced (local LLM)";
    public override string ToString() => StaticName;
    public override string Name => StaticName;
    public override string Url => "https://github.com/ggml-org/llama.cpp";

    public override SeLlamaCppAdvanced Settings
    {
        get => Se.Settings.AutoTranslate.LlamaCppAdvanced;
        set => Se.Settings.AutoTranslate.LlamaCppAdvanced = value;
    }

    protected override string GetApiUrl()
    {
        return AutoTranslateUrl.Complete(Configuration.Settings.Tools.LlamaCppApiUrl, LlamaCppTranslate.DefaultUrl);
    }
}
