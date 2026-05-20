using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public class DownloadPaddleOcrWindow : Window
{
    public DownloadPaddleOcrWindow(DownloadPaddleOcrViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Downloading Paddle OCR";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var titleText = new TextBlock
        {
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        }.WithBindText(vm, nameof(vm.StatusText));

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.MinWidth = 400;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));
        progressBar.Bind(ProgressBar.OpacityProperty, new Binding(nameof(vm.ProgressOpacity)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonCancel);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                titleText,
                progressBar,
                statusText,
                buttonBar,
            }
        };

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
            vm.StartDownload();
        }; 
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}