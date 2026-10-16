using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.CosyVoice3CrispAsrSettings;

/// <summary>
/// CosyVoice3 (CrispASR) settings, in the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class CosyVoice3CrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 150;

    private readonly CosyVoice3CrispAsrSettingsViewModel _vm;

    public CosyVoice3CrispAsrSettingsWindow(CosyVoice3CrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "CosyVoice3");
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader("CosyVoice3 (CrispASR)", new CosyVoice3CrispAsr().Description),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm),
            BuildVoice(vm));
    }

    private static Border BuildModels(CosyVoice3CrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            (CosyVoice3CrispAsr.ModelKeyQ4K, TtsSettingsChrome.MakeStatus(nameof(vm.Q4KBundleBrush), nameof(vm.Q4KBundleLabel))),
            (CosyVoice3CrispAsr.ModelKeyF16, TtsSettingsChrome.MakeStatus(nameof(vm.F16BundleBrush), nameof(vm.F16BundleLabel))),
            // The RL talker bundles (#13272) share every companion with the two above - only the LLM
            // file differs - so an installed RL row means one extra GGUF on disk, not a second bundle.
            (CosyVoice3CrispAsr.ModelKeyRlQ4K, TtsSettingsChrome.MakeStatus(nameof(vm.RlQ4KBundleBrush), nameof(vm.RlQ4KBundleLabel))),
            (CosyVoice3CrispAsr.ModelKeyRlF16, TtsSettingsChrome.MakeStatus(nameof(vm.RlF16BundleBrush), nameof(vm.RlF16BundleLabel))),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    private static Border BuildVoice(CosyVoice3CrispAsrSettingsViewModel vm)
    {
        // Language spoken in imported reference WAVs — sent as `source_lang` so cross-lingual
        // cloning engages when the target language differs (the server cannot detect the
        // reference language itself for Latin scripts). Presets carry their own bank language.
        var sourceLanguageCombo = UiUtil.MakeComboBox(vm.SourceLanguages, vm, nameof(vm.SelectedSourceLanguage));
        sourceLanguageCombo.MinWidth = 200;
        sourceLanguageCombo.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
        AutomationProperties.SetName(sourceLanguageCombo, "Reference language");
        var sourceLanguage = new StackPanel
        {
            Spacing = 4,
            Children =
            {
                sourceLanguageCombo,
                EditorChrome.MakeHint("Language spoken in imported reference WAVs (for cross-lingual cloning). "
                                           + "Auto detects it from each voice's transcript."),
            },
        };

        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.Video.Presets, TtsSettingsChrome.MakeValue(nameof(vm.PresetsLabel))),
            (Se.Language.Video.Voices, TtsSettingsChrome.MakeValue(nameof(vm.VoicesLabel),
                TtsSettingsChrome.MakeFolderButton(Se.Language.Video.TextToSpeech.OpenVoicesFolder, vm.OpenVoicesFolderCommand))),
            (Se.Language.General.Speed, TtsSettingsChrome.MakeSlider(Se.Language.General.Speed, 0.5, 2.0, 0.05, nameof(vm.Speed), nameof(vm.SpeedLabel))),
            ("Reference language", sourceLanguage));
        return EditorChrome.MakeEditorSection(Se.Language.General.Voice, rows);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
