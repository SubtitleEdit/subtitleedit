using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.EngineSettings;

public class SpeechToTextEngineSettingsWindow : Window
{
    private const int LabelWidth = 110;
    private const int ValueWidth = 360;

    private readonly SpeechToTextEngineSettingsViewModel _vm;

    public SpeechToTextEngineSettingsWindow(SpeechToTextEngineSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Speech-to-text engine settings";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 540;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        Content = BuildContent(vm);

        var ok = UiUtil.MakeButtonOk(vm.OkCommand);
        Activated += delegate { ok.Focus(); };
    }

    private Border BuildContent(SpeechToTextEngineSettingsViewModel vm)
    {
        var header = BuildHeader(vm);
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

    private static StackPanel BuildHeader(SpeechToTextEngineSettingsViewModel vm)
    {
        var title = new TextBlock
        {
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TitleText)),
        };

        var subtitle = new TextBlock
        {
            FontSize = 12,
            Opacity = 0.75,
            Margin = new Thickness(0, 2, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SubtitleText)),
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Children = { title, subtitle },
        };
    }

    private static Border BuildDetails(SpeechToTextEngineSettingsViewModel vm)
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
            },
            ColumnSpacing = 12,
            RowSpacing = 10,
        };

        // Backend
        grid.Add(MakeLabel("Backend"), 0, 0);
        grid.Add(MakeValue(nameof(vm.BackendLabel)), 0, 1);

        // Status (coloured dot + text)
        grid.Add(MakeLabel("Status"), 1, 0);
        var statusDot = new Ellipse
        {
            Width = 10,
            Height = 10,
            VerticalAlignment = VerticalAlignment.Center,
            [!Ellipse.FillProperty] = new Binding(nameof(vm.StatusBrush)),
        };
        var statusText = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.StatusLabel)),
        };
        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { statusDot, statusText },
        };
        grid.Add(statusPanel, 1, 1);

        // Install folder
        grid.Add(MakeLabel("Install folder"), 2, 0);
        var folderText = new TextBox
        {
            IsReadOnly = true,
            Width = ValueWidth,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 12,
            [!TextBox.TextProperty] = new Binding(nameof(vm.InstallFolder)),
        };
        grid.Add(folderText, 2, 1);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
        };
    }

    private static Grid BuildActions(SpeechToTextEngineSettingsViewModel vm)
    {
        var redownload = UiUtil.MakeButton("Re-download...", vm.RedownloadCommand).WithIconLeft(IconNames.Download);
        var openFolder = UiUtil.MakeButton("Open folder", vm.OpenFolderCommand).WithIconLeft(IconNames.FolderOpen);
        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);

        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { redownload, openFolder },
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

    private static TextBlock MakeValue(string bindingPath) => new()
    {
        FontWeight = FontWeight.SemiBold,
        VerticalAlignment = VerticalAlignment.Center,
        [!TextBlock.TextProperty] = new Binding(bindingPath),
    };

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
