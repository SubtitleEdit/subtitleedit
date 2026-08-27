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

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DotsTtsCrispAsrSettings;

public class DotsTtsCrispAsrSettingsWindow : Window
{
    private const int LabelWidth = 150;
    private const int ValueWidth = 360;

    private readonly DotsTtsCrispAsrSettingsViewModel _vm;

    public DotsTtsCrispAsrSettingsWindow(DotsTtsCrispAsrSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.Video.TtsCrispAsrSettingsTitle, "dots.tts");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 580;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        Content = BuildContent(vm);

        var ok = UiUtil.MakeButtonOk(vm.OkCommand);
        Activated += delegate { ok.Focus(); };
    }

    private Border BuildContent(DotsTtsCrispAsrSettingsViewModel vm)
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
            Text = "dots.tts (CrispASR)",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
        };

        var subtitle = new TextBlock
        {
            Text = new DotsTtsCrispAsr().Description,
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

    private static Border BuildDetails(DotsTtsCrispAsrSettingsViewModel vm)
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

        grid.Add(MakeLabel("Model " + DotsTtsCrispAsr.ModelKeyQ4K), 1, 0);
        grid.Add(MakeStatusPanel(nameof(vm.CoreQ4KBrush), nameof(vm.CoreQ4KLabel)), 1, 1);

        grid.Add(MakeLabel("Model " + DotsTtsCrispAsr.ModelKeyQ8_0), 2, 0);
        grid.Add(MakeStatusPanel(nameof(vm.CoreQ8_0Brush), nameof(vm.CoreQ8_0Label)), 2, 1);

        grid.Add(MakeLabel("Model " + DotsTtsCrispAsr.ModelKeyF16), 3, 0);
        grid.Add(MakeStatusPanel(nameof(vm.CoreF16Brush), nameof(vm.CoreF16Label)), 3, 1);

        grid.Add(MakeLabel("BigVGAN vocoder"), 4, 0);
        grid.Add(MakeStatusPanel(nameof(vm.VocoderBrush), nameof(vm.VocoderLabel)), 4, 1);

        grid.Add(MakeLabel("CAM++ speaker encoder"), 5, 0);
        grid.Add(MakeStatusPanel(nameof(vm.SpeakerEncoderBrush), nameof(vm.SpeakerEncoderLabel)), 5, 1);

        grid.Add(MakeLabel(Se.Language.Video.Voices), 6, 0);
        var voicesText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.VoicesLabel)),
        };
        grid.Add(voicesText, 6, 1);

        grid.Add(MakeLabel("Quality"), 7, 0);
        grid.Add(MakeOdeStepsPanel(), 7, 1);

        grid.Add(MakeLabel(Se.Language.General.InstallFolder), 8, 0);
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
        grid.Add(folderText, 8, 1);

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
    /// Flow-matching Euler steps: the one real quality/speed knob dots.tts exposes. Measured on
    /// M4 Metal with the F16 core, 4 steps synthesised in 25 s where 16 steps took 38 s for a
    /// comparable line, so this is worth surfacing rather than hard-coding.
    /// </summary>
    private static StackPanel MakeOdeStepsPanel()
    {
        var slider = new Slider
        {
            Minimum = DotsTtsCrispAsr.MinOdeSteps,
            Maximum = DotsTtsCrispAsr.MaxOdeSteps,
            Width = 220,
            TickFrequency = 1,
            IsSnapToTickEnabled = true,
            VerticalAlignment = VerticalAlignment.Center,
            [!Slider.ValueProperty] = new Binding(nameof(DotsTtsCrispAsrSettingsViewModel.OdeSteps)),
        };
        var label = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(8, 0, 0, 0),
            Width = 80,
            [!TextBlock.TextProperty] = new Binding(nameof(DotsTtsCrispAsrSettingsViewModel.OdeStepsLabel)),
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

    private static Grid BuildActions(DotsTtsCrispAsrSettingsViewModel vm)
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
