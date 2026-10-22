using Avalonia;
using Avalonia.Automation;
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

        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand).AsPrimary();
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var sections = new StackPanel { Background = EditorChrome.PanelBrush };
        sections.Children.Add(new Border { Child = BuildHeader(vm), Padding = new Thickness(14, 14, 14, 12) });
        sections.Children.Add(EditorChrome.MakeEditorSection(Se.Language.General.Information, BuildDetails(vm)));

        var root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
        };
        root.Add(sections, 0, 0);
        root.Add(EditorChrome.MakeButtonBar(BuildActions(vm), buttonCancel, buttonOk), 1, 0);
        Content = root;

        UiUtil.FocusOnFirstActivation(this, buttonOk);
    }

    private static StackPanel BuildHeader(VideoOcrEngineSettingsViewModel vm)
    {
        var title = new TextBlock
        {
            FontSize = EditorChrome.ScaledFont(16),
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TitleText)),
        };

        var subtitle = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
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

    private static Grid BuildDetails(VideoOcrEngineSettingsViewModel vm)
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
            FontSize = EditorChrome.FieldFontSize,
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
            FontSize = EditorChrome.FieldFontSize,
            [!TextBox.TextProperty] = new Binding(nameof(vm.InstallFolder)),
        };
        grid.Add(folderText.WithBindIsVisible(nameof(vm.HasInstallFolder)), 2, 1);

        // Request timeout - only for the server engines that have one
        grid.Add(MakeLabel(Se.Language.Ocr.LlamaCppOcrTimeoutMinutes).WithBindIsVisible(nameof(vm.HasTimeout)), 3, 0);
        var timeout = UiUtil.MakeNumericUpDownInt(1, 120, 5, 90, vm, nameof(vm.TimeoutMinutes));
        timeout.Classes.Add("field");
        AutomationProperties.SetName(timeout, Se.Language.Ocr.LlamaCppOcrTimeoutMinutes);
        grid.Add(timeout.WithBindIsVisible(nameof(vm.HasTimeout)), 3, 1);

        // Website
        grid.Add(MakeLabel(Se.Language.Video.VideoOcr.Website).WithBindIsVisible(nameof(vm.HasWebsite)), 4, 0);
        var link = UiUtil.MakeLink(string.Empty, vm.OpenWebsiteCommand, vm, nameof(vm.WebsiteUrl));
        link.HorizontalAlignment = HorizontalAlignment.Left;
        grid.Add(link.WithBindIsVisible(nameof(vm.HasWebsite)), 4, 1);

        return grid;
    }

    /// <summary>
    /// Re-download and open folder, as flat actions on the left of the button bar.
    /// </summary>
    private static StackPanel BuildActions(VideoOcrEngineSettingsViewModel vm)
    {
        var redownload = UiUtil.MakeButton(string.Empty, vm.RedownloadCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.DownloadButtonLabel))
            .WithBindIsVisible(nameof(vm.CanRedownload));
        var openFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenFolderCommand)
            .WithIconLeft(IconNames.FolderOpen)
            .WithBindIsVisible(nameof(vm.HasInstallFolder));
        foreach (var button in new[] { redownload, openFolder })
        {
            button.Classes.Add("tool");
            button.Height = 30;
            button.Padding = new Thickness(10, 0);
            button.FontSize = EditorChrome.FieldFontSize;
        }

        return new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4, Children = { redownload, openFolder } };
    }

    private static TextBlock MakeLabel(string text) => EditorChrome.MakeDimLabel(text, new Thickness(0));

    private static TextBlock MakeValue(string bindingPath) => new()
    {
        FontWeight = FontWeight.SemiBold,
        FontSize = EditorChrome.FieldFontSize,
        VerticalAlignment = VerticalAlignment.Center,
        [!TextBlock.TextProperty] = new Binding(bindingPath),
    };

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
