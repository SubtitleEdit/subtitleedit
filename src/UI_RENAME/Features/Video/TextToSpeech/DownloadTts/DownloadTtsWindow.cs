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

        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            MinWidth = 400,
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters =
                    {
                        new Setter(IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(RangeBase.ValueProperty, new Binding(nameof(vm.ProgressValue)));

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
                progressSlider,
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
}