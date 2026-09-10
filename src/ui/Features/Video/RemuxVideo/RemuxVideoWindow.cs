using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Video.RemuxVideo;

public class RemuxVideoWindow : Window
{
    private readonly RemuxVideoViewModel _vm;
    private Button? _buttonBrowseVideo;

    public RemuxVideoWindow(RemuxVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.RemuxVideoTitle;
        CanResize = false;
        SizeToContent = SizeToContent.Height;
        Width = 700;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var contentPanel = new StackPanel
        {
            Spacing = 10,
            Margin = UiUtil.MakeWindowMargin(),
        };

        // 1. Input Video Section
        var labelVideo = UiUtil.MakeLabel(Se.Language.Video.RemuxVideoInputVideo);
        var textBoxVideo = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.VideoFileName));
        textBoxVideo.HorizontalAlignment = HorizontalAlignment.Stretch;
        var labelVideoSize = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.VideoFileSize)).WithMarginRight(5);
        _buttonBrowseVideo = UiUtil.MakeButtonBrowse(vm.BrowseVideoCommand);
        _buttonBrowseVideo.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance });

        var gridVideo = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
        };
        gridVideo.Add(textBoxVideo, 0, 0);
        gridVideo.Add(labelVideoSize, 0, 1);
        gridVideo.Add(_buttonBrowseVideo, 0, 2);

        // 2. Input Audio Section
        var labelAudio = UiUtil.MakeLabel(Se.Language.Video.RemuxVideoInputAudio);
        var textBoxAudio = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.AudioFileName));
        textBoxAudio.HorizontalAlignment = HorizontalAlignment.Stretch;
        var labelAudioSize = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.AudioFileSize)).WithMarginRight(5);
        var buttonBrowseAudio = UiUtil.MakeButtonBrowse(vm.BrowseAudioCommand);
        buttonBrowseAudio.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance });

        var gridAudio = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
        };
        gridAudio.Add(textBoxAudio, 0, 0);
        gridAudio.Add(labelAudioSize, 0, 1);
        gridAudio.Add(buttonBrowseAudio, 0, 2);

        var labelAudioTrack = UiUtil.MakeLabel(Se.Language.Video.RemuxVideoAudioTrack);
        var comboBoxAudioTrack = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            DisplayMemberBinding = new Binding(nameof(AudioTrackOption.DisplayName)),
        };
        comboBoxAudioTrack.Bind(ComboBox.ItemsSourceProperty, new Binding(nameof(vm.AudioFileTracks)));
        comboBoxAudioTrack.Bind(ComboBox.SelectedItemProperty, new Binding(nameof(vm.SelectedAudioFileTrack)));
        comboBoxAudioTrack.Bind(ComboBox.IsEnabledProperty, new Binding(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance });

        var panelAudioTrack = new StackPanel
        {
            Spacing = 3,
            Children = { labelAudioTrack, comboBoxAudioTrack }
        };
        panelAudioTrack.Bind(StackPanel.IsVisibleProperty, new Binding(nameof(vm.HasMultipleAudioTracks)));

        // 3. Input Subtitle Section (Optional)
        var labelSubtitle = UiUtil.MakeLabel(string.IsNullOrWhiteSpace(Se.Language.Video.RemuxVideoInputSubtitle) ? "Input subtitle (optional):" : Se.Language.Video.RemuxVideoInputSubtitle);
        labelSubtitle.FontWeight = FontWeight.SemiBold;

        var noticeText = string.IsNullOrWhiteSpace(Se.Language.Video.RemuxVideoSoftSubtitlesNotice)
            ? "Soft subtitles only (No burn-in / Not hardcoded)"
            : Se.Language.Video.RemuxVideoSoftSubtitlesNotice;

        var badgeSoftSub = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#1A2196F3")),
            BorderBrush = new SolidColorBrush(Color.Parse("#602196F3")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 1),
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = "⚡ " + noticeText,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.Parse("#2196F3")),
                FontWeight = FontWeight.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        var headerSubtitlePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                labelSubtitle,
                badgeSoftSub,
            },
        };

        var textBoxSubtitle = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.SubtitleFileName));
        textBoxSubtitle.HorizontalAlignment = HorizontalAlignment.Stretch;
        var labelSubtitleSize = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.SubtitleFileSize)).WithMarginRight(5);
        var buttonBrowseSubtitle = UiUtil.MakeButtonBrowse(vm.BrowseSubtitleCommand);
        buttonBrowseSubtitle.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance });

        var gridSubtitle = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
        };
        gridSubtitle.Add(textBoxSubtitle, 0, 0);
        gridSubtitle.Add(labelSubtitleSize, 0, 1);
        gridSubtitle.Add(buttonBrowseSubtitle, 0, 2);

        // 4. Output Format and File Section
        var labelFormat = UiUtil.MakeLabel(Se.Language.Video.RemuxVideoOutputFormat);
        var comboBoxFormat = UiUtil.MakeComboBox(vm.OutputFormats, vm, nameof(vm.SelectedOutputFormat));
        comboBoxFormat.Width = 100;
        comboBoxFormat.Bind(ComboBox.IsEnabledProperty, new Binding(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance });

        var panelFormat = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            Children =
            {
                labelFormat,
                comboBoxFormat,
            },
        };

        var labelOutputFile = UiUtil.MakeLabel(Se.Language.Video.RemuxVideoOutputFile);
        var textBoxOutput = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.OutputFileName));
        textBoxOutput.HorizontalAlignment = HorizontalAlignment.Stretch;
        var buttonBrowseOutput = UiUtil.MakeButtonBrowse(vm.BrowseOutputFileCommand);
        buttonBrowseOutput.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance });

        var gridOutput = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 5,
        };
        gridOutput.Add(textBoxOutput, 0, 0);
        gridOutput.Add(buttonBrowseOutput, 0, 1);

        // 5. Progress Section
        var progressBar = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Height = 16,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var labelProgress = UiUtil.MakeLabel(string.Empty)
            .WithBindText(vm, nameof(vm.ProgressText))
            .WithMarginTop(3);

        var progressPanel = new StackPanel
        {
            Spacing = 3,
            Children =
            {
                progressBar,
                labelProgress,
            },
        };

        // 6. Button Bar Section
        var buttonOpenFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenFolderCommand)
            .WithIconLeft(IconNames.FolderOpen)
            .WithMarginRight(5);
        buttonOpenFolder.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsCompleted)));

        var buttonPlay = UiUtil.MakeButton(Se.Language.General.Play, vm.PlayCommand)
            .WithIconLeft(IconNames.Play)
            .WithMarginRight(5);
        buttonPlay.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsCompleted)));

        var buttonRemux = UiUtil.MakeButton(Se.Language.Video.RemuxVideoTitle, vm.RemuxCommand)
            .WithMarginRight(5);
        buttonRemux.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.CanRemux)));

        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand)
            .WithMarginRight(5);
        buttonDone.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsRemuxing)) { Converter = InverseBooleanConverter.Instance });

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var buttonPanel = UiUtil.MakeButtonBar(
            buttonOpenFolder,
            buttonPlay,
            buttonRemux,
            buttonDone,
            buttonCancel
        );

        contentPanel.Children.Add(labelVideo);
        contentPanel.Children.Add(gridVideo);
        contentPanel.Children.Add(labelAudio);
        contentPanel.Children.Add(gridAudio);
        contentPanel.Children.Add(panelAudioTrack);
        contentPanel.Children.Add(headerSubtitlePanel);
        contentPanel.Children.Add(gridSubtitle);
        contentPanel.Children.Add(panelFormat);
        contentPanel.Children.Add(labelOutputFile);
        contentPanel.Children.Add(gridOutput);
        contentPanel.Children.Add(progressPanel);
        contentPanel.Children.Add(buttonPanel);

        Content = contentPanel;

        UiUtil.FocusOnFirstActivation(this, () => { _buttonBrowseVideo?.Focus(); });
    }
}
