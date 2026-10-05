using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UITests.Features.Sync.PointSyncViaOther;

/// <summary>
/// The "Gap after" column in point sync via other shows the silence after each line, like the
/// main grid's "Gap" - a long one is the tell for a reliable sync point (issues #10175, #15695).
/// </summary>
public class PointSyncViaOtherGapTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static PointSyncViaOtherViewModel MakeViewModel()
        => new(new FileHelper(), new WindowService(new NullServiceProvider()));

    private static SubtitleLineViewModel Line(double startMs, double endMs) => new()
    {
        Text = "Hello",
        StartTime = TimeSpan.FromMilliseconds(startMs),
        EndTime = TimeSpan.FromMilliseconds(endMs),
    };

    [Fact]
    public void Initialize_ComputesTheGapAfterEachLine()
    {
        var vm = MakeViewModel();
        var lines = new List<SubtitleLineViewModel>
        {
            Line(4000, 6000),
            Line(6500, 8000),
            Line(12000, 14000),
        };

        vm.Initialize(lines, 0, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);

        Assert.Equal(500, vm.Subtitles[0].Gap, 3);
        Assert.Equal(4000, vm.Subtitles[1].Gap, 3);

        // The last line has no next line - its gap is the "no value" sentinel (shown blank).
        Assert.Equal(double.MaxValue, vm.Subtitles[2].Gap);
    }

    [Fact]
    public void Apply_RecomputesGapsFromThePreviewedTimes()
    {
        var vm = MakeViewModel();
        var lines = new List<SubtitleLineViewModel>
        {
            Line(1000, 3000),
            Line(5000, 6000),
        };
        vm.Initialize(lines, 0, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);

        // One sync point moving the first line from 1000 ms to 2000 ms (+1000 ms shift).
        vm.SelectedSubtitle = vm.Subtitles[0];
        var syncPoint = new SyncPoint(Line(1000, 3000), 0, Line(2000, 3000), 0);
        vm.SyncPoints.Add(syncPoint);
        vm.Subtitles[0].Gap = 0; // stale - Apply must recompute it

        vm.ApplyCommand.Execute(null);

        // One sync point shifts both lines +1000 ms (2000-4000, 6000-7000), keeping the gap.
        Assert.Equal(2000, vm.Subtitles[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(2000, vm.Subtitles[0].Gap, 3);
    }

    [AvaloniaFact]
    public void Window_HasAGapAfterColumnInBothGridsAndALegend()
    {
        var vm = MakeViewModel();
        vm.Initialize(new List<SubtitleLineViewModel> { Line(1000, 3000) },
            0, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);

        var window = new PointSyncViaOtherWindow(vm);
        try
        {
            var gapHeaders = window.GetLogicalDescendants()
                .OfType<TableView>()
                .SelectMany(t => t.Columns)
                .Count(c => Equals(c.Header, Se.Language.Sync.GapAfter));
            Assert.Equal(2, gapHeaders);

            var legendText = string.Format(Se.Language.Sync.SyncPointCandidateAfterInfo, 3);
            Assert.Contains(window.GetLogicalDescendants().OfType<TextBlock>(),
                t => t.Text == legendText);
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2999, 0)]
    [InlineData(3000, 1)]
    [InlineData(5999, 1)]
    [InlineData(6000, 2)]
    [InlineData(12000, 3)]
    [InlineData(24999, 3)]
    [InlineData(25000, 4)]
    [InlineData(41185, 4)]
    [InlineData(-200, 0)]
    [InlineData(double.MaxValue, 0)]
    [InlineData(double.NaN, 0)]
    public void SyncCandidateLevel_GrowsWithTheSilence(double gapMs, int expectedLevel)
    {
        Assert.Equal(expectedLevel, PointSyncViaOtherWindow.GetSyncCandidateLevel(gapMs));
    }
}
