using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.BackgroundMusic;

/// <summary>
/// Generate background music, in the editor look (see <see cref="EditorChrome"/>): the music
/// settings on the left, the mix and the generated result (play, save, add to video) on the right.
/// </summary>
public class BackgroundMusicWindow : Window
{
    private readonly BackgroundMusicViewModel _vm;

    private const double LabelWidth = 130;

    public BackgroundMusicWindow(BackgroundMusicViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        var l = Se.Language.Video.BackgroundMusic;
        Title = l.Title;
        Width = 780;
        MinWidth = 640;
        SizeToContent = SizeToContent.Height;
        CanResize = true;
        VideoSubDialogChrome.Apply(this);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var comboBoxPreset = UiUtil.MakeComboBox(vm.Presets, vm, nameof(vm.SelectedPreset), null);
        comboBoxPreset.Width = double.NaN;
        comboBoxPreset.HorizontalAlignment = HorizontalAlignment.Stretch;
        comboBoxPreset.WithBindEnabled(nameof(vm.IsNotBusy));
        AutomationProperties.SetName(comboBoxPreset, l.Preset);

        var settings = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)), // music
                new ColumnDefinition(new GridLength(300)), // mix + result
            },
        };
        settings.Add(MakeMusicSection(vm, comboBoxPreset), 0, 0);
        var right = new StackPanel();
        right.Children.Add(MakeMixSection(vm));
        right.Children.Add(MakeResultSection(vm));
        settings.Add(new Border
        {
            Child = right,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(1, 0, 0, 0),
        }, 0, 1);

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(new GridLength(1, GridUnitType.Star)), // settings
                new RowDefinition(GridLength.Auto), // buttons
            },
        };
        root.Add(new Border
        {
            Background = EditorChrome.PanelBrush,
            Child = new ScrollViewer
            {
                Content = settings,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            },
        }, 0, 0);
        root.Add(MakeButtonBar(vm), 1, 0);

        Content = root;

        UiUtil.FocusOnFirstActivation(this, () => comboBoxPreset.Focus());
    }

    private static Border MakeMusicSection(BackgroundMusicViewModel vm, ComboBox comboBoxPreset)
    {
        var l = Se.Language.Video.BackgroundMusic;

        var textBoxPrompt = new TextBox
        {
            AcceptsReturn = false,
            TextWrapping = TextWrapping.Wrap,
            Height = 90,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.Prompt)) { Mode = BindingMode.TwoWay },
            [!TextBox.IsEnabledProperty] = new Binding(nameof(vm.IsNotBusy)),
        };
        textBoxPrompt.Classes.Add("text");
        textBoxPrompt.VerticalContentAlignment = VerticalAlignment.Top;
        AutomationProperties.SetName(textBoxPrompt, l.Prompt);

        var numericBpm = Field(nameof(vm.Bpm), 40, 200, l.TempoBpm);
        var numericGenerateSeconds = Field(nameof(vm.GenerateSeconds), 20, 240, l.GenerateSeconds);

        var numericSeed = Field(nameof(vm.Seed), 0, int.MaxValue, l.Seed);
        numericSeed.Width = 120;
        var switchRandomSeed = VideoSubDialogChrome.MakeSwitch(nameof(vm.UseRandomSeed), l.RandomSeed);
        switchRandomSeed.Bind(IsEnabledProperty, new Binding(nameof(vm.IsNotBusy)));
        var panelSeed = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Children = { numericSeed, VideoSubDialogChrome.MakeSwitchRow(switchRandomSeed, l.RandomSeed) },
        };

        var textOutputLength = new TextBlock
        {
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.OutputLengthText)),
        };

        var labelPrompt = EditorChrome.MakeDimLabel(l.Prompt, new Thickness(0, 6, 0, 0));
        labelPrompt.VerticalAlignment = VerticalAlignment.Top;
        var promptRow = new Grid
        {
            ColumnSpacing = 6,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        promptRow.Add(labelPrompt, 0, 0);
        promptRow.Add(textBoxPrompt, 0, 1);

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(VideoSubDialogChrome.MakeRow(l.Preset, comboBoxPreset, LabelWidth));
        rows.Children.Add(promptRow);
        rows.Children.Add(VideoSubDialogChrome.MakeRow(l.TempoBpm, numericBpm, LabelWidth));
        rows.Children.Add(VideoSubDialogChrome.MakeRow(l.GenerateSeconds, numericGenerateSeconds, LabelWidth));
        rows.Children.Add(VideoSubDialogChrome.MakeRow(l.Seed, panelSeed, LabelWidth));
        rows.Children.Add(VideoSubDialogChrome.MakeRow(l.OutputLength, textOutputLength, LabelWidth));

        return EditorChrome.MakeEditorSection(l.Music, rows, first: true);
    }

    private static Border MakeMixSection(BackgroundMusicViewModel vm)
    {
        var l = Se.Language.Video.BackgroundMusic;

        var numericMusicVolume = Field(nameof(vm.MusicVolumePercent), 0, 200, l.MusicVolumePercent);

        var switchRemoveAudio = VideoSubDialogChrome.MakeSwitch(nameof(vm.RemoveExistingAudioTracks), l.RemoveExistingAudioTracks);
        switchRemoveAudio.Bind(IsEnabledProperty, new Binding(nameof(vm.IsNotBusy)));
        var rowRemoveAudio = VideoSubDialogChrome.MakeSwitchRow(switchRemoveAudio, l.RemoveExistingAudioTracks);
        rowRemoveAudio.Bind(IsVisibleProperty, new Binding(nameof(vm.IsVideoMode)));

        var numericOriginalVolume = Field(nameof(vm.OriginalAudioVolumePercent), 0, 200, l.OriginalAudioVolumePercent);
        var rowOriginalVolume = VideoSubDialogChrome.MakeRow(l.OriginalAudioVolumePercent, numericOriginalVolume, LabelWidth);
        rowOriginalVolume.Bind(IsVisibleProperty, new Binding(nameof(vm.ShowOriginalAudioVolume)));

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(VideoSubDialogChrome.MakeRow(l.MusicVolumePercent, numericMusicVolume, LabelWidth));
        rows.Children.Add(rowRemoveAudio);
        rows.Children.Add(rowOriginalVolume);

        return EditorChrome.MakeEditorSection(Se.Language.Video.BurnIn.Audio, rows, first: true);
    }

    /// <summary>
    /// Progress while generating, the result after it, and what to do with the music.
    /// </summary>
    private static Border MakeResultSection(BackgroundMusicViewModel vm)
    {
        var l = Se.Language.Video.BackgroundMusic;

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        var textProgress = VideoSubDialogChrome.MakeHint(nameof(vm.ProgressText));
        var panelBusy = new StackPanel
        {
            Spacing = 6,
            Children = { progressBar, textProgress },
            [!IsVisibleProperty] = new Binding(nameof(vm.IsBusy)),
        };
        var textResult = new TextBlock
        {
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Foreground = EditorChrome.DimTextBrush,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ResultText)),
            [!IsVisibleProperty] = new Binding(nameof(vm.IsNotBusy)),
        };

        // Room is kept for the progress, so the window does not jump when generating starts
        var status = new Panel { MinHeight = 40, Children = { panelBusy, textResult } };

        var buttonPlay = MakeAction("fa-solid fa-play", Se.Language.General.Play, vm.PlayStopCommand);
        buttonPlay.WithBindEnabled(nameof(vm.HasMusic));
        var playText = (TextBlock)((StackPanel)buttonPlay.Content!).Children[1];
        var playIcon = (Optris.Icons.Avalonia.Icon)((StackPanel)buttonPlay.Content!).Children[0];
        playText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.IsPlaying))
        {
            Converter = new FuncValueConverter<bool, string>(playing => playing ? Se.Language.General.Stop : Se.Language.General.Play),
        });
        playIcon.Bind(Optris.Icons.Avalonia.Icon.ValueProperty, new Binding(nameof(vm.IsPlaying))
        {
            Converter = new FuncValueConverter<bool, string>(playing => playing ? "fa-solid fa-stop" : "fa-solid fa-play"),
        });
        buttonPlay.Bind(AutomationProperties.NameProperty, new Binding(nameof(vm.IsPlaying))
        {
            Converter = new FuncValueConverter<bool, string>(playing => playing ? Se.Language.General.Stop : Se.Language.General.Play),
        });

        var buttonSaveAudio = MakeAction("fa-solid fa-floppy-disk", l.SaveAudioDotDotDot, vm.SaveAudioCommand);
        buttonSaveAudio.WithBindEnabled(nameof(vm.HasMusic));
        var buttonAddToVideo = MakeAction("fa-solid fa-film", l.AddToVideoDotDotDot, vm.AddToVideoCommand);
        buttonAddToVideo.WithBindEnabled(nameof(vm.CanAddToVideo));
        buttonAddToVideo.Bind(IsVisibleProperty, new Binding(nameof(vm.IsVideoMode)));

        var actions = new WrapPanel { Orientation = Orientation.Horizontal };
        actions.Children.Add(buttonPlay);
        actions.Children.Add(buttonSaveAudio);
        actions.Children.Add(buttonAddToVideo);

        var rows = new StackPanel { Spacing = 8 };
        rows.Children.Add(status);
        rows.Children.Add(actions);

        return EditorChrome.MakeEditorSection(Se.Language.Video.CutVideoOutput, rows);
    }

    private static Button MakeAction(string icon, string text, System.Windows.Input.ICommand command)
    {
        var button = VideoSubDialogChrome.MakeAction(icon, text, command);
        ((StackPanel)button.Content!).Margin = new Thickness(8, 0);
        return button;
    }

    private static NumericUpDown Field(string property, double min, double max, string name)
    {
        var field = EditorChrome.MakeField(property, min, max, 90);
        field.HorizontalAlignment = HorizontalAlignment.Left;
        field.Bind(IsEnabledProperty, new Binding(nameof(BackgroundMusicViewModel.IsNotBusy)));
        AutomationProperties.SetName(field, name);
        return field;
    }

    private static Border MakeButtonBar(BackgroundMusicViewModel vm)
    {
        var l = Se.Language.Video.BackgroundMusic;

        var engine = new TextBlock
        {
            Text = l.EngineInfo,
            FontSize = 11.5,
            Foreground = EditorChrome.FaintTextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(engine, l.EngineInfo);
        }

        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand).WithBindEnabled(nameof(vm.IsNotBusy));
        buttonGenerate.Classes.Add("primary");
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsNotBusy));
        buttonOk.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsTextToSpeechMode)));
        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Close, vm.CancelCommand);
        buttonCancel.Bind(Button.ContentProperty, new Binding(nameof(vm.IsBusy))
        {
            Converter = new FuncValueConverter<bool, string>(busy => busy ? Se.Language.General.Cancel : Se.Language.General.Close),
        });

        return VideoSubDialogChrome.MakeButtonBar(engine, buttonCancel, buttonOk, buttonGenerate);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
