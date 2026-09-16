using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;

namespace UITests.Logic.Media;

/// <summary>
/// SecondarySubtitleMerger already resamples the secondary's *style* (font size, margins) to the
/// target header's PlayRes. A paragraph carrying a `\pos` override (SecondarySubtitleJustifier's
/// "Justify lines") is stated in the secondary's own PlayRes too, and needs the exact same
/// rescale - otherwise it renders off-screen against a target with a different PlayRes, such as a
/// SubRip primary (#14842 review).
/// </summary>
public class SecondarySubtitleMergerTests
{
    private static string MakeHeader(int playResX, int playResY, SsaStyle style)
    {
        var header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader, new List<SsaStyle> { style });
        header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + playResX, "[Script Info]", header);
        header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + playResY, "[Script Info]", header);
        return header;
    }

    private static Subtitle MakeSecondary(int playResX, int playResY, string text)
    {
        var style = new SsaStyle { Name = "Secondary", FontSize = 40, MarginLeft = 10, MarginRight = 10, MarginVertical = 10 };
        var subtitle = new Subtitle { Header = MakeHeader(playResX, playResY, style) };
        subtitle.Paragraphs.Add(new Paragraph(text, 0, 1000) { Extra = "Secondary" });
        return subtitle;
    }

    private static Subtitle MakeTarget(int playResX, int playResY)
    {
        var style = new SsaStyle { Name = "Default", FontSize = 40 };
        return new Subtitle { Header = MakeHeader(playResX, playResY, style) };
    }

    [Fact]
    public void AddSecondarySubtitle_SamePlayRes_DoesNotRescalePosAndAddsByReference()
    {
        var secondary = MakeSecondary(1920, 1080, "{\\an7\\pos(100,200)}Line");
        var originalText = secondary.Paragraphs[0].Text;
        var target = MakeTarget(1920, 1080);

        SecondarySubtitleMerger.AddSecondarySubtitle(target, secondary, smpteMode: false);

        Assert.Same(secondary.Paragraphs[0], target.Paragraphs[0]); // still added by reference
        Assert.Equal(originalText, target.Paragraphs[0].Text);
    }

    [Fact]
    public void AddSecondarySubtitle_DifferentPlayRes_RescalesPosToTarget()
    {
        // A SubRip primary's preview header carries no PlayRes at all, so MpvReloader's target
        // falls back to libass's own 384x288 default - this is exactly that case.
        var secondary = MakeSecondary(1920, 1080, "{\\an7\\pos(960,1070)}Line");
        var target = MakeTarget(384, 288);

        SecondarySubtitleMerger.AddSecondarySubtitle(target, secondary, smpteMode: false);

        var text = target.Paragraphs[0].Text;
        Assert.Contains("\\pos(192,", text); // 960 * (384/1920) = 192
        Assert.DoesNotContain("\\pos(960,", text);
    }

    [Fact]
    public void AddSecondarySubtitle_DifferentPlayRes_LeavesTheSourceParagraphUntouched()
    {
        // The secondary subtitle is shared across every preview refresh (every ~500ms while a
        // dialog with a live preview is open); mutating the source in place would rescale it a
        // little further on every single call.
        var secondary = MakeSecondary(1920, 1080, "{\\an7\\pos(960,1070)}Line");
        var originalText = secondary.Paragraphs[0].Text;
        var target = MakeTarget(384, 288);

        SecondarySubtitleMerger.AddSecondarySubtitle(target, secondary, smpteMode: false);

        Assert.Equal(originalText, secondary.Paragraphs[0].Text);
        Assert.NotSame(secondary.Paragraphs[0], target.Paragraphs[0]);
    }

    [Fact]
    public void AddSecondarySubtitle_DifferentPlayRes_ParagraphWithoutPos_IsStillAddedByReference()
    {
        // "Auto" justify (or any paragraph SecondarySubtitleJustifier left untouched) has no
        // \pos to rescale - the pre-existing "add by reference" optimization should still apply.
        var secondary = MakeSecondary(1920, 1080, "Plain line, no override tags");
        var target = MakeTarget(384, 288);

        SecondarySubtitleMerger.AddSecondarySubtitle(target, secondary, smpteMode: false);

        Assert.Same(secondary.Paragraphs[0], target.Paragraphs[0]);
    }

    [Fact]
    public void AddSecondarySubtitle_DifferentPlayRes_SmpteMode_RescalesThePosOnTheStretchedCopy()
    {
        var secondary = MakeSecondary(1920, 1080, "{\\an7\\pos(960,1070)}Line");
        var originalText = secondary.Paragraphs[0].Text;
        var target = MakeTarget(384, 288);

        SecondarySubtitleMerger.AddSecondarySubtitle(target, secondary, smpteMode: true);

        Assert.Equal(originalText, secondary.Paragraphs[0].Text); // source still untouched
        Assert.Contains("\\pos(192,", target.Paragraphs[0].Text);
    }

    [Fact]
    public void AddSecondarySubtitle_DifferentHeight_StillResamplesTheStyleAsBefore()
    {
        var secondary = MakeSecondary(1920, 1080, "Line");
        var target = MakeTarget(1920, 540); // half the height, same width

        SecondarySubtitleMerger.AddSecondarySubtitle(target, secondary, smpteMode: false);

        var style = AdvancedSubStationAlpha.GetSsaStyle("Secondary", target.Header);
        Assert.Equal(20m, style.FontSize); // 40 * (540/1080)
    }
}
