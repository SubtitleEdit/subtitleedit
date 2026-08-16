using Avalonia.Controls.Templates;
using Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.LlamaCpp;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.UiLogic.LlamaCpp;

namespace Nikse.SubtitleEdit.Features.Translate;

/// <summary>
/// Install-status dot templates for the auto-translate combos (engine, llama.cpp model, CrispASR
/// model), shared by the Auto-translate window and batch convert so the same list cannot show dots
/// in one place and plain text in the other.
/// Each template evaluates its dot when a row is first realised, so re-filling the bound collection
/// (or re-assigning the template) is what refreshes the dots after a download - see
/// <see cref="StatusDots.ComboItemTemplate{T}"/>.
/// </summary>
public static class AutoTranslateCombos
{
    public static FuncDataTemplate<IAutoTranslator> EngineItemTemplate()
    {
        return StatusDots.ComboItemTemplate<IAutoTranslator>(
            translator => translator.Name,
            _ => null,
            GetTranslatorDotStatus);
    }

    public static FuncDataTemplate<LlamaCppModelDisplay> LlamaCppModelItemTemplate()
    {
        return StatusDots.ComboItemTemplate<LlamaCppModelDisplay>(
            model => model.Model.DisplayName,
            GetLlamaCppModelSize,
            GetLlamaCppModelDotStatus);
    }

    public static FuncDataTemplate<SpeechToTextModelDisplay> CrispAsrModelItemTemplate()
    {
        return StatusDots.ComboItemTemplate<SpeechToTextModelDisplay>(
            model => model.Model.Name,
            model => string.IsNullOrEmpty(model.Model.Size) ? null : model.Model.Size,
            model => model.Engine.IsModelInstalled(model.Model)
                ? DownloadDotStatus.UpToDate
                : DownloadDotStatus.NotInstalled);
    }

    // Install-status dot for the auto-translate engine combo. Only the two engines that Subtitle
    // Edit downloads itself - llama.cpp and CrispASR/MADLAD - get a dot; cloud/API translators
    // (Google, DeepL, ChatGPT, ...) and externally-hosted servers have nothing to install.
    private static DownloadDotStatus GetTranslatorDotStatus(IAutoTranslator translator)
    {
        switch (translator)
        {
            case LlamaCppTranslate:
            case LlamaCppAdvancedTranslate:
                return StatusDots.From(
                    LlamaCppServerManager.IsEngineInstalled(),
                    LlamaCppUpdateStatus.GetEngineUpdateStatus());
            case CrispAsrMadladTranslate:
                var crispAsr = new CrispAsrMadlad();
                if (!crispAsr.IsEngineInstalled())
                {
                    return DownloadDotStatus.NotInstalled;
                }

                return StatusDots.From(true, DownloadHashManager.GetSidecarStatus(crispAsr.GetAndCreateWhisperFolder()));
            default:
                return DownloadDotStatus.None;
        }
    }

    // A custom *.gguf the user dropped into the models folder has no Url - it is already on disk,
    // so it shows a green dot and a "custom" size tag rather than a download size.
    private static string? GetLlamaCppModelSize(LlamaCppModelDisplay model)
    {
        if (string.IsNullOrEmpty(model.Model.Url))
        {
            var custom = Se.Language.General.Custom;
            return string.IsNullOrEmpty(model.Model.Size) ? custom : $"{custom}, {model.Model.Size}";
        }

        return string.IsNullOrEmpty(model.Model.Size) ? null : model.Model.Size;
    }

    private static DownloadDotStatus GetLlamaCppModelDotStatus(LlamaCppModelDisplay model)
    {
        return model.IsInstalled ? DownloadDotStatus.UpToDate : DownloadDotStatus.NotInstalled;
    }
}
