using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Sync.PointSyncViaOther;

/// <summary>
/// Lines that are part of a sync point stay marked in both grids, so it is easy to see which
/// lines are already synced (issue #17053).
/// </summary>
public class PointSyncViaOtherSyncedLinesTests
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

    private static PointSyncViaOtherViewModel MakeViewModel()
    {
        var vm = new PointSyncViaOtherViewModel(new FileHelper(), new WindowService(new NullServiceProvider()));
        vm.Initialize(new List<SubtitleLineViewModel> { Line(1000), Line(3000), Line(5000) },
            0, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);
        vm.Othersubtitles.Add(Line(1500));
        vm.Othersubtitles.Add(Line(3500));
        vm.Othersubtitles.Add(Line(5500));
        return vm;
    }

    [Fact]
    public void SetSyncPoint_MarksBothLines_DeleteUnmarksThem()
    {
        var vm = MakeViewModel();
        var changes = 0;
        vm.SyncedLinesChanged += () => changes++;

        vm.SelectedSubtitle = vm.Subtitles[1];
        vm.SelectedOtherSubtitle = vm.Othersubtitles[2];
        vm.SetSyncPointCommand.Execute(null);

        Assert.True(vm.IsSynced(vm.Subtitles[1]));
        Assert.True(vm.IsSynced(vm.Othersubtitles[2]));
        Assert.False(vm.IsSynced(vm.Subtitles[0]));
        Assert.False(vm.IsSynced(vm.Othersubtitles[1]));
        Assert.True(changes > 0);

        vm.DeleteSelectedPointSyncCommand.Execute(null);

        Assert.False(vm.IsSynced(vm.Subtitles[1]));
        Assert.False(vm.IsSynced(vm.Othersubtitles[2]));
    }

    [Fact]
    public void ResettingALineSyncPoint_MovesTheOtherMark()
    {
        var vm = MakeViewModel();

        vm.SelectedSubtitle = vm.Subtitles[0];
        vm.SelectedOtherSubtitle = vm.Othersubtitles[0];
        vm.SetSyncPointCommand.Execute(null);

        vm.SelectedOtherSubtitle = vm.Othersubtitles[1];
        vm.SetSyncPointCommand.Execute(null);

        Assert.True(vm.IsSynced(vm.Subtitles[0]));
        Assert.False(vm.IsSynced(vm.Othersubtitles[0]));
        Assert.True(vm.IsSynced(vm.Othersubtitles[1]));
    }

    [Fact]
    public void SyncPointWithoutOtherLine_MarksOnlyTheLeftLine()
    {
        // A sync point set via video has no other-subtitle line (right index -1).
        var vm = MakeViewModel();
        vm.SyncPoints.Add(new SyncPoint(vm.Subtitles[2], 2, Line(9000), -1));

        Assert.True(vm.IsSynced(vm.Subtitles[2]));
        Assert.DoesNotContain(vm.Othersubtitles, vm.IsSynced);
    }
}
