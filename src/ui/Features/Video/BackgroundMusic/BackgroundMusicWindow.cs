using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.BackgroundMusic;

public class BackgroundMusicWindow : Window
{
    private readonly BackgroundMusicViewModel _vm;

    public BackgroundMusicWindow(BackgroundMusicViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        var l = Se.Language.Video.BackgroundMusic;
        Title = l.Title;
        Width = 720;
        MinWidth = 560;
        SizeToContent = SizeToContent.Height;
        CanResize = true;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        const int labelWidth = 170;

        var labelPreset = UiUtil.MakeLabel(l.Preset);
        var comboBoxPreset = UiUtil.MakeComboBox(vm.Presets, vm, nameof(vm.SelectedPreset), null);
        comboBoxPreset.Width = 340;
        comboBoxPreset.WithBindEnabled(nameof(vm.IsNotBusy));

        var labelPrompt = UiUtil.MakeLabel(l.Prompt);
        labelPrompt.VerticalAlignment = VerticalAlignment.Top;
        var textBoxPrompt = new TextBox
        {
            AcceptsReturn = false,
            TextWrapping = TextWrapping.Wrap,
            Height = 90,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.Prompt)) { Mode = BindingMode.TwoWay },
            [!TextBox.IsEnabledProperty] = new Binding(nameof(vm.IsNotBusy)),
        };
        AutomationProperties.SetName(textBoxPrompt, l.Prompt);

        var labelBpm = UiUtil.MakeLabel(l.TempoBpm);
        var numericBpm = UiUtil.MakeNumericUpDownInt(40, 200, 120, 130, vm, nameof(vm.Bpm)).WithBindEnabled(nameof(vm.IsNotBusy));

        var labelGenerateSeconds = UiUtil.MakeLabel(l.GenerateSeconds);
        var numericGenerateSeconds = UiUtil.MakeNumericUpDownInt(20, 240, 60, 130, vm, nameof(vm.GenerateSeconds)).WithBindEnabled(nameof(vm.IsNotBusy));

        var labelSeed = UiUtil.MakeLabel(l.Seed);
        var numericSeed = new NumericUpDown
        {
            Minimum = 0,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
            Width = 130,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.Seed)) { Mode = BindingMode.TwoWay },
            [!NumericUpDown.IsEnabledProperty] = new Binding(nameof(vm.IsNotBusy)),
        };
        AutomationProperties.SetName(numericSeed, l.Seed);
        var checkBoxRandomSeed = UiUtil.MakeCheckBox(l.RandomSeed, vm, nameof(vm.UseRandomSeed)).WithBindEnabled(nameof(vm.IsNotBusy));
        var panelSeed = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children = { numericSeed, checkBoxRandomSeed },
        };

        var labelOutputLength = UiUtil.MakeLabel(l.OutputLength);
        var textOutputLength = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.OutputLengthText));

        var labelMusicVolume = UiUtil.MakeLabel(l.MusicVolumePercent);
        var numericMusicVolume = UiUtil.MakeNumericUpDownInt(0, 200, 100, 130, vm, nameof(vm.MusicVolumePercent)).WithBindEnabled(nameof(vm.IsNotBusy));

        var checkBoxRemoveAudio = UiUtil.MakeCheckBox(l.RemoveExistingAudioTracks, vm, nameof(vm.RemoveExistingAudioTracks)).WithBindEnabled(nameof(vm.IsNotBusy));
        checkBoxRemoveAudio.Bind(CheckBox.IsVisibleProperty, new Binding(nameof(vm.IsVideoMode)));

        var labelOriginalVolume = UiUtil.MakeLabel(l.OriginalAudioVolumePercent).WithBindVisible(vm, nameof(vm.ShowOriginalAudioVolume));
        var numericOriginalVolume = UiUtil.MakeNumericUpDownInt(0, 200, 100, 130, vm, nameof(vm.OriginalAudioVolumePercent), nameof(vm.ShowOriginalAudioVolume))
            .WithBindEnabled(nameof(vm.IsNotBusy));

        var labelEngine = UiUtil.MakeLabel(l.EngineInfo);
        labelEngine.Opacity = 0.7;

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(labelWidth, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto }, // preset
                new RowDefinition { Height = GridLength.Auto }, // prompt
                new RowDefinition { Height = GridLength.Auto }, // bpm
                new RowDefinition { Height = GridLength.Auto }, // generate seconds
                new RowDefinition { Height = GridLength.Auto }, // seed
                new RowDefinition { Height = GridLength.Auto }, // output length
                new RowDefinition { Height = GridLength.Auto }, // music volume
                new RowDefinition { Height = GridLength.Auto }, // remove audio
                new RowDefinition { Height = GridLength.Auto }, // original volume
                new RowDefinition { Height = GridLength.Auto }, // engine info
            },
            ColumnSpacing = 10,
            RowSpacing = 8,
        };

        grid.Add(labelPreset, 0, 0);
        grid.Add(comboBoxPreset, 0, 1);
        grid.Add(labelPrompt, 1, 0);
        grid.Add(textBoxPrompt, 1, 1);
        grid.Add(labelBpm, 2, 0);
        grid.Add(numericBpm, 2, 1);
        grid.Add(labelGenerateSeconds, 3, 0);
        grid.Add(numericGenerateSeconds, 3, 1);
        grid.Add(labelSeed, 4, 0);
        grid.Add(panelSeed, 4, 1);
        grid.Add(labelOutputLength, 5, 0);
        grid.Add(textOutputLength, 5, 1);
        grid.Add(labelMusicVolume, 6, 0);
        grid.Add(numericMusicVolume, 6, 1);
        grid.Add(checkBoxRemoveAudio, 7, 1);
        grid.Add(labelOriginalVolume, 8, 0);
        grid.Add(numericOriginalVolume, 8, 1);
        grid.Add(labelEngine, 9, 0, 1, 2);

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsBusy)));
        var textProgress = new TextBlock
        {
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ProgressText)),
            [!TextBlock.IsVisibleProperty] = new Binding(nameof(vm.IsBusy)),
        };
        var textResult = new TextBlock
        {
            TextTrimming = TextTrimming.CharacterEllipsis,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ResultText)),
            [!TextBlock.IsVisibleProperty] = new Binding(nameof(vm.IsNotBusy)),
        };
        var panelProgress = new StackPanel
        {
            Spacing = 4,
            Height = 44, // reserve the space so the window does not jump when generating starts
            Margin = new Thickness(0, 10, 0, 0),
            Children = { progressBar, textProgress, textResult },
        };

        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand).WithBindEnabled(nameof(vm.IsNotBusy));
        var buttonPlay = UiUtil.MakeButton(Se.Language.General.Play, vm.PlayStopCommand).WithBindEnabled(nameof(vm.HasMusic));
        buttonPlay.Bind(Button.ContentProperty, new Binding(nameof(vm.IsPlaying))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<bool, string>(playing => playing ? Se.Language.General.Stop : Se.Language.General.Play),
        });
        var buttonSaveAudio = UiUtil.MakeButton(l.SaveAudioDotDotDot, vm.SaveAudioCommand).WithBindEnabled(nameof(vm.HasMusic));
        var buttonAddToVideo = UiUtil.MakeButton(l.AddToVideoDotDotDot, vm.AddToVideoCommand).WithBindEnabled(nameof(vm.CanAddToVideo));
        buttonAddToVideo.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsVideoMode)));
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).WithBindEnabled(nameof(vm.IsNotBusy));
        buttonOk.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsTextToSpeechMode)));
        var buttonCancel = UiUtil.MakeButton(Se.Language.General.Close, vm.CancelCommand);
        buttonCancel.Bind(Button.ContentProperty, new Binding(nameof(vm.IsBusy))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<bool, string>(busy => busy ? Se.Language.General.Cancel : Se.Language.General.Close),
        });

        var buttonBar = UiUtil.MakeButtonBar(buttonGenerate, buttonPlay, buttonSaveAudio, buttonAddToVideo, buttonOk, buttonCancel);

        var root = new StackPanel
        {
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                UiUtil.MakeBorderForControl(grid),
                panelProgress,
                buttonBar,
            },
        };

        Content = root;

        UiUtil.FocusOnFirstActivation(this, () => comboBoxPreset.Focus());
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
