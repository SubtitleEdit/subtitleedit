using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;

namespace UITests.Features.Video;

public class OpenFromUrlViewModelTests
{
    [Fact]
    public void ModeCommands_DisabledUntilUrlEntered()
    {
        var vm = new OpenFromUrlViewModel { DownloadVideo = true, Url = string.Empty };

        Assert.False(vm.OpenOnlineCommand.CanExecute(null));
        Assert.False(vm.DownloadAndOpenCommand.CanExecute(null));

        vm.Url = "   ";
        Assert.False(vm.OpenOnlineCommand.CanExecute(null));

        vm.Url = "https://youtu.be/l7tQ1v4TCeY";
        Assert.True(vm.OpenOnlineCommand.CanExecute(null));
        Assert.True(vm.DownloadAndOpenCommand.CanExecute(null));
    }

    [Fact]
    public void DownloadAndOpen_DisabledWhenNothingToDownload()
    {
        var vm = new OpenFromUrlViewModel { Url = "https://youtu.be/l7tQ1v4TCeY", DownloadVideo = false, DownloadSubtitles = false };
        var changed = 0;
        vm.DownloadAndOpenCommand.CanExecuteChanged += (_, _) => changed++;

        Assert.False(vm.DownloadAndOpenCommand.CanExecute(null));
        Assert.True(vm.OpenOnlineCommand.CanExecute(null));

        vm.DownloadSubtitles = true;
        Assert.True(vm.DownloadAndOpenCommand.CanExecute(null));
        Assert.True(changed > 0);
    }

    [AvaloniaFact]
    public void Window_CardsFollowUrl()
    {
        var vm = new OpenFromUrlViewModel { DownloadVideo = true, Url = string.Empty };
        var window = new OpenFromUrlWindow(vm);
        window.Show();
        try
        {
            var cards = window.GetLogicalDescendants().OfType<Button>().Where(b => b.Classes.Contains("card")).ToList();
            Assert.Equal(2, cards.Count);
            Assert.All(cards, c => Assert.False(c.IsEffectivelyEnabled));
            Shot(window, "open-from-url-empty.png");

            vm.Url = "https://youtu.be/l7tQ1v4TCeY";
            Dispatcher.UIThread.RunJobs();
            Assert.All(cards, c => Assert.True(c.IsEffectivelyEnabled));
            Shot(window, "open-from-url-filled.png");
        }
        finally
        {
            window.Close();
        }
    }

    private static void Shot(Window window, string name)
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
