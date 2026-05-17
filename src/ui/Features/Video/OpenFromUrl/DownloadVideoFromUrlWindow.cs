using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

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

        DataContext = vm;

        var heading = new TextBlock
        {
            Text = Se.Language.Video.OpenFromUrlDownloadingTitle,
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
        };

        var fileName = new TextBlock
        {
            Opacity = 0.75,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.FileNameDisplay)),
        };

        var progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            IsHitTestVisible = false,
            Focusable = false,
            MinWidth = 400,
            Margin = new Thickness(0, 4, 0, 0),
            Styles =
            {
                new Style(x => x.OfType<Thumb>())
                {
                    Setters = { new Setter(Thumb.IsVisibleProperty, false) }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters = { new Setter(Track.HeightProperty, 6.0) }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(vm.Progress)));

        var statusText = new TextBlock { Opacity = 0.85 };
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

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

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonCancel);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                heading,
                fileName,
                progressSlider,
                statusText,
                errorText,
                buttonBar,
            },
        };

        Loaded += delegate
        {
            buttonCancel.Focus();
            vm.StartDownload();
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
