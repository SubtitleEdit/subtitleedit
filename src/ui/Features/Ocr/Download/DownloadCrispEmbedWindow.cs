using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public class DownloadCrispEmbedWindow : Window
{
    public DownloadCrispEmbedWindow(DownloadCrispEmbedViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Downloading CrispEmbed";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var titleText = new TextBlock
        {
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        }.WithBindText(vm, nameof(vm.TitleText));

        var progressBar = UiUtil.MakeProgressBar();
        progressBar.MinWidth = 400;
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        var statusText = new TextBlock();
        statusText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.ProgressText)));

        var errorText = new TextBlock
        {
            Foreground = Brushes.Red,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400,
        };
        errorText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.Error)));

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
                errorText,
                buttonBar,
            }
        };

        Loaded += delegate
        {
            buttonCancel.Focus(); // hack to make OnKeyDown work
        };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
