using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.Bookmarks;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Bookmarks;

/// <summary>
/// The bookmarks list filters by note or subtitle text, ignoring case.
/// </summary>
public class BookmarksListFilterTests
{
    private static List<SubtitleLineViewModel> Lines() => new()
    {
        new() { Number = 1, Text = "Hello there", StartTime = TimeSpan.FromSeconds(1), EndTime = TimeSpan.FromSeconds(2), Bookmark = "Ask the EXPERT\nsecond line" },
        new() { Number = 2, Text = "Not bookmarked", StartTime = TimeSpan.FromSeconds(3), EndTime = TimeSpan.FromSeconds(4) },
        new() { Number = 3, Text = "The expert says no", StartTime = TimeSpan.FromSeconds(5), EndTime = TimeSpan.FromSeconds(6), Bookmark = "", BookmarkColor = 1 },
        new() { Number = 4, Text = "Goodbye", StartTime = TimeSpan.FromSeconds(7), EndTime = TimeSpan.FromSeconds(8), Bookmark = "My own note", BookmarkColor = 2 },
    };

    [Fact]
    public void Filter_MatchesNoteOrText_IgnoringCase()
    {
        var vm = new BookmarksListViewModel(new FileHelper());
        vm.Initialize(Lines());
        Assert.Equal(3, vm.Subtitles.Count);

        vm.FilterText = "expert";
        Assert.Equal(new[] { 1, 3 }, vm.Subtitles.Select(p => p.Number));
        Assert.Equal(1, vm.SelectedSubtitle?.Number);

        vm.FilterText = "nothing matches";
        Assert.Empty(vm.Subtitles);
        Assert.False(vm.HasBookmarks);

        vm.FilterText = string.Empty;
        Assert.Equal(3, vm.Subtitles.Count);
    }

    [AvaloniaFact]
    public void Window_ShowsFilteredRows()
    {
        var vm = new BookmarksListViewModel(new FileHelper());
        vm.Initialize(Lines());
        var window = new BookmarksListWindow(vm);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        try
        {
            vm.FilterText = "own";
            Dispatcher.UIThread.RunJobs();
            Assert.Single(vm.Subtitles);

            var dir = Environment.GetEnvironmentVariable("SHOT_DIR");
            if (!string.IsNullOrEmpty(dir))
            {
                vm.FilterText = string.Empty;
                for (var i = 0; i < 5; i++)
                {
                    Dispatcher.UIThread.RunJobs();
                    AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                }

                window.CaptureRenderedFrame()!.Save(Path.Combine(dir, "bookmarks-list.png"), PngBitmapEncoderOptions.Default);
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
