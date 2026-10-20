using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Qwen3TtsCrispAsrSettings;

/// <summary>
/// Qwen3 TTS (CrispASR) settings, in the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class Qwen3TtsCrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 150;

    private readonly Qwen3TtsCrispAsrSettingsViewModel _vm;

    public Qwen3TtsCrispAsrSettingsWindow(Qwen3TtsCrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "Qwen3 TTS");
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader("Qwen3 TTS (CrispASR)", new Qwen3TtsCrispAsr().Description),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm),
            BuildVoice(vm));
    }

    private static Border BuildModels(Qwen3TtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            ("1.7B VoiceDesign", TtsSettingsChrome.MakeStatus(nameof(vm.VoiceDesignTalkerBrush), nameof(vm.VoiceDesignTalkerLabel))),
            ("1.7B CustomVoice", TtsSettingsChrome.MakeStatus(nameof(vm.CustomVoiceTalkerBrush), nameof(vm.CustomVoiceTalkerLabel))),
            ("1.7B Voice clone", TtsSettingsChrome.MakeStatus(nameof(vm.CloneTalkerBrush), nameof(vm.CloneTalkerLabel))),
            ("Codec (12 Hz)", TtsSettingsChrome.MakeStatus(nameof(vm.CodecBrush), nameof(vm.CodecLabel))),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    private static Border BuildVoice(Qwen3TtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.Video.Voices, TtsSettingsChrome.MakeValue(nameof(vm.VoicesLabel),
                TtsSettingsChrome.MakeFolderButton(Se.Language.Video.TextToSpeech.OpenVoicesFolder, vm.OpenVoicesFolderCommand))));
        return EditorChrome.MakeEditorSection(Se.Language.General.Voice, rows);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
