using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaDraw;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;

namespace UITests.Features.Assa;

/// <summary>
/// Drawing coordinates are in script (PlayRes) space: the canvas has to start at the header's
/// resolution, and a header without one must get the canvas size on OK, or libass falls back to
/// 384x288 and scales the drawing off screen.
/// </summary>
public class AssaDrawResolutionTests
{
    private static Subtitle MakeSubtitle(string header) => new() { Header = header };

    private static SubtitleLineViewModel MakeLine(string text) =>
        new(new Paragraph(text, 0, 2000) { Extra = "Default" }, new AdvancedSubStationAlpha());

    [Fact]
    public void Initialize_HeaderPlayRes_WinsOverVideoSize()
    {
        var header = AdvancedSubStationAlpha.SetResolution(AdvancedSubStationAlpha.DefaultHeader, 1280, 720);
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());

        vm.Initialize(MakeSubtitle(header), [], null, null);

        Assert.Equal(1280, vm.CanvasWidth);
        Assert.Equal(720, vm.CanvasHeight);
    }

    [Fact]
    public void Initialize_ZeroVideoSize_KeepsDefaultCanvas()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());

        vm.Initialize(MakeSubtitle(AdvancedSubStationAlpha.DefaultHeader), [], 0, 0);

        Assert.Equal(1920, vm.CanvasWidth);
        Assert.Equal(1080, vm.CanvasHeight);
    }

    [Fact]
    public void Ok_HeaderWithoutPlayRes_GetsCanvasSize()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        vm.Initialize(MakeSubtitle(AdvancedSubStationAlpha.DefaultHeader), [MakeLine("{\\p1}m 0 0 l 100 0 100 100{\\p0}")], 1280, 720);

        vm.OkCommand.Execute(null);

        var header = vm.ResultSubtitle.Header;
        Assert.Equal("1280", AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", header));
        Assert.Equal("720", AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", header));
    }

    [Fact]
    public void Initialize_DrawingWithPosInSameTagBlock_IsImported()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());

        vm.Initialize(MakeSubtitle(AdvancedSubStationAlpha.DefaultHeader),
            [MakeLine("{\\an7\\pos(0,0)\\iclip(m 10 10 l 20 10 20 20)\\p1}m 0 0 l 100 0 100 100{\\p0}")], 1920, 1080);

        Assert.Equal(2, vm.Shapes.Count);
        Assert.Single(vm.Shapes, s => s.IsEraser);
        Assert.Equal("m 0 0 l 100 0 100 100", vm.Shapes.Single(s => !s.IsEraser).ToAssa());
    }

    [Fact]
    public void Ok_EraserOnlyLayer_GetsExistingStyle()
    {
        var vm = new AssaDrawViewModel(new FileHelper(), new StubWindowService());
        var drawLine = MakeLine("{\\p1}m 0 0 l 100 0 100 100{\\p0}");
        var maskLine = MakeLine("{\\iclip(m 10 10 l 20 10 20 20)}");
        maskLine.Layer = 1;
        vm.Initialize(MakeSubtitle(AdvancedSubStationAlpha.DefaultHeader), [drawLine, maskLine], 1920, 1080);

        vm.OkCommand.Execute(null);

        var styleNames = AdvancedSubStationAlpha.GetSsaStylesFromHeader(vm.ResultSubtitle.Header).Select(s => s.Name).ToList();
        var mask = Assert.Single(vm.ResultSubtitle.Paragraphs, p => p.Layer == 1);
        Assert.StartsWith("{\\iclip(", mask.Text);
        Assert.False(string.IsNullOrEmpty(mask.Extra));
        Assert.Contains(mask.Extra, styleNames);
    }
}
