using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;

public sealed class DownloadTtsWindow : Window
{
    private readonly DownloadTtsViewModel _vm;

    public DownloadTtsWindow(DownloadTtsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "TTS - Download engine";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        vm.Window = this;
        DataContext = vm;
        _vm = vm;

        var titleText = new TextBlock
        {            
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.TitleText)),
        };

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.MinWidth = 400;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
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

        Activated += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        _vm.OnClosing();
    }
}