using Avalonia.Controls;
using Avalonia.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTtsCrispAsrSettings;

/// <summary>
/// IndexTTS (CrispASR) settings, in the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class IndexTtsCrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 150;

    private readonly IndexTtsCrispAsrSettingsViewModel _vm;

    public IndexTtsCrispAsrSettingsWindow(IndexTtsCrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "IndexTTS");
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader("IndexTTS (CrispASR)", new IndexTtsCrispAsr().Description),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm),
            BuildVoice(vm));
    }

    private static Border BuildModels(IndexTtsCrispAsrSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            ("GPT " + IndexTtsCrispAsr.ModelKeyQ4K, TtsSettingsChrome.MakeStatus(nameof(vm.TalkerQ4KBrush), nameof(vm.TalkerQ4KLabel))),
            ("GPT " + IndexTtsCrispAsr.ModelKeyQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.TalkerQ8_0Brush), nameof(vm.TalkerQ8_0Label))),
            ("GPT " + IndexTtsCrispAsr.ModelKeyF16, TtsSettingsChrome.MakeStatus(nameof(vm.TalkerF16Brush), nameof(vm.TalkerF16Label))),
            ("BigVGAN codec", TtsSettingsChrome.MakeStatus(nameof(vm.CodecBrush), nameof(vm.CodecLabel))),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    private static Border BuildVoice(IndexTtsCrispAsrSettingsViewModel vm)
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
