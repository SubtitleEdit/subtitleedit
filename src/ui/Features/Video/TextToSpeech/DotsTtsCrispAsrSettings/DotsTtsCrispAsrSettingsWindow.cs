using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DotsTtsCrispAsrSettings;

/// <summary>
/// dots.tts (CrispASR) settings, in the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class DotsTtsCrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 150;

    private readonly DotsTtsCrispAsrSettingsViewModel _vm;

    public DotsTtsCrispAsrSettingsWindow(DotsTtsCrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "dots.tts");
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader("dots.tts (CrispASR)", new DotsTtsCrispAsr().Description),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm),
            BuildVoice(vm));
    }

    private static Border BuildModels(DotsTtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            ("Model " + DotsTtsCrispAsr.ModelKeyQ4K, TtsSettingsChrome.MakeStatus(nameof(vm.CoreQ4KBrush), nameof(vm.CoreQ4KLabel))),
            ("Model " + DotsTtsCrispAsr.ModelKeyQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.CoreQ8_0Brush), nameof(vm.CoreQ8_0Label))),
            ("Model " + DotsTtsCrispAsr.ModelKeyF16, TtsSettingsChrome.MakeStatus(nameof(vm.CoreF16Brush), nameof(vm.CoreF16Label))),
            ("BigVGAN vocoder", TtsSettingsChrome.MakeStatus(nameof(vm.VocoderBrush), nameof(vm.VocoderLabel))),
            ("CAM++ speaker encoder", TtsSettingsChrome.MakeStatus(nameof(vm.SpeakerEncoderBrush), nameof(vm.SpeakerEncoderLabel))),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    private static Border BuildVoice(DotsTtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.Video.Voices, TtsSettingsChrome.MakeValue(nameof(vm.VoicesLabel),
                TtsSettingsChrome.MakeFolderButton(Se.Language.Video.TextToSpeech.OpenVoicesFolder, vm.OpenVoicesFolderCommand))),
            // Flow-matching Euler steps: the one real quality/speed knob dots.tts exposes. Measured on
            // M4 Metal with the F16 core, 4 steps synthesised in 25 s where 16 steps took 38 s for a
            // comparable line, so this is worth surfacing rather than hard-coding.
            (Se.Language.General.Quality, TtsSettingsChrome.MakeSlider(Se.Language.General.Quality, DotsTtsCrispAsr.MinOdeSteps, DotsTtsCrispAsr.MaxOdeSteps, 1, nameof(vm.OdeSteps), nameof(vm.OdeStepsLabel))));
        return EditorChrome.MakeEditorSection(Se.Language.General.Voice, rows);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
