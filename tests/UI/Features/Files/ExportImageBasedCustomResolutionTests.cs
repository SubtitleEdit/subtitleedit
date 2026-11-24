using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;

namespace UITests.Features.Files;

/// <summary>
/// Image based export has a "Custom resolution..." entry for a free width x height, e.g.
/// 1920x800 for a video with the black bars cropped (#17132).
/// </summary>
public class ExportImageBasedCustomResolutionTests
{
    [Fact]
    public void Resolutions_HaveCustomEntryRightAfterPickFromVideo()
    {
        var items = ResolutionItem.GetResolutions().ToList();

        Assert.False(items[0].IsCustom);
        Assert.Equal(0, items[0].Width);
        Assert.True(items[1].IsCustom);
        Assert.Equal(0, items[1].Width);
        Assert.Single(items, r => r.IsCustom);
        Assert.All(items.Skip(2), r => Assert.True(r.Width > 0 && r.Height > 0));
    }

    [Fact]
    public void CustomEntry_NeverMatchesASize()
    {
        var custom = ResolutionItem.GetResolutions().Single(r => r.IsCustom);

        Assert.False(custom.Matches(1920, 800, null));
    }

    [Theory]
    [InlineData(1920, 800, true)]
    [InlineData(3840, 1600, true)]
    [InlineData(16, 16, true)]
    [InlineData(15, 800, false)]
    [InlineData(1920, 16385, false)]
    public void Ok_OnlyAcceptsSizesInRange(int width, int height, bool expectedOk)
    {
        var vm = new CustomResolutionViewModel
        {
            ResolutionWidth = width,
            ResolutionHeight = height,
        };

        vm.OkCommand.Execute(null);

        Assert.Equal(expectedOk, vm.OkPressed);
        if (expectedOk)
        {
            Assert.Equal(width, vm.Width);
            Assert.Equal(height, vm.Height);
        }
    }

    [Fact]
    public void Ok_WithEmptyField_DoesNothing()
    {
        var vm = new CustomResolutionViewModel { ResolutionWidth = null };

        vm.OkCommand.Execute(null);

        Assert.False(vm.OkPressed);
    }

    [Fact]
    public void Initialize_KeepsTheCurrentSize_AndFallsBackForInvalidOnes()
    {
        var vm = new CustomResolutionViewModel();

        vm.Initialize(1920, 800);
        Assert.Equal(1920, vm.ResolutionWidth);
        Assert.Equal(800, vm.ResolutionHeight);

        vm.Initialize(0, 0);
        Assert.Equal(1920, vm.ResolutionWidth);
        Assert.Equal(1080, vm.ResolutionHeight);
    }

    [AvaloniaFact]
    public void Window_OkClosesWithTheEnteredSize()
    {
        var vm = new CustomResolutionViewModel();
        vm.Initialize(1920, 800);
        var window = new CustomResolutionWindow(vm);
        var closed = false;
        window.Closed += (_, _) => closed = true;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Save(window, "custom-resolution.png");

        vm.OkCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(closed);
        Assert.True(vm.OkPressed);
        Assert.Equal(1920, vm.Width);
        Assert.Equal(800, vm.Height);
    }

    private static void Save(Avalonia.Controls.Window window, string name)
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
