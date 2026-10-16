using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Confucius4TtsCrispAsrSettings;

/// <summary>
/// Confucius4-TTS (CrispASR) settings, in the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class Confucius4TtsCrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 150;

    private readonly Confucius4TtsCrispAsrSettingsViewModel _vm;

    public Confucius4TtsCrispAsrSettingsWindow(Confucius4TtsCrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "Confucius4-TTS");
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader("Confucius4-TTS (CrispASR)", new Confucius4TtsCrispAsr().Description),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm),
            BuildVoice(vm));
    }

    private static Border BuildModels(Confucius4TtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            ("Model " + Confucius4TtsCrispAsr.ModelKeyQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.CoreQ8_0Brush), nameof(vm.CoreQ8_0Label))),
            ("Model " + Confucius4TtsCrispAsr.ModelKeyF16, TtsSettingsChrome.MakeStatus(nameof(vm.CoreF16Brush), nameof(vm.CoreF16Label))),
            ("BigVGAN vocoder", TtsSettingsChrome.MakeStatus(nameof(vm.VocoderBrush), nameof(vm.VocoderLabel))),
            ("w2v-BERT encoder", TtsSettingsChrome.MakeStatus(nameof(vm.W2vBrush), nameof(vm.W2vLabel))),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    private static Border BuildVoice(Confucius4TtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.Video.Voices, TtsSettingsChrome.MakeValue(nameof(vm.VoicesLabel),
                TtsSettingsChrome.MakeFolderButton(Se.Language.Video.TextToSpeech.OpenVoicesFolder, vm.OpenVoicesFolderCommand))),
            // Flow-matching Euler steps for the S2A stage — the quality/speed knob this backend exposes
            // (--tts-steps; CrispASR default 20, upstream reference samples at 25).
            (Se.Language.General.Quality, TtsSettingsChrome.MakeSlider(Se.Language.General.Quality, Confucius4TtsCrispAsr.MinOdeSteps, Confucius4TtsCrispAsr.MaxOdeSteps, 1, nameof(vm.OdeSteps), nameof(vm.OdeStepsLabel))));
        return EditorChrome.MakeEditorSection(Se.Language.General.Voice, rows);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
