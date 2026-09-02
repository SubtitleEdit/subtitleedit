using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.AudioCppTtsSettings;

/// <summary>
/// Settings dialog shared by the audio.cpp engines that have no per-request knobs (Higgs
/// Audio v3, Fish Audio S2 Pro): runtime status with re-download, the two model quants,
/// imported-voices count, and folder shortcuts. Which engine it shows comes from the view
/// model's <see cref="AudioCppTtsSettingsAdapter"/>, handed to Initialize before this
/// constructor runs.
/// </summary>
public class AudioCppTtsSettingsWindow : Window
{
    private const int LabelWidth = 150;
    private const int ValueWidth = 360;

    private readonly AudioCppTtsSettingsViewModel _vm;

    public AudioCppTtsSettingsWindow(AudioCppTtsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Adapter.EngineName + " settings";
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

    private Border BuildContent(AudioCppTtsSettingsViewModel vm)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children = { BuildHeader(vm), BuildDetails(vm), BuildActions(vm) },
        };

        var outerGrid = new Grid { Margin = UiUtil.MakeWindowMargin() };
        outerGrid.Children.Add(stack);

        return new Border
        {
            Child = outerGrid,
            Padding = new Thickness(4),
        };
    }

    private static StackPanel BuildHeader(AudioCppTtsSettingsViewModel vm)
    {
        var title = new TextBlock
        {
            Text = vm.Adapter.EngineName,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
        };

        var subtitle = new TextBlock
        {
            Text = vm.Adapter.Description,
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

    private static Border BuildDetails(AudioCppTtsSettingsViewModel vm)
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // default model
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // alt model
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // voices
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

        grid.Add(MakeLabel("Model " + vm.Adapter.ModelKeyDefault), 1, 0);
        var defaultPanel = MakeStatusPanel(nameof(vm.ModelDefaultBrush), nameof(vm.ModelDefaultLabel));
        defaultPanel.Children.Add(UiUtil.MakeButton(string.Empty, vm.DownloadModelCommand, vm.Adapter.ModelKeyDefault)
            .WithIconLeft(IconNames.Download)
            .WithMarginLeft(12));
        grid.Add(defaultPanel, 1, 1);

        grid.Add(MakeLabel("Model " + vm.Adapter.ModelKeyAlt), 2, 0);
        var altPanel = MakeStatusPanel(nameof(vm.ModelAltBrush), nameof(vm.ModelAltLabel));
        altPanel.Children.Add(UiUtil.MakeButton(string.Empty, vm.DownloadModelCommand, vm.Adapter.ModelKeyAlt)
            .WithIconLeft(IconNames.Download)
            .WithMarginLeft(12));
        grid.Add(altPanel, 2, 1);

        grid.Add(MakeLabel(Se.Language.Video.Voices), 3, 0);
        grid.Add(new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.VoicesLabel)),
        }, 3, 1);

        grid.Add(MakeLabel(Se.Language.General.InstallFolder), 4, 0);
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
        }, 4, 1);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
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

    private static Control BuildActions(AudioCppTtsSettingsViewModel vm)
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
