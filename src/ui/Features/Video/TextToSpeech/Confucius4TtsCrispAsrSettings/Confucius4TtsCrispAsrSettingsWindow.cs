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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Confucius4TtsCrispAsrSettings;

public class Confucius4TtsCrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 150;
    private const int ValueWidth = 360;

    private readonly Confucius4TtsCrispAsrSettingsViewModel _vm;

    public Confucius4TtsCrispAsrSettingsWindow(Confucius4TtsCrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "Confucius4-TTS");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 580;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        Content = BuildContent(vm);

        var ok = UiUtil.MakeButtonOk(vm.OkCommand);
        UiUtil.FocusOnFirstActivation(this, ok);
    }

    private Border BuildContent(Confucius4TtsCrispAsrSettingsViewModel vm)
    {
        var header = BuildHeader();
        var details = BuildDetails(vm);
        var actions = BuildActions(vm);

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children = { header, details, actions },
        };

        var outerGrid = new Grid
        {
            Margin = UiUtil.MakeWindowMargin(),
        };
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
            Text = "Confucius4-TTS (CrispASR)",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
        };

        var subtitle = new TextBlock
        {
            Text = new Confucius4TtsCrispAsr().Description,
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

    private static Border BuildDetails(Confucius4TtsCrispAsrSettingsViewModel vm)
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
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

        grid.Add(MakeLabel("Model " + Confucius4TtsCrispAsr.ModelKeyQ8_0), 1, 0);
        grid.Add(MakeStatusPanel(nameof(vm.CoreQ8_0Brush), nameof(vm.CoreQ8_0Label)), 1, 1);

        grid.Add(MakeLabel("Model " + Confucius4TtsCrispAsr.ModelKeyF16), 2, 0);
        grid.Add(MakeStatusPanel(nameof(vm.CoreF16Brush), nameof(vm.CoreF16Label)), 2, 1);

        grid.Add(MakeLabel("BigVGAN vocoder"), 3, 0);
        grid.Add(MakeStatusPanel(nameof(vm.VocoderBrush), nameof(vm.VocoderLabel)), 3, 1);

        grid.Add(MakeLabel("w2v-BERT encoder"), 4, 0);
        grid.Add(MakeStatusPanel(nameof(vm.W2vBrush), nameof(vm.W2vLabel)), 4, 1);

        grid.Add(MakeLabel(Se.Language.Video.Voices), 5, 0);
        var voicesText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.VoicesLabel)),
        };
        grid.Add(voicesText, 5, 1);

        grid.Add(MakeLabel("Quality"), 6, 0);
        grid.Add(MakeOdeStepsPanel(), 6, 1);

        grid.Add(MakeLabel(Se.Language.General.InstallFolder), 7, 0);
        var folderText = new TextBox
        {
            IsReadOnly = true,
            Width = ValueWidth,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 12,
            [!TextBox.TextProperty] = new Binding(nameof(vm.ModelsFolder)),
        };
        grid.Add(folderText, 7, 1);

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
    /// Flow-matching Euler steps for the S2A stage — the quality/speed knob this backend exposes
    /// (--tts-steps; CrispASR default 20, upstream reference samples at 25).
    /// </summary>
    private static StackPanel MakeOdeStepsPanel()
    {
        var slider = new Slider
        {
            Minimum = Confucius4TtsCrispAsr.MinOdeSteps,
            Maximum = Confucius4TtsCrispAsr.MaxOdeSteps,
            Width = 220,
            TickFrequency = 1,
            IsSnapToTickEnabled = true,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(nameof(Confucius4TtsCrispAsrSettingsViewModel.OdeSteps)),
        };
        var label = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
            Width = 80,
            [!TextBlock.TextProperty] = new Binding(nameof(Confucius4TtsCrispAsrSettingsViewModel.OdeStepsLabel)),
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
            [!Ellipse.FillProperty] = new Binding(brushBindingPath),
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

    private static Grid BuildActions(Confucius4TtsCrispAsrSettingsViewModel vm)
    {
        var openModelsFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenModelsFolderCommand).WithIconLeft(IconNames.FolderOpen);
        var openVoicesFolder = UiUtil.MakeButton("Voices folder", vm.OpenVoicesFolderCommand).WithIconLeft(IconNames.FolderOpen);
        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);

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
