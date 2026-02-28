using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public class DownloadTesseractModelWindow : Window
{
    public DownloadTesseractModelWindow(DownloadTesseractModelViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Downloading Tesseract model";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var titleText = new TextBlock
        {
            Text = "Downloading Tesseract model",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };
        titleText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsProgressVisible)));

        var panelPickDictionary = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 8),
            Children =
            {
                new TextBlock
                {
                    Text = "Select Tesseract dictionary:",
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new ComboBox
                {
                    Margin = new Thickness(0, 0, 0, 0),
                    IsTextSearchEnabled = true,
                    ItemsSource = vm.TesseractDictionaryItems,
                    SelectedItem = vm.SelectedTesseractDictionaryItem
                }.WithBindSelected(nameof(vm.SelectedTesseractDictionaryItem)),
                UiUtil.MakeButton("Download", vm.DownloadCommand),
            }
        };
        panelPickDictionary.Bind(Panel.IsVisibleProperty, new Binding(nameof(vm.IsProgressVisible)) { Converter = new InverseBooleanConverter() });

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
                        new Setter(Thumb.IsVisibleProperty, false)
                    }
                },
                new Style(x => x.OfType<Track>())
                {
                    Setters =
                    {
                        new Setter(Track.HeightProperty, 6.0)
                    }
                },
            }
        };
        progressSlider.Bind(Slider.ValueProperty, new Binding(nameof(DownloadFfmpegViewModel.Progress)));
        progressSlider.Bind(Slider.IsVisibleProperty, new Binding(nameof(vm.IsProgressVisible)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(DownloadFfmpegViewModel.StatusText)));
        statusText.Bind(TextBlock.IsVisibleProperty, new Binding(nameof(vm.IsProgressVisible)));

        var buttonCancel = UiUtil.MakeButtonCancel(vm.CommandCancelCommand);
        var buttonBar = UiUtil.MakeButtonBar(buttonCancel);

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                titleText,
                panelPickDictionary,
                progressSlider,
                statusText,
                buttonBar,
            }
        };

        Activated += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        }; 
        KeyDown += (s, e) => vm.OnKeyDown(e);   
    }
}