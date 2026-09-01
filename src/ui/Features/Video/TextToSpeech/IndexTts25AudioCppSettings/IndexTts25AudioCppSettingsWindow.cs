using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTts25AudioCppSettings;

public class IndexTts25AudioCppSettingsWindow : Window
{
    private const int LabelWidth = 150;
    private const int ValueWidth = 360;

    private readonly IndexTts25AudioCppSettingsViewModel _vm;

    public IndexTts25AudioCppSettingsWindow(IndexTts25AudioCppSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.IndexTts25SettingsTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 620;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        Content = BuildContent(vm);

        var ok = UiUtil.MakeButtonOk(vm.OkCommand);
        UiUtil.FocusOnFirstActivation(this, ok);
    }

    private Border BuildContent(IndexTts25AudioCppSettingsViewModel vm)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children = { BuildHeader(), BuildDetails(vm), BuildActions(vm) },
        };

        var outerGrid = new Grid { Margin = UiUtil.MakeWindowMargin() };
        outerGrid.Children.Add(stack);

        return new Border
        {
            Child = outerGrid,
            Padding = new Thickness(4),
        };
    }

    private static StackPanel BuildHeader()
    {
        var title = new TextBlock
        {
            Text = "IndexTTS 2.5 (audio.cpp)",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
        };

        var subtitle = new TextBlock
        {
            Text = new IndexTts25AudioCpp().Description,
            FontSize = 12,
            Opacity = 0.75,
            Margin = new Thickness(0, 2, 0, 0),
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Children = { title, subtitle },
        };
    }

    private static Border BuildDetails(IndexTts25AudioCppSettingsViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(LabelWidth, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // engine
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Q8_0
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // F16
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // voices
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // emotion
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // emotion strength
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // speed
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // folder
            },
            ColumnSpacing = 12,
            RowSpacing = 10,
        };

        grid.Add(MakeLabel(Se.Language.General.Engine), 0, 0);
        var enginePanel = MakeStatusPanel(nameof(vm.EngineBrush), nameof(vm.EngineLabel));
        var engineButton = UiUtil.MakeButton(string.Empty, vm.RedownloadEngineCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.EngineDownloadButtonText))
            .WithMarginLeft(12);
        enginePanel.Children.Add(engineButton);
        grid.Add(enginePanel, 0, 1);

        grid.Add(MakeLabel("Model " + IndexTts25AudioCpp.ModelKeyQ8_0), 1, 0);
        var q8Panel = MakeStatusPanel(nameof(vm.ModelQ8_0Brush), nameof(vm.ModelQ8_0Label));
        q8Panel.Children.Add(UiUtil.MakeButton(string.Empty, vm.DownloadModelCommand, IndexTts25AudioCpp.ModelKeyQ8_0)
            .WithIconLeft(IconNames.Download)
            .WithMarginLeft(12));
        grid.Add(q8Panel, 1, 1);

        grid.Add(MakeLabel("Model " + IndexTts25AudioCpp.ModelKeyF16), 2, 0);
        var f16Panel = MakeStatusPanel(nameof(vm.ModelF16Brush), nameof(vm.ModelF16Label));
        f16Panel.Children.Add(UiUtil.MakeButton(string.Empty, vm.DownloadModelCommand, IndexTts25AudioCpp.ModelKeyF16)
            .WithIconLeft(IconNames.Download)
            .WithMarginLeft(12));
        grid.Add(f16Panel, 2, 1);

        grid.Add(MakeLabel(Se.Language.Video.Voices), 3, 0);
        grid.Add(new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.VoicesLabel)),
        }, 3, 1);

        grid.Add(MakeLabel(Se.Language.Video.IndexTts25Emotion), 4, 0);
        var emotionCombo = new ComboBox
        {
            Width = 200,
            VerticalAlignment = VerticalAlignment.Center,
            [!ItemsControl.ItemsSourceProperty] = new Binding(nameof(vm.Emotions)),
            [!ComboBox.SelectedItemProperty] = new Binding(nameof(vm.SelectedEmotion)),
        };
        grid.Add(emotionCombo, 4, 1);

        grid.Add(MakeLabel(Se.Language.Video.IndexTts25EmotionStrength), 5, 0);
        grid.Add(MakeEmotionAlphaPanel(), 5, 1);

        grid.Add(MakeLabel(Se.Language.General.Speed), 6, 0);
        grid.Add(MakeSpeedPanel(), 6, 1);

        grid.Add(MakeLabel(Se.Language.General.InstallFolder), 7, 0);
        grid.Add(new TextBox
        {
            IsReadOnly = true,
            Width = ValueWidth,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 12,
            [!TextBox.TextProperty] = new Binding(nameof(vm.ModelsFolder)),
        }, 7, 1);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
        };
    }

    /// <summary>
    /// Speaking rate, presented the way users expect: right is faster. The engine converts
    /// this to IndexTTS-2.5's own `duration_factor`, which runs the other way (>1 = longer
    /// output = slower speech). Range capped at 0.5-2.0, which is what the model supports.
    /// </summary>
    private static StackPanel MakeSpeedPanel()
    {
        var slider = new Slider
        {
            Minimum = 0.5,
            Maximum = 2.0,
            Width = 220,
            TickFrequency = 0.05,
            IsSnapToTickEnabled = true,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(nameof(IndexTts25AudioCppSettingsViewModel.Speed)),
        };
        var label = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
            Width = 90,
            [!TextBlock.TextProperty] = new Binding(nameof(IndexTts25AudioCppSettingsViewModel.SpeedLabel)),
        };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { slider, label },
        };
    }

    /// <summary>
    /// Emotion blend strength. Disabled while the emotion is "None", since the model ignores
    /// it without an emotion vector — a live slider that changes nothing reads as a bug.
    /// </summary>
    private static StackPanel MakeEmotionAlphaPanel()
    {
        var slider = new Slider
        {
            Minimum = 0.0,
            Maximum = 1.0,
            Width = 220,
            TickFrequency = 0.05,
            IsSnapToTickEnabled = true,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(nameof(IndexTts25AudioCppSettingsViewModel.EmotionAlpha)),
            [!IsEnabledProperty] = new Binding(nameof(IndexTts25AudioCppSettingsViewModel.IsEmotionAlphaEnabled)),
        };
        var label = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
            Width = 150,
            [!TextBlock.TextProperty] = new Binding(nameof(IndexTts25AudioCppSettingsViewModel.EmotionAlphaLabel)),
            [!IsEnabledProperty] = new Binding(nameof(IndexTts25AudioCppSettingsViewModel.IsEmotionAlphaEnabled)),
        };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { slider, label },
        };
    }

    private static StackPanel MakeStatusPanel(string brushBindingPath, string labelBindingPath)
    {
        var dot = new Ellipse
        {
            Width = 10,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            [!Shape.FillProperty] = new Binding(brushBindingPath),
        };
        var text = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(labelBindingPath),
        };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { dot, text },
        };
    }

    private static Control BuildActions(IndexTts25AudioCppSettingsViewModel vm)
    {
        var openModelsFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenModelsFolderCommand).WithIconLeft(IconNames.FolderOpen);
        var openVoicesFolder = UiUtil.MakeButton(Se.Language.Video.Voices, vm.OpenVoicesFolderCommand).WithIconLeft(IconNames.FolderOpen);
        var close = UiUtil.MakeButtonOk(vm.OkCommand);

        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { openModelsFolder, openVoicesFolder },
        };

        var rightPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Children = { close },
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
        };
        grid.Add(leftPanel, 0, 0);
        grid.Add(rightPanel, 0, 2);
        return grid;
    }

    private static TextBlock MakeLabel(string text) => new()
    {
        Text = text,
        Opacity = 0.7,
        VerticalAlignment = VerticalAlignment.Center,
    };

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
