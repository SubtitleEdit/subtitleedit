using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

/// <summary>
/// Download progress for "Open from URL", in the editor look (see <see cref="EditorChrome"/>).
/// </summary>
public class DownloadVideoFromUrlWindow : Window
{
    public DownloadVideoFromUrlWindow(DownloadVideoFromUrlViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.OpenFromUrlDownloadingTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        Width = 480;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.VideoTool);

        DataContext = vm;

        var fileName = new TextBlock
        {
            FontSize = 12.5,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.FileNameDisplay)),
        };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.HorizontalAlignment = HorizontalAlignment.Stretch;
        progressBar.Margin = new Thickness(0, 2, 0, 0);
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.Progress)));
        // Post-processing reports nothing, so the bar animates instead of sitting frozen at 100%.
        progressBar.Bind(ProgressBar.IsIndeterminateProperty, new Binding(nameof(vm.IsProgressIndeterminate)));

        var statusText = EditorChrome.MakeBoundHint(nameof(vm.StatusText), 12);

        var errorText = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12.5,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Error)));
        errorText.Bind(IsVisibleProperty, new Binding(nameof(vm.Error))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)),
        });

        // The big "Downloading ..." title of the other download windows, not a small section header
        var titleText = EditorChrome.MakeDownloadTitle(Se.Language.Video.OpenFromUrlDownloadingTitle);
        var rows = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                titleText,
                fileName,
                progressBar,
                statusText,
                errorText,
            },
        };
        var section = new Border { Child = rows, Padding = new Thickness(20, 18) }; // like EditorChrome.MakeDownloadBody
        section.Background = EditorChrome.PanelBrush;
        section.Width = 480; // SizeToContent ignores the window Width: fixed here so the bar has room and long names wrap

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);

        var root = new Grid
        {
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
        };
        root.Add(section, 0, 0);
        root.Add(EditorChrome.MakeButtonBar(null, buttonCancel), 1, 0);
        Content = root;

        Loaded += delegate
        {
            buttonCancel.Focus();
            vm.StartDownload();
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
