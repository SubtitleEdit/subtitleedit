using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SpeakFromLine;

public class SpeakFromLineWindow : Window
{
    public SpeakFromLineWindow(SpeakFromLineViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Bind(TitleProperty, new Binding(nameof(vm.Title)));
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 400;
        vm.Window = this;
        DataContext = vm;

        const int comboWidth = 300;

        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithWidth(comboWidth);
        comboBoxEngines.ItemTemplate = new FuncDataTemplate<ITtsEngine>((engine, _) =>
            new TextBlock { Text = engine?.Name ?? string.Empty, VerticalAlignment = VerticalAlignment.Center });

        var labelEngineDescription = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(11.5),
            Opacity = 0.65,
            MaxWidth = comboWidth,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.EngineDescription)) { Mode = BindingMode.OneWay },
        };

        var comboBoxVoices = UiUtil.MakeComboBox(vm.Voices, vm, nameof(vm.SelectedVoice)).WithWidth(comboWidth);
        comboBoxVoices.ItemTemplate = new FuncDataTemplate<Voice>((_, _) =>
        {
            var textBlock = new TextBlock { VerticalAlignment = VerticalAlignment.Center };
            textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(Voice.DisplayName)));
            return textBlock;
        }, true);

        var comboBoxLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithWidth(comboWidth);
        var labelLanguage = UiUtil.MakeLabel(Se.Language.General.Language);
        labelLanguage.Bind(IsVisibleProperty, new Binding(nameof(vm.HasLanguageParameter)));
        comboBoxLanguages.Bind(IsVisibleProperty, new Binding(nameof(vm.HasLanguageParameter)));

        var labelHint = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.Hint)),
            Opacity = 0.8,
            MaxWidth = 420,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Left,
        };

        var checkBoxLowerVolume = new CheckBox
        {
            Content = Se.Language.Video.TextToSpeech.LowerVideoVolumeWhileSpeaking,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.LowerVideoVolume)),
            [!IsVisibleProperty] = new Binding(nameof(vm.IsPlayWithSpeech)),
        };
        var checkBoxPauseWhenLate = new CheckBox
        {
            Content = Se.Language.Video.TextToSpeech.PauseVideoWhenSpeechRunsLate,
            [!ToggleButton.IsCheckedProperty] = new Binding(nameof(vm.PauseVideoWhenLate)),
            [!IsVisibleProperty] = new Binding(nameof(vm.IsPlayWithSpeech)),
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Bind(IsEnabledProperty, new Binding(nameof(vm.IsBusy)) { Converter = Avalonia.Data.Converters.BoolConverters.Not });
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 8,
        };

        grid.Add(labelHint, 0, 0, 1, 2);
        grid.Add(UiUtil.MakeLabel(Se.Language.General.Engine), 1, 0);
        grid.Add(comboBoxEngines, 1, 1);
        grid.Add(labelEngineDescription, 2, 1);
        grid.Add(UiUtil.MakeLabel(Se.Language.General.Voice), 3, 0);
        grid.Add(comboBoxVoices, 3, 1);
        grid.Add(labelLanguage, 4, 0);
        grid.Add(comboBoxLanguages, 4, 1);
        grid.Add(checkBoxLowerVolume, 5, 1);
        grid.Add(checkBoxPauseWhenLate, 6, 1);
        grid.Add(buttonPanel, 7, 0, 1, 2);

        Content = grid;

        // Initial focus on an input, not an action button - a focused button clicks on bare Space.
        UiUtil.FocusOnFirstActivation(this, comboBoxVoices);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
