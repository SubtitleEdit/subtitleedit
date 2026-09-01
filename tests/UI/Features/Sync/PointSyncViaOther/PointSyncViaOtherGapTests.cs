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
/// The "Gap" column in point sync via other shows the silence *before* each line - the tell
/// for a reliable sync point (issue #10175) - unlike the main grid's gap-to-next.
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
    public void Initialize_ComputesTheGapBeforeEachLine()
    {
        var vm = MakeViewModel();
        var lines = new List<SubtitleLineViewModel>
        {
            Line(4000, 6000),
            Line(6500, 8000),
            Line(12000, 14000),
        };

        vm.Initialize(lines, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);

        // The first line's gap is measured from 00:00 - it too starts after "silence".
        Assert.Equal(4000, vm.Subtitles[0].PreviousGap, 3);
        Assert.Equal(500, vm.Subtitles[1].PreviousGap, 3);
        Assert.Equal(4000, vm.Subtitles[2].PreviousGap, 3);
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
        vm.Initialize(lines, string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);

        // One sync point moving the first line from 1000 ms to 2000 ms (+1000 ms shift).
        vm.SelectedSubtitle = vm.Subtitles[0];
        var syncPoint = new SyncPoint(Line(1000, 3000), 0, Line(2000, 3000), 0);
        vm.SyncPoints.Add(syncPoint);

        vm.ApplyCommand.Execute(null);

        Assert.Equal(2000, vm.Subtitles[0].StartTime.TotalMilliseconds, 3);
        Assert.Equal(2000, vm.Subtitles[0].PreviousGap, 3);
        Assert.Equal(2000, vm.Subtitles[1].PreviousGap, 3);
    }

    [AvaloniaFact]
    public void Window_HasAGapColumnInBothGridsAndALegend()
    {
        var vm = MakeViewModel();
        vm.Initialize(new List<SubtitleLineViewModel> { Line(1000, 3000) },
            string.Empty, string.Empty, VideoPreviewSubtitleContext.Default);

        var window = new PointSyncViaOtherWindow(vm);
        try
        {
            var gapHeaders = window.GetLogicalDescendants()
                .OfType<TableView>()
                .SelectMany(t => t.Columns)
                .Count(c => Equals(c.Header, Se.Language.General.Gap));
            Assert.Equal(2, gapHeaders);

            var legendText = string.Format(Se.Language.Sync.SyncPointCandidateInfo, 3);
            Assert.Contains(window.GetLogicalDescendants().OfType<TextBlock>(),
                t => t.Text == legendText);
        }
        finally
        {
            window.Close();
        }
    }
}
