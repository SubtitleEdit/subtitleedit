using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Ocr.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.CrispEmbedSettings;

public class CrispEmbedSettingsWindow : Window
{
    private const int LabelWidth = 110;

    public CrispEmbedSettingsWindow(CrispEmbedSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.CrispEmbedSettingsTitle;

        // Explicit width plus height-only auto-sizing: SizeToContent.WidthAndHeight makes this
        // window far too wide on macOS.
        Width = 800;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        vm.Window = this;
        DataContext = vm;

        var buttonClose = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            RowSpacing = 14,
            Margin = UiUtil.MakeWindowMargin(),
        };
        grid.Add(BuildHeader(), 0, 0);
        grid.Add(BuildEngineSection(vm), 1, 0);
        grid.Add(BuildModelsSection(vm), 2, 0);
        grid.Add(UiUtil.MakeButtonBar(buttonClose), 3, 0);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, buttonClose);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static StackPanel BuildHeader()
    {
        var title = new TextBlock
        {
            Text = CrispEmbedEngine.StaticName,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
        };

        var subtitle = new TextBlock
        {
            Text = Se.Language.Ocr.CrispEmbedDescription,
            FontSize = 12,
            Opacity = 0.75,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 2, 0, 0),
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Children = { title, subtitle },
        };
    }

    private static Border BuildEngineSection(CrispEmbedSettingsViewModel vm)
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
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 12,
            RowSpacing = 10,
        };

        var enginePanel = MakeStatusPanel(nameof(vm.EngineBrush), nameof(vm.EngineLabel));
        var engineButton = UiUtil.MakeButton(string.Empty, vm.RedownloadEngineCommand)
            .WithIconLeftBindText(IconNames.CloudDownload, nameof(vm.EngineDownloadButtonText))
            .WithMarginLeft(12);
        enginePanel.Children.Add(engineButton);

        grid.Add(MakeLabel(Se.Language.General.Engine), 0, 0);
        grid.Add(enginePanel, 0, 1);

        var folderText = new TextBox
        {
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TextBox.TextProperty] = new Binding(nameof(vm.InstallFolder)),
        };

        var folderPanel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            ColumnSpacing = 8,
        };
        folderPanel.Add(folderText, 0, 0);
        folderPanel.Add(
            UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenInstallFolderCommand)
                .WithIconLeft(IconNames.FolderOpen), 0, 1);

        grid.Add(MakeLabel(Se.Language.General.InstallFolder), 1, 0);
        grid.Add(folderPanel, 1, 1);

        return MakeSectionBorder(grid);
    }

    private static Border BuildModelsSection(CrispEmbedSettingsViewModel vm)
    {
        var header = new TextBlock
        {
            Text = Se.Language.General.Models,
            Opacity = 0.7,
            Margin = new Thickness(0, 0, 0, 8),
        };

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { header, MakeModelList(vm) },
        };

        return MakeSectionBorder(stack);
    }

    /// <summary>
    /// One row per downloadable model across all backends: backend, file name, size, install
    /// state, and its own download button. Everything is bound rather than read from the item
    /// passed to the template, as the template is reused across rows.
    /// </summary>
    private static ItemsControl MakeModelList(CrispEmbedSettingsViewModel vm)
    {
        return new ItemsControl
        {
            ItemsSource = vm.Models,
            ItemTemplate = new FuncDataTemplate<CrispEmbedModelStatusViewModel>((_, _) =>
            {
                var backend = new TextBlock
                {
                    Width = 120,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(CrispEmbedModelStatusViewModel.BackendName)),
                };
                var name = new TextBlock
                {
                    Width = 265,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    [!TextBlock.TextProperty] = new Binding(nameof(CrispEmbedModelStatusViewModel.ModelName)),
                };
                var size = new TextBlock
                {
                    Width = 70,
                    Opacity = 0.7,
                    VerticalAlignment = VerticalAlignment.Center,
                    [!TextBlock.TextProperty] = new Binding(nameof(CrispEmbedModelStatusViewModel.SizeText)),
                };
                var status = MakeStatusPanel(
                    nameof(CrispEmbedModelStatusViewModel.StatusBrush),
                    nameof(CrispEmbedModelStatusViewModel.StatusLabel));
                status.Width = 110;

                // Fixed width so the buttons line up down the column - "Download" and
                // "Re-download" are different lengths and the rows mix both.
                var button = UiUtil.MakeButton(string.Empty)
                    .WithIconLeft(IconNames.Download);
                button.Width = 140;
                button.Bind(ContentControl.ContentProperty,
                    new Binding(nameof(CrispEmbedModelStatusViewModel.DownloadButtonText)));
                button.Bind(Button.CommandProperty,
                    new Binding(nameof(CrispEmbedModelStatusViewModel.DownloadCommand)));

                return new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 8,
                    Margin = new Thickness(0, 0, 0, 6),
                    Children = { backend, name, size, status, button },
                };
            }, true),
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

    private static Border MakeSectionBorder(Control content) => new()
    {
        Child = content,
        Padding = new Thickness(14),
        CornerRadius = new CornerRadius(6),
        BorderThickness = new Thickness(1),
        BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x80, 0x80, 0x80)),
    };

    private static TextBlock MakeLabel(string text) => new()
    {
        Text = text,
        Opacity = 0.7,
        VerticalAlignment = VerticalAlignment.Center,
    };
}
