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

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.EngineSettings;

public class SpeechToTextEngineSettingsWindow : Window
{
    private const int LabelWidth = 110;
    private const int ValueWidth = 360;

    private readonly SpeechToTextEngineSettingsViewModel _vm;

    public SpeechToTextEngineSettingsWindow(SpeechToTextEngineSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.AudioToText.EngineSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 540;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.SpeechToText);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var close = EditorChrome.AsPrimary(UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand));
        Content = BuildContent(vm, close);
    }

    private static StackPanel BuildContent(SpeechToTextEngineSettingsViewModel vm, Button close)
    {
        var header = BuildHeader(vm);
        header.Margin = new Thickness(16, 14, 16, 14);

        var details = new Border
        {
            Child = EditorChrome.MakeEditorSection(Se.Language.General.Information, BuildDetails(vm)),
            Background = EditorChrome.PanelBrush,
        };

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { header, details, BuildActions(vm, close) },
        };
    }

    private static StackPanel BuildHeader(SpeechToTextEngineSettingsViewModel vm)
    {
        var title = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(18),
            FontWeight = FontWeight.SemiBold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TitleText)),
        };

        var subtitle = new TextBlock
        {
            FontSize = UiUtil.ScaledFontSize(12),
            Foreground = EditorChrome.DimTextBrush,
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

    private static Grid BuildDetails(SpeechToTextEngineSettingsViewModel vm)
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
        var statusPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { statusDot, statusText },
        };
        grid.Add(statusPanel, 1, 1);

        // Install folder
        grid.Add(MakeLabel(Se.Language.General.InstallFolder), 2, 0);
        var folderText = new TextBox
        {
            IsReadOnly = true,
            Width = ValueWidth,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = UiUtil.ScaledFontSize(12),
            [!TextBox.TextProperty] = new Binding(nameof(vm.InstallFolder)),
        };
        AutomationProperties.SetName(folderText, Se.Language.General.InstallFolder);
        grid.Add(folderText, 2, 1);

        return grid;
    }

    /// <summary>
    /// Re-download and open folder on the left, Close on the right.
    /// </summary>
    private static Border BuildActions(SpeechToTextEngineSettingsViewModel vm, Button close)
    {
        var redownload = UiUtil.MakeButton(string.Empty, vm.RedownloadCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.DownloadButtonLabel));
        var openFolder = UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenFolderCommand).WithIconLeft(IconNames.FolderOpen);
        redownload.Margin = new Thickness(0);
        openFolder.Margin = new Thickness(0);

        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { redownload, openFolder },
        };

        return EditorChrome.MakeFlushButtonBar(leftPanel, close);
    }

    private static TextBlock MakeLabel(string text) => new()
    {
        Text = text,
        FontSize = 12,
        Foreground = EditorChrome.DimTextBrush,
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
