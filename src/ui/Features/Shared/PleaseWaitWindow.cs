using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Shared;

public class PleaseWaitWindow : Window
{
    public PleaseWaitWindow(PleaseWaitViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Empty;

        // Fix the width and only auto-size the height. SizeToContent.WidthAndHeight
        // renders far too wide on macOS, so keep an explicit width for a consistent
        // look across platforms.
        Width = 360;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        ShowInTaskbar = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        DataContext = vm;

        var titleText = new TextBlock
        {
            FontSize = 18,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8),
        };
        titleText.Bind(TextBlock.TextProperty, new Binding(nameof(vm.StatusText)));

        var progressBar = new ProgressBar
        {
            IsIndeterminate = true,
            Minimum = 0,
            Maximum = 100,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Height = 8,
        };
        progressBar.Bind(ProgressBar.IsIndeterminateProperty, new Binding(nameof(vm.IsIndeterminate)));
        progressBar.Bind(ProgressBar.ValueProperty, new Binding(nameof(vm.ProgressValue)));

        Content = new StackPanel
        {
            Spacing = 8,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                titleText,
                progressBar,
            }
        };
    }
}
