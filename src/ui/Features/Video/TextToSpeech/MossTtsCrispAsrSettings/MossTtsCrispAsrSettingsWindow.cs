using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.MossTtsCrispAsrSettings;

/// <summary>
/// MOSS-TTS (CrispASR) settings, in the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class MossTtsCrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 150;

    private readonly MossTtsCrispAsrSettingsViewModel _vm;

    public MossTtsCrispAsrSettingsWindow(MossTtsCrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "MOSS-TTS");
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader("MOSS-TTS (CrispASR)", new MossTtsCrispAsr().Description),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm),
            BuildVoice(vm));
    }

    private static Border BuildModels(MossTtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            ("Model " + MossTtsCrispAsr.ModelKeyQ4K, TtsSettingsChrome.MakeStatus(nameof(vm.ModelQ4KBrush), nameof(vm.ModelQ4KLabel))),
            ("Model " + MossTtsCrispAsr.ModelKeyQ6K, TtsSettingsChrome.MakeStatus(nameof(vm.ModelQ6KBrush), nameof(vm.ModelQ6KLabel))),
            ("Model " + MossTtsCrispAsr.ModelKeyQ8, TtsSettingsChrome.MakeStatus(nameof(vm.ModelQ8Brush), nameof(vm.ModelQ8Label))),
            ("Model " + MossTtsCrispAsr.ModelKeyF16, TtsSettingsChrome.MakeStatus(nameof(vm.ModelF16Brush), nameof(vm.ModelF16Label))),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    private static Border BuildVoice(MossTtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.Video.Voices, TtsSettingsChrome.MakeValue(nameof(vm.VoicesLabel),
                TtsSettingsChrome.MakeFolderButton(Se.Language.Video.TextToSpeech.OpenVoicesFolder, vm.OpenVoicesFolderCommand))),
            (Se.Language.General.Speed, TtsSettingsChrome.MakeSlider(Se.Language.General.Speed, 0.5, 2.0, 0.05, nameof(vm.Speed), nameof(vm.SpeedLabel))));
        return EditorChrome.MakeEditorSection(Se.Language.General.Voice, rows);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
