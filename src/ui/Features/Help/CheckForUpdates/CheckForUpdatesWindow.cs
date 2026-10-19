using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Help.CheckForUpdates;

/// <summary>
/// Check for updates in the editor look (see <see cref="EditorChrome"/>): the status line with the
/// download/install actions on top, the changelog in a field frame under it, OK in the button bar.
/// </summary>
public class CheckForUpdatesWindow : Window
{
    public CheckForUpdatesWindow(CheckForUpdatesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Help.CheckForUpdates;
        Width = 700;
        Height = 550;
        MinWidth = 500;
        MinHeight = 400;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var statusIcon = new Optris.Icons.Avalonia.Icon
        {
            Value = "fa-solid fa-circle-arrow-up",
            FontSize = 18,
            Foreground = EditorChrome.AccentBrush,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var statusLabel = new TextBlock
        {
            FontSize = 13.5,
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
        };
        statusLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

        var downloadButton = EditorChrome.MakeTextToolButton(IconNames.Web, Se.Language.Help.CheckForUpdatesDownloadNewVersion, vm.OpenDownloadPageCommand);
        downloadButton.Foreground = EditorChrome.AccentBrush;
        downloadButton.VerticalAlignment = VerticalAlignment.Center;
        downloadButton.DataContext = vm;
        downloadButton.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsDownloadLinkVisible)));

        // In-app update (Windows installer, Linux tarball): download, verify, install after exit.
        var installButton = EditorChrome.MakeTextToolButton(IconNames.Download, Se.Language.Help.CheckForUpdatesInstallAndRestart, vm.InstallUpdateCommand);
        installButton.Foreground = EditorChrome.AccentBrush;
        installButton.VerticalAlignment = VerticalAlignment.Center;
        installButton.DataContext = vm;
        installButton.Bind(Button.IsVisibleProperty, new Binding(nameof(vm.IsInstallVisible)));

        var downloadProgress = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Width = 120,
            MinWidth = 0,
            ShowProgressText = true,
            ProgressTextFormat = "{0:0}%",
            VerticalAlignment = VerticalAlignment.Center,
            DataContext = vm,
        };
        downloadProgress.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.DownloadProgress)));
        downloadProgress.Bind(ProgressBar.IsVisibleProperty, new Binding(nameof(vm.IsDownloading)));

        var statusRow = new Grid
        {
            ColumnSpacing = 10,
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto,Auto"),
        };
        statusRow.Add(statusIcon, 0, 0);
        statusRow.Add(statusLabel, 0, 1);
        statusRow.Add(downloadProgress, 0, 2);
        statusRow.Add(installButton, 0, 3);
        statusRow.Add(downloadButton, 0, 4);
        var statusStrip = new Border
        {
            Child = statusRow,
            Padding = new Thickness(16, 12),
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 0, 0, 1),
        };

        var changeLogBox = new TextBox
        {
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            FontSize = 12.5,
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Padding = new Thickness(10, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            DataContext = vm,
        };
        changeLogBox.Classes.Add("text"); // keeps the field color while focused
        // With auto-hiding scrollbars (e.g. macOS "When Scrolling") the horizontal scrollbar
        // overlays the last line of the changelog; reserve its space like the subtitle grid does,
        // so the final line is always fully visible when scrolled to the bottom (#12972).
        ScrollViewer.SetAllowAutoHide(changeLogBox, false);
        changeLogBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.ChangeLogText)));
        changeLogBox.WithLabeledBy(statusLabel); // read-only changelog; the status line above is its only heading (#12087)

        var changeLogFrame = EditorChrome.MakeListFrame(changeLogBox, clearListBackground: true);
        changeLogFrame.Margin = new Thickness(14, 12, 14, 14);

        var sections = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
        };
        sections.Add(statusStrip, 0, 0);
        sections.Add(changeLogFrame, 1, 0);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonOk));

        Activated += async delegate
        {
            buttonOk.Focus();
            await vm.CheckForUpdates();
        };
        KeyDown += vm.OnKeyDown;
        Closed += delegate { vm.OnClosed(); };
    }
}
