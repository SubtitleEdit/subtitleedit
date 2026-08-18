using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.ResolutionResampler;
using System.Linq;

namespace UITests.Features.Assa;

/// <summary>
/// Setting the ASSA resolution from the video must not rewrite styles the user configured
/// (issue #13799): an OCR result gets the default style storage written into a header that names
/// no PlayResX/PlayResY, and resampling that header scaled font size, margins, outline and shadow
/// by the ratio to the video height. SE 4 only lifted the small built-in font sizes in this case,
/// which is what <see cref="AssaResamplerHelper.ScaleDefaultFontSizes"/> does.
/// </summary>
public class AssaResamplerDefaultFontSizeTests
{
    private static Subtitle MakeSubtitleWithStyle(decimal fontSize)
    {
        var style = new SsaStyle
        {
            Name = "Test-ASS",
            FontName = "Courier New",
            FontSize = fontSize,
            MarginLeft = 5,
            MarginRight = 5,
            MarginVertical = 10,
            OutlineWidth = 2,
            ShadowWidth = 1,
        };

        return new Subtitle
        {
            Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
                AdvancedSubStationAlpha.DefaultHeader,
                new[] { style }.ToList()),
        };
    }

    private static SsaStyle GetStyle(Subtitle subtitle)
    {
        return AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header).Single(s => s.Name == "Test-ASS");
    }

    [Fact]
    public void ScaleDefaultFontSizes_UserPickedSize_LeavesTheWholeStyleAlone()
    {
        // The reported style: 30pt with 5/5/10 margins, outline 2, shadow 1. Every one of those
        // values came back changed (44.6 / 9 / 9 / 15 / 3 / 1.5) before the fix.
        var subtitle = MakeSubtitleWithStyle(30);

        AssaResamplerHelper.ScaleDefaultFontSizes(subtitle, 1080);

        var style = GetStyle(subtitle);
        Assert.Equal(30, style.FontSize);
        Assert.Equal(5, style.MarginLeft);
        Assert.Equal(5, style.MarginRight);
        Assert.Equal(10, style.MarginVertical);
        Assert.Equal(2, style.OutlineWidth);
        Assert.Equal(1, style.ShadowWidth);
    }

    [Fact]
    public void ScaleDefaultFontSizes_BuiltInSize_IsLiftedToTheVideoHeight()
    {
        // 20pt against ASSA's 288-high default is 7% of the picture - it has to grow with PlayResY
        // or the subtitle turns unreadable. Margins and widths still stay put.
        var subtitle = MakeSubtitleWithStyle(20);

        AssaResamplerHelper.ScaleDefaultFontSizes(subtitle, 1080);

        var style = GetStyle(subtitle);
        Assert.Equal(75, style.FontSize); // 20 * 1080 / 288
        Assert.Equal(5, style.MarginLeft);
        Assert.Equal(10, style.MarginVertical);
        Assert.Equal(2, style.OutlineWidth);
        Assert.Equal(1, style.ShadowWidth);
    }

    [Fact]
    public void ScaleDefaultFontSizes_AtTheThreshold_IsStillLifted()
    {
        var subtitle = MakeSubtitleWithStyle(25);

        AssaResamplerHelper.ScaleDefaultFontSizes(subtitle, 576);

        Assert.Equal(50, GetStyle(subtitle).FontSize); // 25 * 576 / 288
    }

    [Fact]
    public void ScaleDefaultFontSizes_NoHeaderOrNoVideoHeight_DoesNothing()
    {
        var noHeader = new Subtitle();
        AssaResamplerHelper.ScaleDefaultFontSizes(noHeader, 1080);
        Assert.True(string.IsNullOrEmpty(noHeader.Header));

        var subtitle = MakeSubtitleWithStyle(20);
        AssaResamplerHelper.ScaleDefaultFontSizes(subtitle, 0);
        Assert.Equal(20, GetStyle(subtitle).FontSize);
    }

    [Fact]
    public void ApplyResampling_HeaderWithARealResolution_StillScalesEverything()
    {
        // The other branch is untouched: a file authored for 1280x720 still resamples in full.
        var subtitle = MakeSubtitleWithStyle(30);

        AssaResamplerHelper.ApplyResampling(subtitle, 1280, 720, 1920, 1080);

        var style = GetStyle(subtitle);
        Assert.Equal(45, style.FontSize);
        Assert.Equal(8, style.MarginLeft);
        Assert.Equal(15, style.MarginVertical);
        Assert.Equal(3, style.OutlineWidth);
        Assert.Equal(1.5m, style.ShadowWidth);
    }
}
