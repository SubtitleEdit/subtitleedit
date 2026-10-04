using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

public class DownloadFfmpegWindow : Window
{
    public DownloadFfmpegWindow(DownloadFfmpegViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX, "ffmpeg");
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = string.Format(Se.Language.General.DownloadingX, "ffmpeg"),
            FontSize = UiUtil.ScaledFontSize(20),
            FontWeight = FontWeight.Bold,
        };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.MinWidth = 400;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(DownloadFfmpegViewModel.Progress)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(DownloadFfmpegViewModel.StatusText)));

        // Download/unpack failures (no data, ffmpeg in use, ...) - otherwise the window just sits there.
        var errorText = DownloadWindowUi.MakeErrorText();

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonRetry = DownloadWindowUi.MakeButtonRetry(vm.RetryCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonRetry, buttonCancel);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                titleText,
                progressBar,
                statusText,
                errorText,
                buttonBar,
            }
        };

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            vm.StartDownload();
        };
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}