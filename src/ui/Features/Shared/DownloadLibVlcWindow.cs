using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared;

public class DownloadLibVlcWindow : Window
{
    public DownloadLibVlcWindow(DownloadLibVlcViewModel vm)
    {
        vm.Window = this;
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = string.Format(Se.Language.General.DownloadingX,  "libVLC");
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
        progressBar.MinWidth = 450;
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