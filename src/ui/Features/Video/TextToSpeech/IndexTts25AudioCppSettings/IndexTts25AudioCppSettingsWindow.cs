using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTts25AudioCppSettings;

/// <summary>
/// IndexTTS 2.5 (audio.cpp) settings, in the editor look (see <see cref="TtsSettingsChrome"/>).
/// </summary>
public class IndexTts25AudioCppSettingsWindow : Window
{
    private const int LabelWidth = 150;

    private readonly IndexTts25AudioCppSettingsViewModel _vm;

    public IndexTts25AudioCppSettingsWindow(IndexTts25AudioCppSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.IndexTts25SettingsTitle;
        TtsSettingsChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var close = UiUtil.MakeButtonOk(vm.OkCommand);
        Content = TtsSettingsChrome.MakeContent(
            TtsSettingsChrome.MakeHeader("IndexTTS 2.5 (audio.cpp)", new IndexTts25AudioCpp().Description),
            TtsSettingsChrome.MakeButtonBar(close),
            BuildModels(vm),
            BuildVoice(vm));
    }

    private static Border BuildModels(IndexTts25AudioCppSettingsViewModel vm)
    {
        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.General.Engine, TtsSettingsChrome.MakeStatus(nameof(vm.EngineBrush), nameof(vm.EngineLabel),
                TtsSettingsChrome.MakeEngineDownloadButton(vm.RedownloadEngineCommand, nameof(vm.EngineDownloadButtonText)))),
            ("Model " + IndexTts25AudioCpp.ModelKeyQ8_0, TtsSettingsChrome.MakeStatus(nameof(vm.ModelQ8_0Brush), nameof(vm.ModelQ8_0Label),
                TtsSettingsChrome.MakeModelDownloadButton(vm.DownloadModelCommand, IndexTts25AudioCpp.ModelKeyQ8_0))),
            ("Model " + IndexTts25AudioCpp.ModelKeyF16, TtsSettingsChrome.MakeStatus(nameof(vm.ModelF16Brush), nameof(vm.ModelF16Label),
                TtsSettingsChrome.MakeModelDownloadButton(vm.DownloadModelCommand, IndexTts25AudioCpp.ModelKeyF16))),
            (Se.Language.General.InstallFolder, TtsSettingsChrome.MakeFolder(nameof(vm.ModelsFolder), vm.OpenModelsFolderCommand)));
        return EditorChrome.MakeEditorSection(Se.Language.General.Models, rows);
    }

    private static Border BuildVoice(IndexTts25AudioCppSettingsViewModel vm)
    {
        var emotionCombo = new ComboBox
        {
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Emotions)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedEmotion)),
        };
        AutomationProperties.SetName(emotionCombo, Se.Language.Video.IndexTts25Emotion);

        var rows = TtsSettingsChrome.MakeRows(LabelWidth,
            (Se.Language.Video.Voices, TtsSettingsChrome.MakeValue(nameof(vm.VoicesLabel),
                TtsSettingsChrome.MakeFolderButton(Se.Language.Video.TextToSpeech.OpenVoicesFolder, vm.OpenVoicesFolderCommand))),
            (Se.Language.Video.IndexTts25Emotion, emotionCombo),
            // Emotion blend strength. Disabled while the emotion is "None", since the model ignores
            // it without an emotion vector — a live slider that changes nothing reads as a bug.
            (Se.Language.Video.IndexTts25EmotionStrength, TtsSettingsChrome.MakeSlider(Se.Language.Video.IndexTts25EmotionStrength, 0.0, 1.0, 0.05,
                nameof(vm.EmotionAlpha), nameof(vm.EmotionAlphaLabel), nameof(vm.IsEmotionAlphaEnabled))),
            // Speaking rate, presented the way users expect: right is faster. The engine converts
            // this to IndexTTS-2.5's own `duration_factor`, which runs the other way (>1 = longer
            // output = slower speech). Range capped at 0.5-2.0, which is what the model supports.
            (Se.Language.General.Speed, TtsSettingsChrome.MakeSlider(Se.Language.General.Speed, 0.5, 2.0, 0.05, nameof(vm.Speed), nameof(vm.SpeedLabel))));
        return EditorChrome.MakeEditorSection(Se.Language.General.Voice, rows);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
