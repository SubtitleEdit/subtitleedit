using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaSetPosition;

namespace UITests.Features.Assa;

/// <summary>
/// Regression tests for the "Set position" coordinate spaces: the overlay pipeline measures in
/// render (video pixel) space, but libass interprets \pos() in PlayRes space — e.g.
/// \pos(320,180) with PlayRes 640x360 is the center of a 1080p frame. The dialog used to write
/// render pixels into the tag (off by renderSize/playRes), and the Center buttons made the
/// opposite mistake, centering the render-space overlay on the PlayRes width.
/// </summary>
public class AssaSetPositionCoordinateSpaceTests
{
    [Theory]
    [InlineData(960.0, 640, 1920, 320)] // 3x downscale
    [InlineData(960.0, 1920, 1920, 960)] // identity when PlayRes == render size
    [InlineData(100.4, 1920, 1920, 100)] // rounds
    [InlineData(100.5, 1920, 1920, 101)] // ... away from zero
    [InlineData(960.0, 3840, 1920, 1920)] // upscale
    [InlineData(960.0, 0, 1920, 960)] // degenerate PlayRes: pass through
    [InlineData(960.0, 640, 0, 960)] // degenerate render size: pass through
    public void ToScriptSpace_ScalesByPlayResOverRenderSize(double renderValue, int playRes, int renderSize, int expected)
    {
        Assert.Equal(expected, AssaSetPositionViewModel.ToScriptSpace(renderValue, playRes, renderSize));
    }

    [AvaloniaFact]
    public void ResultXy_ConvertRenderSpaceToPlayResSpace()
    {
        // Fresh VM: 1x1 overlay bitmap, centered/bottom alignment, render canvas 1920x1080.
        var vm = new AssaSetPositionViewModel
        {
            SourceWidth = 640,
            SourceHeight = 360,
            ScreenshotX = 300,
            ScreenshotY = 600,
        };

        // X: (300 + 0.5) * 640 / 1920 = 100.17 -> 100; Y: (600 + 1) * 360 / 1080 = 200.33 -> 200.
        Assert.Equal(100, vm.ResultX);
        Assert.Equal(200, vm.ResultY);
    }

    [AvaloniaFact]
    public void ResultXy_AreUnchangedWhenPlayResMatchesRenderSize()
    {
        var vm = new AssaSetPositionViewModel
        {
            SourceWidth = 1920,
            SourceHeight = 1080,
            ScreenshotX = 300,
            ScreenshotY = 600,
        };

        // Same values the pre-conversion code produced for the 1x1 overlay: 300.5 -> 301, 601.
        Assert.Equal(301, vm.ResultX);
        Assert.Equal(601, vm.ResultY);
    }

    [AvaloniaFact]
    public async Task CenterButtons_CenterOnTheRenderCanvasNotPlayRes()
    {
        var vm = new AssaSetPositionViewModel
        {
            SourceWidth = 640, // must not influence centering
            SourceHeight = 360,
        };

        await vm.CenterHorizontallyCommand.ExecuteAsync(null);
        await vm.CenterVerticallyCommand.ExecuteAsync(null);

        // Render canvas defaults to 1920x1080; overlay is 1x1: 960 - 0.5 -> 960, 540 - 0.5 -> 540.
        Assert.Equal(960, vm.ScreenshotX);
        Assert.Equal(540, vm.ScreenshotY);

        // And the resulting \pos anchor is the script-space center.
        Assert.Equal(320, vm.ResultX);
        Assert.Equal(180, vm.ResultY);
    }

    [Fact]
    public void EnsurePlayRes_ReadsExistingResolution()
    {
        var header = AdvancedSubStationAlpha.SetResolution(AdvancedSubStationAlpha.DefaultHeader, 640, 360);

        var (result, w, h) = AssaSetPositionViewModel.EnsurePlayRes(header, 1920, 1080);

        Assert.Equal(header, result);
        Assert.Equal(640, w);
        Assert.Equal(360, h);
    }

    [Fact]
    public void EnsurePlayRes_StampsFallbackWhenMissing()
    {
        // The default header carries no PlayRes lines - libass would assume 384x288.
        var header = AdvancedSubStationAlpha.DefaultHeader;

        var (result, w, h) = AssaSetPositionViewModel.EnsurePlayRes(header, 1920, 1080);

        Assert.Equal(1920, w);
        Assert.Equal(1080, h);
        Assert.Equal("1920", AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", result));
        Assert.Equal("1080", AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", result));
    }
}
