using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Assa.AssaProgressBar;

namespace UITests.Features.Assa;

/// <summary>
/// Progress bar styles, edge margin and the playhead knob.
/// </summary>
public class AssaProgressBarStylesTests
{
    [Fact]
    public void EveryStyle_FitsTheBarBox_AndSpansTheWidth()
    {
        Assert.Equal(18, ProgressBarStyles.Names.Count);
        for (var i = 0; i < ProgressBarStyles.Names.Count; i++)
        {
            var bounds = AssaDrawingGeometry.FromDrawing(ProgressBarStyles.Drawing(i, 1920, 40)).Bounds;
            Assert.True(bounds.Left >= -0.5 && bounds.Right <= 1920.5 && bounds.Top >= -0.5 && bounds.Bottom <= 40.5, ProgressBarStyles.Names[i]);
            Assert.True(bounds.Width > 1800, ProgressBarStyles.Names[i]);
        }
    }

    private static AssaProgressBarViewModel MakeViewModel()
    {
        var vm = new AssaProgressBarViewModel();
        vm.Initialize(new Subtitle { Header = AdvancedSubStationAlpha.DefaultHeader }, 1920, 1080, 60_000, null);
        return vm;
    }

    [AvaloniaFact]
    public void BarStyle_IsTheFilledDrawing()
    {
        var vm = MakeViewModel();
        vm.CornerStyleIndex = 7; // dots

        Assert.Contains("\\K6000\\p1}" + ProgressBarStyles.Drawing(7, 1920, vm.BarHeight) + "{\\p0}", vm.PreviewCode);
        Assert.Equal(ProgressBarStyles.Names[7], vm.BarStyleName);
    }

    [AvaloniaFact]
    public void EdgeMargin_LiftsBarAndChapters_AndPlayheadMovesAcross()
    {
        var vm = MakeViewModel();
        vm.PositionBottom = true;
        vm.BarHeight = 20;
        vm.EdgeMargin = 30;
        vm.ShowPlayhead = true;
        vm.AddChapterCommand.Execute(null);

        Assert.Contains("SE-progress-bar-bg,Arial,20", vm.PreviewCode);
        Assert.Contains(",1,0,0,30,1", vm.PreviewCode); // alignment 1, MarginV 30

        // Chapter text centered on the lifted bar: 1080 - 30 - 20 + 10
        Assert.Contains("\\pos(", vm.PreviewCode);
        Assert.Contains(", 1040)}", vm.PreviewCode);

        // Knob: 32 px (1.6 x height), centered on the bar, moving over the whole minute
        Assert.Contains("\\move(-16,1024,1904,1024,0,60000)", vm.PreviewCode);
    }
}
