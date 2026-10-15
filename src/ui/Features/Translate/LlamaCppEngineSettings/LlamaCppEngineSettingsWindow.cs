using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Chrome = Nikse.SubtitleEdit.Features.Ocr.OcrDialogChrome;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppEngineSettings;

/// <summary>
/// llama.cpp engine settings in the editor look (see <see cref="EditorChrome"/>), like the OCR
/// engine settings: backend, install status, release and folder, re-download and open folder in
/// the button bar.
/// </summary>
public class LlamaCppEngineSettingsWindow : Window
{
    private const int LabelWidth = 100;
    private const int ValueWidth = 380;

    private readonly LlamaCppEngineSettingsViewModel _vm;

    public LlamaCppEngineSettingsWindow(LlamaCppEngineSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.LlamaCppEngineSettings;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        MinWidth = 540;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        Chrome.Apply(this);

        var folderText = new TextBox
        {
            IsReadOnly = true,
            Width = ValueWidth,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(0),
            // Wrapped, so the whole path shows instead of being cut at the window edge
            TextWrapping = TextWrapping.Wrap,
            MinHeight = 0,
            VerticalContentAlignment = VerticalAlignment.Center,
            FontSize = 12.5,
            [!TextBox.TextProperty] = new Binding(nameof(vm.InstallFolder)),
        };
        AutomationProperties.SetName(folderText, Se.Language.General.InstallFolder);

        var releaseText = new TextBlock
        {
            FontFamily = new FontFamily("Cascadia Mono,Consolas,Menlo,Monaco,monospace"),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ReleaseTag)),
        };

        var rows = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                Chrome.Row(Se.Language.General.Backend, MakeValue(nameof(vm.BackendLabel)), LabelWidth),
                Chrome.Row(Se.Language.General.Status, Chrome.StatusPanel(nameof(vm.StatusBrush), nameof(vm.StatusLabel)), LabelWidth),
                Chrome.Row(Se.Language.General.Release, releaseText, LabelWidth),
                Chrome.Row(Se.Language.General.InstallFolder, folderText, LabelWidth, alignTop: true),
            },
        };

        var sections = new StackPanel
        {
            Children =
            {
                Chrome.MakeHeader("llama.cpp", Se.Language.General.LlamaCppEngineSettingsSubtitle),
                EditorChrome.MakeEditorSection(Se.Language.General.Engine, rows),
            },
        };

        var redownload = Chrome.Flat(UiUtil.MakeButton(string.Empty, vm.RedownloadCommand)
            .WithIconLeftBindText(IconNames.Download, nameof(vm.DownloadButtonLabel)));
        var openFolder = Chrome.Flat(UiUtil.MakeButton(Se.Language.General.OpenContainingFolder, vm.OpenFolderCommand).WithIconLeft(IconNames.FolderOpen));
        var close = Chrome.MakePrimary(UiUtil.MakeButton(Se.Language.General.Close, vm.OkCommand));

        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
            Children = { redownload, openFolder },
        };

        Content = Chrome.Root(sections, Chrome.ButtonBar(leftPanel, close));

        UiUtil.FocusOnFirstActivation(this, close);
    }

    private static TextBlock MakeValue(string bindingPath) => new()
    {
        FontSize = 12.5,
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
