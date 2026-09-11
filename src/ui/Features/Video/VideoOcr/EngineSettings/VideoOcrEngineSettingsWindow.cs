using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.VideoOcr.EngineSettings;

public class VideoOcrEngineSettingsWindow : Window
{
    private const int LabelWidth = 150;
    private const int ValueWidth = 360;

    private readonly VideoOcrEngineSettingsViewModel _vm;

    public VideoOcrEngineSettingsWindow(VideoOcrEngineSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.VideoOcr.EngineSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 560;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 14,
            Children = { BuildHeader(vm), BuildDetails(vm), BuildActions(vm, buttonOk, buttonCancel) },
        };

        Content = new Border
        {
            Child = new Grid { Margin = UiUtil.MakeWindowMargin(), Children = { stack } },
            Padding = new Thickness(4),
        };

        UiUtil.FocusOnFirstActivation(this, buttonOk);
    }

    private static StackPanel BuildHeader(VideoOcrEngineSettingsViewModel vm)
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
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = LabelWidth + ValueWidth + 40,
            Margin = new Thickness(0, 2, 0, 0),
            [!TextBlock.TextProperty] = new Binding(nameof(vm.SubtitleText)),
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { title, subtitle },
        };
    }

    private static Border BuildDetails(VideoOcrEngineSettingsViewModel vm)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(LabelWidth, GridUnitType.Pixel) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            ColumnSpacing = 12,
            RowSpacing = 10,
        };

        for (var i = 0; i < 5; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        // Backend
        grid.Add(MakeLabel(Se.Language.General.Backend), 0, 0);
        grid.Add(MakeValue(nameof(vm.BackendLabel)), 0, 1);

        // Status (coloured dot + text)
        grid.Add(MakeLabel(Se.Language.General.Status), 1, 0);
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
        grid.Add(new StackPanel { Orientation = Orientation.Horizontal, Children = { statusDot, statusText } }, 1, 1);

        // Install folder - only for engines SE downloads
        grid.Add(MakeLabel(Se.Language.General.InstallFolder).WithBindIsVisible(nameof(vm.HasInstallFolder)), 2, 0);
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
        grid.Add(folderText.WithBindIsVisible(nameof(vm.HasInstallFolder)), 2, 1);

        // Request timeout - only for the server engines that have one
        grid.Add(MakeLabel(Se.Language.Ocr.LlamaCppOcrTimeoutMinutes).WithBindIsVisible(nameof(vm.HasTimeout)), 3, 0);
        grid.Add(UiUtil.MakeNumericUpDownInt(1, 120, 5, 140, vm, nameof(vm.TimeoutMinutes))
            .WithBindIsVisible(nameof(vm.HasTimeout)), 3, 1);

        // Website
        grid.Add(MakeLabel(Se.Language.Video.VideoOcr.Website).WithBindIsVisible(nameof(vm.HasWebsite)), 4, 0);
        var link = UiUtil.MakeLink(string.Empty, vm.OpenWebsiteCommand, vm, nameof(vm.WebsiteUrl));
        link.HorizontalAlignment = HorizontalAlignment.Left;
        grid.Add(link.WithBindIsVisible(nameof(vm.HasWebsite)), 4, 1);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
        };
    }

    private static Grid BuildActions(VideoOcrEngineSettingsViewModel vm, Button buttonOk, Button buttonCancel)
    {
        var redownload = UiUtil.MakeButton(string.Empty, vm.RedownloadCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.DownloadButtonLabel))
            .WithBindIsVisible(nameof(vm.CanRedownload));
        var openFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenFolderCommand)
            .WithIconLeft(IconNames.FolderOpen)
            .WithBindIsVisible(nameof(vm.HasInstallFolder));

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
        };
        grid.Add(new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { redownload, openFolder } }, 0, 0);
        grid.Add(UiUtil.MakeButtonBar(buttonOk, buttonCancel), 0, 2);
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
