using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// --layout-resolution writes ASSA LayoutResX/LayoutResY (#16933).
/// </summary>
public class LayoutResolutionPostProcessTest
{
    [Fact]
    public void LayoutResolution_IsWritten_NextToPlayRes()
    {
        var subtitle = new Subtitle();

        LibSEIntegration.ApplyAssaPostProcess(subtitle, "AdvancedSubStationAlpha", (720, 576), null, (1024, 576));

        Assert.Equal("720", AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", subtitle.Header));
        Assert.True(AdvancedSubStationAlpha.TryGetLayoutResolution(subtitle.Header, out var w, out var h));
        Assert.Equal((1024, 576), (w, h));
    }

    [Fact]
    public void NoLayoutResolution_LeavesHeaderWithout()
    {
        var subtitle = new Subtitle();

        LibSEIntegration.ApplyAssaPostProcess(subtitle, "AdvancedSubStationAlpha", (1920, 1080), null);

        Assert.DoesNotContain("LayoutRes", subtitle.Header);
    }
}
