using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.SubtitleFormats.Assa;

/// <summary>
/// LayoutResX/LayoutResY (libass 0.17+), issue #16933.
/// </summary>
public class LayoutResolutionTest
{
    private static string HeaderWith(string extraScriptInfo) =>
        AdvancedSubStationAlpha.DefaultHeader.Replace("[V4+ Styles]", extraScriptInfo + Environment.NewLine + Environment.NewLine + "[V4+ Styles]");

    [Fact]
    public void TryGet_ReadsBothValues()
    {
        var header = HeaderWith("LayoutResX: 1024\r\nLayoutResY: 576");

        Assert.True(AdvancedSubStationAlpha.TryGetLayoutResolution(header, out var w, out var h));
        Assert.Equal(1024, w);
        Assert.Equal(576, h);
    }

    [Theory]
    [InlineData("")]
    [InlineData("LayoutResX: 1024")]
    [InlineData("LayoutResX: 0\r\nLayoutResY: 576")]
    [InlineData("LayoutResX: abc\r\nLayoutResY: 576")]
    public void TryGet_MissingOrInvalid_IsFalse(string extra)
    {
        Assert.False(AdvancedSubStationAlpha.TryGetLayoutResolution(HeaderWith(extra), out _, out _));
    }

    [Fact]
    public void Set_AddsAndReplaces_WithoutTouchingPlayRes()
    {
        var header = AdvancedSubStationAlpha.SetResolution(AdvancedSubStationAlpha.DefaultHeader, 1920, 1080);

        header = AdvancedSubStationAlpha.SetLayoutResolution(header, 1280, 720);
        header = AdvancedSubStationAlpha.SetLayoutResolution(header, 1920, 1080);

        Assert.True(AdvancedSubStationAlpha.TryGetLayoutResolution(header, out var w, out var h));
        Assert.Equal((1920, 1080), (w, h));
        Assert.Single(header.SplitToLines(), l => l.StartsWith("LayoutResX:", StringComparison.Ordinal));
        Assert.Equal("1920", AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", header));
        Assert.Contains("[V4+ Styles]", header);
    }

    [Fact]
    public void Set_Zero_RemovesBoth()
    {
        var header = HeaderWith("LayoutResX: 1024\r\nLayoutResY: 576");

        header = AdvancedSubStationAlpha.SetLayoutResolution(header, 0, 0);

        Assert.DoesNotContain("LayoutRes", header);
        Assert.Contains("[V4+ Styles]", header);
    }

    [Fact]
    public void LoadAndSave_KeepsLayoutRes()
    {
        var format = new AdvancedSubStationAlpha();
        var source = new Subtitle { Header = HeaderWith("LayoutResX: 1024\r\nLayoutResY: 576") };
        source.Paragraphs.Add(new Paragraph("Hello", 1000, 2000));
        var text = format.ToText(source, "test");

        var loaded = new Subtitle();
        format.LoadSubtitle(loaded, text.SplitToLines(), "test.ass");

        Assert.True(AdvancedSubStationAlpha.TryGetLayoutResolution(loaded.Header, out var w, out var h));
        Assert.Equal((1024, 576), (w, h));
    }

    [Theory]
    [InlineData(1920, 1080, 1280, 720, 1920, 1080, 1920, 1080)] // proportional: unchanged
    [InlineData(1920, 1080, 640, 480, 1440, 1080, 1920, 1080)] // 4:3 PlayRes on 16:9 layout, proportional
    [InlineData(1920, 1080, 1440, 1080, 1920, 1080, 2560, 1080)] // stretch 4:3 -> 16:9: X follows AR
    [InlineData(1024, 576, 720, 576, 1024, 576, 1456, 576)]
    public void AfterResample_FollowsAspectRatioChangeOnly(int lw, int lh, int sw, int sh, int tw, int th, int ew, int eh)
    {
        var (w, h) = AdvancedSubStationAlpha.GetLayoutResolutionAfterResample(lw, lh, sw, sh, tw, th);

        Assert.Equal((ew, eh), (w, h));
    }

    [Theory]
    [InlineData(720, 576, 16.0 / 9.0, 1024, 576)]   // anamorphic PAL widescreen
    [InlineData(720, 576, 4.0 / 3.0, 768, 576)]     // PAL 4:3
    [InlineData(720, 480, 4.0 / 3.0, 720, 540)]     // NTSC 4:3: taller, never narrower
    [InlineData(1920, 1080, 16.0 / 9.0, 1920, 1080)]
    [InlineData(1920, 1080, 0, 1920, 1080)]         // no DAR: square pixels
    public void DisplayResolution_WidensToDisplayAspect(int w, int h, double dar, int ew, int eh)
    {
        Assert.Equal((ew, eh), AdvancedSubStationAlpha.GetDisplayResolution(w, h, dar));
    }
}
