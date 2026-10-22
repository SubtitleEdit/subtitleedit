using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.KokoroTtsSettings;

/// <summary>
/// Kokoro TTS engine status, in the editor look (see <see cref="EditorChrome"/>): engine name and
/// description on top, an engine section (backend, status + download, release, install folder),
/// Close in the button bar.
/// </summary>
public class KokoroTtsSettingsWindow : Window
{
    private const double LabelWidth = 100;
    private const double ValueWidth = 340;

    private readonly KokoroTtsSettingsViewModel _vm;

    public KokoroTtsSettingsWindow(KokoroTtsSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TextToSpeech.KokoroTtsSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 520;
        Background = EditorChrome.WindowBrush;
        EditorChrome.AddEditorStyles(Styles);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var sections = new StackPanel { Background = EditorChrome.PanelBrush };
        sections.Children.Add(MakeHeader("Kokoro TTS", new KokoroTtsCpp().Description));
        sections.Children.Add(MakeEngineSection(vm, out var folderText));

        var root = new DockPanel();
        var buttonBar = MakeButtonBar(vm);
        DockPanel.SetDock(buttonBar, Dock.Bottom);
        root.Children.Add(buttonBar);
        root.Children.Add(sections);
        Content = root;

        // initial focus on an input, not an action button - a focused button clicks on bare Space;
        // a focused control also makes sure OnKeyDown fires
        UiUtil.FocusOnFirstActivation(this, folderText);
    }

    private static Border MakeHeader(string title, string description)
    {
        var panel = new StackPanel { Spacing = 2 };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = EditorChrome.ScaledFont(16),
            FontWeight = FontWeight.SemiBold,
        });
        panel.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 480,
            HorizontalAlignment = HorizontalAlignment.Left,
        });

        return new Border { Child = panel, Padding = new Thickness(14, 14, 14, 12) };
    }

    private static Border MakeEngineSection(KokoroTtsSettingsViewModel vm, out TextBox folderText)
    {
        var rows = new StackPanel { Spacing = 8 };

        rows.Children.Add(Row(Se.Language.General.Backend, new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.BackendLabel)),
        }));

        // Status: colored dot + text, the (re)download button beside it
        var redownload = UiUtil.MakeButton(string.Empty, vm.RedownloadCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.DownloadButtonLabel));
        var status = MakeStatusPanel(nameof(vm.StatusBrush), nameof(vm.StatusLabel));
        status.Children.Add(redownload);
        rows.Children.Add(Row(Se.Language.General.Status, status));

        rows.Children.Add(Row(Se.Language.General.Release, new TextBlock
        {
            FontFamily = new FontFamily("Cascadia Mono,Consolas,Menlo,Monaco,monospace"),
            FontSize = EditorChrome.FieldFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ReleaseTag)),
        }));

        rows.Children.Add(Row(Se.Language.General.InstallFolder, MakeFolderRow(nameof(vm.InstallFolder), vm.OpenFolderCommand, out folderText)));

        return EditorChrome.MakeEditorSection(Se.Language.General.Engine, rows);
    }

    /// <summary>
    /// A dim label of fixed width and the content after it, so the rows line up.
    /// </summary>
    private static Grid Row(string label, Control content)
    {
        var text = EditorChrome.MakeDimLabel(label, new Thickness(0));
        text.TextTrimming = TextTrimming.CharacterEllipsis;
        var grid = new Grid
        {
            ColumnSpacing = 10,
            MinHeight = 28,
            ColumnDefinitions = { new ColumnDefinition(new GridLength(LabelWidth)), new ColumnDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(text, 0, 0);
        grid.Add(content, 0, 1);
        return grid;
    }

    private static StackPanel MakeStatusPanel(string brushBindingPath, string labelBindingPath)
    {
        var dot = new Ellipse
        {
            Width = 9,
            Height = 9,
            VerticalAlignment = VerticalAlignment.Center,
            [!Ellipse.FillProperty] = new Binding(brushBindingPath),
        };
        var text = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            FontWeight = FontWeight.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0),
            [!TextBlock.TextProperty] = new Binding(labelBindingPath),
        };
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { dot, text },
        };
    }

    /// <summary>
    /// The read-only install folder with an "open folder" icon button after it.
    /// </summary>
    private static StackPanel MakeFolderRow(string folderBindingPath, System.Windows.Input.ICommand openFolderCommand, out TextBox folderText)
    {
        folderText = new TextBox
        {
            IsReadOnly = true,
            Width = ValueWidth,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 0),
            MinHeight = 28,
            Height = 28,
            VerticalAlignment = VerticalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = EditorChrome.FieldFontSize,
            [!TextBox.TextProperty] = new Binding(folderBindingPath),
        };
        AutomationProperties.SetName(folderText, Se.Language.General.InstallFolder);

        var openFolder = EditorChrome.CreateToolButton(IconNames.FolderOpen, Se.Language.General.OpenContainingFolder, openFolderCommand, 28);
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            Children = { folderText, openFolder },
        };
    }

    private static Border MakeButtonBar(KokoroTtsSettingsViewModel vm)
    {
        var close = UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand);
        close.Classes.Add("primary");

        return EditorChrome.WrapButtonBar(new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Children = { close },
        });
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
