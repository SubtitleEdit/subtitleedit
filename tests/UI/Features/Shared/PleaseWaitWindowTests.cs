using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Shared;

/// <summary>
/// The "Please wait" window can offer Cancel - used while an "Open online" stream loads (#17001).
/// </summary>
public class PleaseWaitWindowTests
{
    [AvaloniaFact]
    public void CancelButton_HiddenByDefault()
    {
        var vm = new PleaseWaitViewModel();
        var window = new PleaseWaitWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.False(FindCancelButton(window).IsVisible);
        Save(window, "please-wait.png");
        window.Close();
    }

    [AvaloniaFact]
    public void CancelButton_WhenEnabled_SetsIsCancelledAndCloses()
    {
        var vm = new PleaseWaitViewModel { StatusText = Se.Language.Video.OpenFromUrlLoadingVideo, CanCancel = true };
        var window = new PleaseWaitWindow(vm);
        var closed = false;
        window.Closed += (_, _) => closed = true;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var button = FindCancelButton(window);
        Assert.True(button.IsVisible);
        Save(window, "please-wait-cancel.png");

        Assert.False(vm.IsCancelled);
        button.Command!.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(vm.IsCancelled);
        Assert.True(closed);
    }

    private static Button FindCancelButton(Window window) =>
        window.GetLogicalDescendants().OfType<Button>().Single();

    private static void Save(Window window, string name)
    {
        var dir = Environment.GetEnvironmentVariable("SHOT_DIR");
        if (string.IsNullOrEmpty(dir))
        {
            return;
        }

        for (var i = 0; i < 5; i++)
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }

        window.CaptureRenderedFrame()!.Save(Path.Combine(dir, name), PngBitmapEncoderOptions.Default);
    }
}
