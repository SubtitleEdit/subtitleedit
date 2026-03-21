using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

public class TextToSpeechWindow : Window
{
    private readonly TextToSpeechViewModel _vm;

    public TextToSpeechWindow(TextToSpeechViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelEngine = new Label
        {
            Content = Se.Language.Video.TextToSpeech.TextToSpeechEngine,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var labelSettings = new Label
        {
            Content = Se.Language.General.Settings,
            VerticalAlignment = VerticalAlignment.Bottom,
        };

        var engineLayout = MakeEngineControls(vm);

        var settingsLayout = MakeSettingsControls(vm);

        var progressBarLayout = MakeProgressBarControls(vm);

        var buttonDone = UiUtil.MakeButtonDone(vm.DoneCommand).WithBindIsVisible(nameof(vm.IsNotGenerating));
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand).WithBindIsVisible(nameof(vm.IsGenerating));
        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButton(Se.Language.Video.TextToSpeech.GenerateSpeechFromText, vm.GenerateTtsCommand).WithBindIsEnabled(nameof(vm.IsNotGenerating)),
            UiUtil.MakeButton(Se.Language.General.ImportDotDotDot, vm.ImportCommand).WithBindIsEnabled(nameof(vm.IsNotGenerating)),
            buttonCancel,
            buttonDone
        ).WithMarginTop(0);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 0,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelEngine, 0, 0);
        grid.Add(labelSettings, 0, 1);

        grid.Add(engineLayout, 1, 0);
        grid.Add(settingsLayout, 1, 1);

        grid.Add(progressBarLayout, 2, 0, 1, 2);

        grid.Add(buttonPanel, 3, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonDone.Focus(); }; // hack to make OnKeyDown work
    }

    private static Border MakeEngineControls(TextToSpeechViewModel vm)
    {
        var labelMinWidth = 100;
        var controlMinWidth = 200;

        var comboBoxEngines = UiUtil.MakeComboBox(vm.Engines, vm, nameof(vm.SelectedEngine)).WithWidth(controlMinWidth);
        comboBoxEngines.SelectionChanged += vm.SelectedEngineChanged;

        var panelEngine = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Engine,
                    MinWidth = labelMinWidth,
                },
                comboBoxEngines,
                UiUtil.MakeButton(vm.ShowEngineSettingsCommand, IconNames.Settings)
                    .WithMarginLeft(5)
                    .WithBindIsVisible(nameof(vm.IsEngineSettingsVisible)),
            }
        };

        var buttonTestVoice = UiUtil.MakeButton(Se.Language.Video.TextToSpeech.TestVoice, vm.TestVoiceCommand).WithBindIsEnabled(nameof(vm.IsVoiceTestEnabled));
        var panelVoice = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Voice,
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Voices, vm, nameof(vm.SelectedVoice)).WithWidth(controlMinWidth),
                new Label
                {
                    [!Label.ContentProperty] = new Binding(nameof(vm.VoiceCountInfo)) { Mode = BindingMode.TwoWay }
                },
                buttonTestVoice,
                UiUtil.MakeButton(vm.ShowTestVoiceSettingsCommand, IconNames.Settings),
            }
        };

        var comboBoxModels = UiUtil.MakeComboBox(vm.Models, vm, nameof(vm.SelectedModel)).WithWidth(controlMinWidth);
        var panelModel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Model,
                    MinWidth = labelMinWidth,
                },
                comboBoxModels,
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasModel)) { Mode = BindingMode.OneWay },
        };
        comboBoxModels.SelectionChanged += vm.SelectedModelChanged;

        var panelRegion = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Region,
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeComboBox(vm.Regions, vm, nameof(vm.SelectedRegion)).WithWidth(controlMinWidth),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasRegion)) { Mode = BindingMode.OneWay },
        };

        var comboBoxLanguages = UiUtil.MakeComboBox(vm.Languages, vm, nameof(vm.SelectedLanguage)).WithWidth(controlMinWidth);
        var panelLanguage = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.Language,
                    MinWidth = labelMinWidth,
                },
                comboBoxLanguages,
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasLanguageParameter)) { Mode = BindingMode.OneWay },
        };
        comboBoxLanguages.SelectionChanged += vm.SelectedLanguageChanged;

        var panelApiKey = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.ApiKey,
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeTextBox(325, vm, nameof(vm.ApiKey)),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasApiKey)) { Mode = BindingMode.OneWay },
        };

        var panelKeyFile = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 10, 0, 0),
            Children =
            {
                new Label
                {
                    Content = Se.Language.General.KeyFile,
                    MinWidth = labelMinWidth,
                },
                UiUtil.MakeTextBox(325, vm, nameof(vm.KeyFile)).WithMarginRight(4),
                UiUtil.MakeButtonBrowse(vm.BrowseKeyFileCommand),
            },
            [!StackPanel.IsVisibleProperty] = new Binding(nameof(vm.HasKeyFile)) { Mode = BindingMode.OneWay },
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 15),
        };

        grid.Add(panelEngine, 0, 0);
        grid.Add(panelVoice, 1, 0);
        grid.Add(panelModel, 2, 0);
        grid.Add(panelRegion, 3, 0);
        grid.Add(panelLanguage, 4, 0);
        grid.Add(panelApiKey, 5, 0);
        grid.Add(panelKeyFile, 6, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Border MakeSettingsControls(TextToSpeechViewModel vm)
    {
        var checkBoxReviewAudioClips = new CheckBox
        {
            Content = Se.Language.Video.TextToSpeech.ReviewAudioSegments,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 10, 0, 10),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DoReviewAudioClips)) { Mode = BindingMode.TwoWay }
        };

        var checkBoxAddAudioToVideoFile = new CheckBox
        {
            Content = Se.Language.Video.TextToSpeech.AddAudioToVideoFile,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 10),
            [!CheckBox.IsCheckedProperty] = new Binding(nameof(vm.DoGenerateVideoFile)) { Mode = BindingMode.TwoWay }
        };

        var panelAddAudioToVideoFile = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Top,
            Children =
            {
                checkBoxAddAudioToVideoFile,
                UiUtil.MakeButton(vm.ShowEncodingSettingsCommand, IconNames.Settings)
                      .WithMarginLeft(5).WithMarginTop(0).WithTopAlignment(),
            }
        };

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(checkBoxReviewAudioClips, 0, 0);
        grid.Add(panelAddAudioToVideoFile, 1, 0);

        return UiUtil.MakeBorderForControl(grid);
    }

    private static Grid MakeProgressBarControls(TextToSpeechViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 0, 0, 20),
            [!Grid.OpacityProperty] = new Binding(nameof(vm.ProgressOpacity)) { Mode = BindingMode.OneWay },
        };

        var label = new Label
        {
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 20, 0, 0),
            [!Label.ContentProperty] = new Binding(nameof(vm.ProgressText)) { Mode = BindingMode.TwoWay },
        };

        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        grid.Add(label, 0, 0);
        grid.Add(progressSlider, 0, 0);

        return grid;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing(e);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _vm.OnLoaded(e);
    }
}
