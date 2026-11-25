using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSync;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Sync.PointSync;

/// <summary>
/// Lines that are part of a sync point stay marked in the "Point sync" grid, like in
/// "Point sync via other subtitle" (issue #17053).
/// </summary>
public class PointSyncSyncedLinesTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static SubtitleLineViewModel Line(double startMs) => new()
    {
        Text = "Hello",
        StartTime = TimeSpan.FromMilliseconds(startMs),
        EndTime = TimeSpan.FromMilliseconds(startMs + 1000),
    };

    [Fact]
    public void AddingAndDeletingSyncPoint_MarksAndUnmarksTheLine()
    {
        var vm = new PointSyncViewModel(new WindowService(new NullServiceProvider()));
        vm.Initialize(new List<SubtitleLineViewModel> { Line(1000), Line(3000), Line(5000) },
            0, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default, null);
        var changes = 0;
        vm.SyncedLinesChanged += () => changes++;

        var syncPoint = new SyncPoint(vm.Subtitles[1], 1, Line(3500), 1);
        vm.SyncPoints.Add(syncPoint);

        Assert.True(vm.IsSynced(vm.Subtitles[1]));
        Assert.False(vm.IsSynced(vm.Subtitles[0]));
        Assert.False(vm.IsSynced(vm.Subtitles[2]));
        Assert.True(changes > 0);

        vm.SelectedSyncPoint = syncPoint;
        vm.DeleteSelectedPointSyncCommand.Execute(null);

        Assert.DoesNotContain(vm.Subtitles, vm.IsSynced);
    }
}
