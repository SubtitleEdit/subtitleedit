using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.AddAudioToVideo;

public class AddAudioToVideoWindow : Window
{
    private readonly AddAudioToVideoViewModel _vm;
    private Button? _buttonBrowseInput;

    public AddAudioToVideoWindow(AddAudioToVideoViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.IsNullOrWhiteSpace(Se.Language.Video.AddAudioToVideoTitle)
            ? "Add audio to video / audio"
            : Se.Language.Video.AddAudioToVideoTitle;
        MinWidth = 620;
        Width = 660;
        CanResize = true;
        SizeToContent = SizeToContent.Height;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var inputSection = MakeInputSection(vm);
        var audioSection = MakeAudioSection(vm);
        var optionsSection = MakeOptionsSection(vm);
        var outputSection = MakeOutputSection(vm);
        var progressSection = MakeProgressSection(vm);
        var buttonBar = MakeButtonBar(vm);

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = UiUtil.MakeWindowMargin(),
        };

        mainPanel.Children.Add(inputSection);
        mainPanel.Children.Add(audioSection);
        mainPanel.Children.Add(optionsSection);
        mainPanel.Children.Add(outputSection);
        mainPanel.Children.Add(progressSection);
        mainPanel.Children.Add(buttonBar);

        Content = mainPanel;

        UiUtil.FocusOnFirstActivation(this, () => { _buttonBrowseInput?.Focus(); });
    }

    private Control MakeInputSection(AddAudioToVideoViewModel vm)
    {
        var inputLabelText = string.IsNullOrWhiteSpace(Se.Language.Video.AddAudioInputMediaFile)
            ? "Input video / audio file:"
            : Se.Language.Video.AddAudioInputMediaFile;
        var label = UiUtil.MakeLabel(inputLabelText);
        var textBox = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.InputMediaFileName));
        textBox.IsReadOnly = true;
        textBox.Bind(TextBox.IsEnabledProperty, new Binding(nameof(vm.IsNotGenerating)));

        _buttonBrowseInput = UiUtil.MakeButtonBrowse(vm.BrowseInputMediaCommand, accessibleName: inputLabelText.TrimEnd(':'));
        _buttonBrowseInput.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsNotGenerating)));

        var labelInfo = UiUtil.MakeLabel(string.Empty)
            .WithBindText(vm, nameof(vm.MediaInfoText));
        labelInfo.Opacity = 0.75;
        labelInfo.FontSize = 11;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowSpacing = 4,
            ColumnSpacing = 6,
        };

        Grid.SetRow(label, 0);
        Grid.SetColumn(label, 0);
        Grid.SetColumnSpan(label, 2);

        Grid.SetRow(textBox, 1);
        Grid.SetColumn(textBox, 0);

        Grid.SetRow(_buttonBrowseInput, 1);
        Grid.SetColumn(_buttonBrowseInput, 1);

        Grid.SetRow(labelInfo, 2);
        Grid.SetColumn(labelInfo, 0);
        Grid.SetColumnSpan(labelInfo, 2);

        grid.Children.Add(label);
        grid.Children.Add(textBox);
        grid.Children.Add(_buttonBrowseInput);
        grid.Children.Add(labelInfo);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Control MakeAudioSection(AddAudioToVideoViewModel vm)
    {
        var audioLabelText = string.IsNullOrWhiteSpace(Se.Language.Video.AddAudioAudioFileToAdd)
            ? "Audio file to add:"
            : Se.Language.Video.AddAudioAudioFileToAdd;
        var label = UiUtil.MakeLabel(audioLabelText);
        var textBox = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.AudioFileName));
        textBox.IsReadOnly = true;
        textBox.Bind(TextBox.IsEnabledProperty, new Binding(nameof(vm.IsNotGenerating)));

        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.BrowseAudioCommand, accessibleName: audioLabelText.TrimEnd(':'));
        buttonBrowse.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsNotGenerating)));

        var labelInfo = UiUtil.MakeLabel(string.Empty)
            .WithBindText(vm, nameof(vm.AudioInfoText));
        labelInfo.Opacity = 0.75;
        labelInfo.FontSize = 11;

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowSpacing = 4,
            ColumnSpacing = 6,
        };

        Grid.SetRow(label, 0);
        Grid.SetColumn(label, 0);
        Grid.SetColumnSpan(label, 2);

        Grid.SetRow(textBox, 1);
        Grid.SetColumn(textBox, 0);

        Grid.SetRow(buttonBrowse, 1);
        Grid.SetColumn(buttonBrowse, 1);

        Grid.SetRow(labelInfo, 2);
        Grid.SetColumn(labelInfo, 0);
        Grid.SetColumnSpan(labelInfo, 2);

        grid.Children.Add(label);
        grid.Children.Add(textBox);
        grid.Children.Add(buttonBrowse);
        grid.Children.Add(labelInfo);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Control MakeOptionsSection(AddAudioToVideoViewModel vm)
    {
        var replaceText = string.IsNullOrWhiteSpace(Se.Language.Video.AddAudioReplaceOriginal)
            ? "Replace original audio"
            : Se.Language.Video.AddAudioReplaceOriginal;
        var checkBoxReplace = UiUtil.MakeCheckBox(replaceText, vm, nameof(vm.ReplaceOriginalAudio));
        checkBoxReplace.Bind(CheckBox.IsEnabledProperty, new Binding(nameof(vm.CanReplaceOriginalAudio)));
        checkBoxReplace.Bind(CheckBox.IsVisibleProperty, new Binding(nameof(vm.IsVideoInput)));

        var duckingText = string.IsNullOrWhiteSpace(Se.Language.Video.AddAudioDucking)
            ? "Audio ducking (reduce original volume)"
            : Se.Language.Video.AddAudioDucking;
        var checkBoxDucking = UiUtil.MakeCheckBox(duckingText, vm, nameof(vm.IsDuckingEnabled));
        checkBoxDucking.Bind(CheckBox.IsEnabledProperty, new Binding(nameof(vm.IsDuckingControlsEnabled)));

        var volumeLabelText = string.IsNullOrWhiteSpace(Se.Language.Video.AddAudioDuckingVolume)
            ? "Original volume %:"
            : Se.Language.Video.AddAudioDuckingVolume;
        var labelVolume = UiUtil.MakeLabel(volumeLabelText);
        var numVolume = UiUtil.MakeNumericUpDownInt(0, 200, 15, 120, vm, nameof(vm.DuckingVolumePercent));

        var panelVolume = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(24, 0, 0, 0),
        };
        panelVolume.Children.Add(labelVolume);
        panelVolume.Children.Add(numVolume);
        panelVolume.Bind(StackPanel.IsEnabledProperty, new Binding(nameof(vm.IsDuckingVolumeEnabled)));

        var addedVolumeLabelText = string.IsNullOrWhiteSpace(Se.Language.Video.AddAudioAddedVolume)
            ? "Added audio volume %:"
            : Se.Language.Video.AddAudioAddedVolume;
        var labelAddedVolume = UiUtil.MakeLabel(addedVolumeLabelText);
        var numAddedVolume = UiUtil.MakeNumericUpDownInt(0, 200, 100, 120, vm, nameof(vm.AddedAudioVolumePercent));
        numAddedVolume.Bind(NumericUpDown.IsEnabledProperty, new Binding(nameof(vm.IsNotGenerating)));

        var panelAddedVolume = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(24, 0, 0, 0),
        };
        panelAddedVolume.Children.Add(labelAddedVolume);
        panelAddedVolume.Children.Add(numAddedVolume);

        var stackOptions = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
        };

        stackOptions.Children.Add(checkBoxReplace);
        stackOptions.Children.Add(checkBoxDucking);
        stackOptions.Children.Add(panelVolume);
        stackOptions.Children.Add(panelAddedVolume);

        return UiUtil.MakeBorderForControl(stackOptions);
    }

    private static Control MakeOutputSection(AddAudioToVideoViewModel vm)
    {
        var outputLabelText = string.IsNullOrWhiteSpace(Se.Language.Video.AddAudioOutputFile)
            ? "Output file:"
            : Se.Language.Video.AddAudioOutputFile;
        var label = UiUtil.MakeLabel(outputLabelText);
        var textBox = UiUtil.MakeTextBox(double.NaN, vm, nameof(vm.OutputFileName));
        textBox.Bind(TextBox.IsEnabledProperty, new Binding(nameof(vm.IsNotGenerating)));

        var buttonBrowse = UiUtil.MakeButtonBrowse(vm.BrowseOutputFileCommand, accessibleName: outputLabelText.TrimEnd(':'));
        buttonBrowse.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsNotGenerating)));

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowSpacing = 4,
            ColumnSpacing = 6,
        };

        Grid.SetRow(label, 0);
        Grid.SetColumn(label, 0);
        Grid.SetColumnSpan(label, 2);

        Grid.SetRow(textBox, 1);
        Grid.SetColumn(textBox, 0);

        Grid.SetRow(buttonBrowse, 1);
        Grid.SetColumn(buttonBrowse, 1);

        grid.Children.Add(label);
        grid.Children.Add(textBox);
        grid.Children.Add(buttonBrowse);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Control MakeProgressSection(AddAudioToVideoViewModel vm)
    {
        var progressBar = UiUtil.MakeProgressBar();
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var textBlock = new TextBlock
        {
            Margin = new Thickness(0, 4, 0, 0),
        };
        textBlock.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
        };
        stack.Children.Add(progressBar);
        stack.Children.Add(textBlock);

        return stack;
    }

    private static Control MakeButtonBar(AddAudioToVideoViewModel vm)
    {
        var buttonGenerate = UiUtil.MakeButton(Se.Language.General.Generate, vm.GenerateCommand);
        buttonGenerate.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.CanGenerate)));
        buttonGenerate.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsNotGenerating)));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        buttonCancel.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsGenerating)));

        var buttonOpenFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenFolderCommand);
        buttonOpenFolder.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsCompleted)));

        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand);
        buttonDone.Bind(Button.IsEnabledProperty, new Binding(nameof(vm.IsNotGenerating)));

        return UiUtil.MakeButtonBar(buttonGenerate, buttonCancel, buttonOpenFolder, buttonDone);
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
