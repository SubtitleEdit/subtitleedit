using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SpeakFromLine;

/// <summary>
/// Speak from the current line / play with speech, in the editor look (see
/// <see cref="EditorChrome"/>): the hint, engine, voice and language, the playback switches,
/// Cancel and OK.
/// </summary>
public class SpeakFromLineWindow : Window
{
    private const double LabelWidth = 76;
    private const int ComboWidth = 300;

    public SpeakFromLineWindow(SpeakFromLineViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 400;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TextFieldsBare | EditorWindowStyles.MuteDisabledPrimary);
        vm.Window = this;
        DataContext = vm;

        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithWidth(ComboWidth);
        comboBoxEngines.ItemTemplate = new FuncDataTemplate<ITtsEngine>((engine, _) =>
            new TextBlock { Text = engine?.Name ?? string.Empty, VerticalAlignment = VerticalAlignment.Center });
        AutomationProperties.SetName(comboBoxEngines, Se.Language.General.Engine);

        var labelEngineDescription = TtsMainEditorLook.Hint(binding: nameof(vm.EngineDescription));
        labelEngineDescription.MaxWidth = ComboWidth;
        labelEngineDescription.HorizontalAlignment = HorizontalAlignment.Left;
        labelEngineDescription.Margin = new Thickness(0, -3, 0, 0);

        var comboBoxVoices = UiUtil.MakeComboBox(vm.Voices, vm, nameof(vm.SelectedVoice)).WithWidth(ComboWidth);
        comboBoxVoices.ItemTemplate = new FuncDataTemplate<Voice>((_, _) =>
        {
            var textBlock = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(Voice.DisplayName)));
            return textBlock;
        }, true);
        AutomationProperties.SetName(comboBoxVoices, Se.Language.General.Voice);

        var comboBoxLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithWidth(ComboWidth);
        AutomationProperties.SetName(comboBoxLanguages, Se.Language.General.Language);
        var rowLanguage = EditorChrome.MakeLabeledRow(Se.Language.General.Language, comboBoxLanguages, LabelWidth, spacing: 6);
        rowLanguage.Bind(IsVisibleProperty, new Binding(nameof(vm.HasLanguageParameter)));

        var labelHint = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Hint)),
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            MaxWidth = 420,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var switchLowerVolume = TtsMainEditorLook.SwitchRow(nameof(vm.LowerVideoVolume), Se.Language.Video.TextToSpeech.LowerVideoVolumeWhileSpeaking);
        var switchPauseWhenLate = TtsMainEditorLook.SwitchRow(nameof(vm.PauseVideoWhenLate), Se.Language.Video.TextToSpeech.PauseVideoWhenSpeechRunsLate);
        var playback = new StackPanel
        {
            Spacing = 8,
            Children = { switchLowerVolume, switchPauseWhenLate },
            [!IsVisibleProperty] = new Binding(nameof(vm.IsPlayWithSpeech)),
        };

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(labelHint);
        rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.General.Engine, comboBoxEngines, LabelWidth, spacing: 6));
        rows.Children.Add(EditorChrome.MakeLabeledRow((string?)null, labelEngineDescription, LabelWidth, spacing: 6));
        rows.Children.Add(EditorChrome.MakeLabeledRow(Se.Language.General.Voice, comboBoxVoices, LabelWidth, spacing: 6));
        rows.Children.Add(rowLanguage);
        rows.Children.Add(playback);

        var buttonOk = EditorChrome.AsPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        buttonOk.Bind(IsEnabledProperty, new Binding(nameof(vm.IsBusy)) { Converter = Avalonia.Data.Converters.BoolConverters.Not });
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
        };
        grid.Add(new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = EditorChrome.MakeEditorSection(Se.Language.Video.TextToSpeech.EngineAndVoice, rows, first: true),
        }, 0, 0);
        grid.Add(EditorChrome.MakeButtonBar(null, buttonCancel, buttonOk), 1, 0);

        Content = grid;

        // Initial focus on an input, not an action button - a focused button clicks on bare Space.
        UiUtil.FocusOnFirstActivation(this, comboBoxVoices);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
