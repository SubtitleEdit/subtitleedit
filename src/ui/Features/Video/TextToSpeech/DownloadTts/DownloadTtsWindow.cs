using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Data;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;

public sealed class DownloadTtsWindow : Window
{
    private readonly DownloadTtsViewModel _vm;

    public DownloadTtsWindow(DownloadTtsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Video.TtsDownloadEngineTitle;
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

        // The view models set Error on every failure path, but nothing rendered it - the user saw
        // only the generic "Download failed" while the real cause was written to a property no
        // window bound. DownloadVideoFromUrlWindow and DownloadCrispEmbedWindow show it this way.
        var errorText = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B)),
            TextWrapping = TextWrapping.Wrap,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Error)));
        errorText.Bind(IsVisibleProperty, new Binding(nameof(vm.Error))
        {
            Converter = new Avalonia.Data.Converters.FuncValueConverter<string?, bool>(s => !string.IsNullOrEmpty(s)),
        });

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
                errorText,
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