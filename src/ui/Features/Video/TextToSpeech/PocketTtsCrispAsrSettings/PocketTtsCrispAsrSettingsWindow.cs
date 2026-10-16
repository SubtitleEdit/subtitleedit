using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.PocketTtsCrispAsrSettings;

/// <summary>
/// Pocket TTS (CrispASR) settings, in the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class PocketTtsCrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 170;

    private readonly PocketTtsCrispAsrSettingsViewModel _vm;

    public PocketTtsCrispAsrSettingsWindow(PocketTtsCrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "Pocket TTS");
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        // The weights are CC-BY-4.0 with Kyutai's additional gated-use conditions; surface that
        // (with a link to the model page) rather than leaving it buried in the download source.
        var license = TtsSettingsChrome.MakeLicenseLink("License: CC-BY-4.0 + Kyutai gated-use conditions", vm.OpenLicensePageCommand);
        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader("Pocket TTS (CrispASR)", new PocketTtsCrispAsr().Description, license),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm),
            BuildVoice(vm));
    }

    private static Border BuildModels(PocketTtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            (PocketTtsCrispAsr.ModelKeyEnglishF16, TtsSettingsChrome.MakeStatus(nameof(vm.EnglishF16Brush), nameof(vm.EnglishF16Label))),
            (PocketTtsCrispAsr.ModelKeyEnglishQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.EnglishQ8_0Brush), nameof(vm.EnglishQ8_0Label))),
            (PocketTtsCrispAsr.ModelKeyGermanQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.GermanBrush), nameof(vm.GermanLabel))),
            (PocketTtsCrispAsr.ModelKeySpanishQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.SpanishBrush), nameof(vm.SpanishLabel))),
            (PocketTtsCrispAsr.ModelKeyItalianQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.ItalianBrush), nameof(vm.ItalianLabel))),
            (PocketTtsCrispAsr.ModelKeyPortugueseQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.PortugueseBrush), nameof(vm.PortugueseLabel))),
            (PocketTtsCrispAsr.ModelKeyFrenchQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.FrenchBrush), nameof(vm.FrenchLabel))),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    private static Border BuildVoice(PocketTtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.Video.Voices, TtsSettingsChrome.MakeValue(nameof(vm.VoicesLabel),
                TtsSettingsChrome.MakeFolderButton(Se.Language.Video.TextToSpeech.OpenVoicesFolder, vm.OpenVoicesFolderCommand))),
            // Server accepts 0.25-4.0; cap UI at 0.5-2.0 since anything outside that range is
            // already a hack rather than a "preferred pace" choice.
            (Se.Language.General.Speed, TtsSettingsChrome.MakeSlider(Se.Language.General.Speed, 0.5, 2.0, 0.05, nameof(vm.Speed), nameof(vm.SpeedLabel))));
        return EditorChrome.MakeEditorSection(Se.Language.General.Voice, rows);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
