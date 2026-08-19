using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace LibSETests.SubtitleFormats;

public class SubStationAlphaTest
{
    private const string SsaWithMarkedDefault = @"[Script Info]
ScriptType: v4.00

[V4 Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
Style: Default,Arial,20,16777215,65535,0,0,0,0,1,1,1,2,10,10,10,0,1

[Events]
Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: Marked=0,0:20:19.02,0:20:23.82,*Default,NTP,0000,0000,0000,,Wir freuen uns sehr,
Dialogue: Marked=0,0:20:24.01,0:20:26.44,*Default,NTP,0000,0000,0000,,Du wirst sehr reich werden.";

    [Fact]
    public void LeadingAsteriskOnStyleNameIsStripped()
    {
        // Issue #11342: "*Default" (old SSA "marked" convention) must resolve to the "Default"
        // style defined in [V4 Styles], otherwise the style column shows up empty.
        var subtitle = new Subtitle();
        new SubStationAlpha().LoadSubtitle(subtitle, new List<string>(SsaWithMarkedDefault.SplitToLines()), "test.ssa");

        Assert.Equal(2, subtitle.Paragraphs.Count);
        Assert.All(subtitle.Paragraphs, p => Assert.Equal("Default", p.Extra));
    }

    // A header-less payload (bare event lines, as the clipboard carries) must parse its events
    // without any of them ending up in Header, which consumers treat as pre-[Events] content.
    [Theory]
    [InlineData("ssa", "Dialogue: Marked=0,0:00:01.00,0:00:02.00,*Default,NTP,0000,0000,0000,,Hello")]
    [InlineData("ssa", "Comment: Marked=0,0:00:01.00,0:00:02.00,*Default,NTP,0000,0000,0000,,Hello")]
    [InlineData("ass", "Dialogue: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,Hello")]
    [InlineData("ass", "Comment: 0,0:00:01.00,0:00:02.00,Default,,0,0,0,,Hello")]
    public void HeaderLessEventPayload_DoesNotPutEventLinesInHeader(string extension, string eventLine)
    {
        var subtitle = new Subtitle();
        SubtitleFormat format = extension == "ass" ? new AdvancedSubStationAlpha() : new SubStationAlpha();

        format.LoadSubtitle(subtitle, new List<string> { eventLine }, "test." + extension);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello", subtitle.Paragraphs[0].Text);

        var header = subtitle.Header ?? string.Empty;
        Assert.DoesNotContain("Dialogue:", header, System.StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Comment:", header, System.StringComparison.OrdinalIgnoreCase);
    }

    // The normal case must be untouched: a full file still captures every pre-[Events] section.
    [Fact]
    public void FullFile_StillCapturesHeaderSections()
    {
        var subtitle = new Subtitle();

        new SubStationAlpha().LoadSubtitle(subtitle, new List<string>(SsaWithMarkedDefault.SplitToLines()), "test.ssa");

        Assert.Contains("[Script Info]", subtitle.Header);
        Assert.Contains("[V4 Styles]", subtitle.Header);
        Assert.DoesNotContain("Dialogue:", subtitle.Header, System.StringComparison.OrdinalIgnoreCase);
    }

    private static string SsaWithStyle(string styleLine) => @"[Script Info]
; This is a Sub Station Alpha v4 script.
Title: t
ScriptType: v4.00
Collisions: Normal
PlayDepth: 0

[V4 Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
" + styleLine + @"

[Events]
Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: Marked=0,0:00:01.00,0:00:02.00,*Default,NTP,0000,0000,0000,,Hello";

    /// <summary>
    /// What the styles dialog does on OK: read the styles out of the .ssa header and write them
    /// straight back. Nothing was edited, so the style line must come back unchanged.
    /// </summary>
    private static string StylesDialogRoundTrip(string styleLine)
    {
        var subtitle = new Subtitle();
        new SubStationAlpha().LoadSubtitle(subtitle, new List<string>(SsaWithStyle(styleLine).SplitToLines()), "test.ssa");

        var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(subtitle.Header);
        var assaHeader = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(subtitle.Header, styles);
        var ssaHeader = SubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(assaHeader, string.Empty);

        foreach (var line in ssaHeader.SplitToLines())
        {
            if (line.StartsWith("Style:", System.StringComparison.Ordinal))
            {
                return line;
            }
        }

        return string.Empty;
    }

    // Issue #13734: colors were written as 32-bit ARGB ("4294967040" for yellow) instead of the
    // "&H00BBGGRR" decimal SSA v4 uses, so every color was rejected by players and lost on reopen.
    [Fact]
    public void StyleColorsSurviveTheStylesDialog()
    {
        // yellow primary/secondary, red tertiary (= outline), blue back
        const string styleLine = "Style: Default,Arial,20,65535,65535,255,16711680,0,0,1,2,3,2,11,12,13,0,1";

        Assert.Equal(styleLine, StylesDialogRoundTrip(styleLine));
    }

    // Issue #13734: outline width, shadow width and the three margins came back as the defaults.
    [Fact]
    public void StyleNumbersSurviveTheStylesDialog()
    {
        const string styleLine = "Style: Default,Arial,22,16777215,65535,0,0,-1,-1,3,2,3,2,11,12,13,0,1";

        Assert.Equal(styleLine, StylesDialogRoundTrip(styleLine));
    }

    // Issue #13734: SSA v4 numbers alignment 5-7 top and 9-11 middle, which was read as if it were
    // the "an1"-"an9" of [V4+ Styles] - top left came back as middle center, and 10/11 as bottom.
    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    [InlineData("5")]
    [InlineData("6")]
    [InlineData("7")]
    [InlineData("9")]
    [InlineData("10")]
    [InlineData("11")]
    public void StyleAlignmentSurvivesTheStylesDialog(string alignment)
    {
        var styleLine = $"Style: Default,Arial,20,16777215,65535,0,0,0,0,1,1,1,{alignment},10,10,10,0,1";

        Assert.Equal(styleLine, StylesDialogRoundTrip(styleLine));
    }

    // A comma-decimal locale must not split a fractional font size into two fields.
    [Fact]
    public void StyleWithFractionalFontSizeIsWrittenInvariantly()
    {
        var oldCulture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            const string styleLine = "Style: Default,Arial,20.5,16777215,65535,0,0,0,0,1,1.5,1,2,10,10,10,0,1";

            Assert.Equal(styleLine, StylesDialogRoundTrip(styleLine));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = oldCulture;
        }
    }

    // Converting .ssa to .ass must carry the outline colour (SSA's TertiaryColour) and the
    // widths/margins over - all of them used to be reset to defaults.
    [Fact]
    public void ConvertingToAdvancedSubStationAlphaKeepsStyleValues()
    {
        var subtitle = new Subtitle();
        new SubStationAlpha().LoadSubtitle(
            subtitle,
            new List<string>(SsaWithStyle("Style: Default,Arial,20,65535,65535,255,16711680,0,0,1,2,3,6,11,12,13,0,1").SplitToLines()),
            "test.ssa");

        var assaHeader = AdvancedSubStationAlpha.GetHeaderAndStylesFromSubStationAlpha(subtitle.Header);
        var style = AdvancedSubStationAlpha.GetSsaStyle("Default", assaHeader);

        Assert.Equal(SKColors.Yellow, style.Primary);
        Assert.Equal(SKColors.Red, style.Outline);
        Assert.Equal(SKColors.Blue, style.Background);
        Assert.Equal(2m, style.OutlineWidth);
        Assert.Equal(3m, style.ShadowWidth);
        Assert.Equal(11, style.MarginLeft);
        Assert.Equal(12, style.MarginRight);
        Assert.Equal(13, style.MarginVertical);
        Assert.Equal("8", style.Alignment); // SSA 6 (top center) is "an8"
    }

    // Files SE already wrote with the too-wide value must not read back as the default color.
    [Fact]
    public void OverlyWideColorValueIsStillParsed()
    {
        // 4294967040 == 0xFFFFFF00, the 32-bit "&HAABBGGRR" word written unsigned
        var color = AdvancedSubStationAlpha.GetSsaColor("4294967040", SKColors.White);

        Assert.Equal(SKColors.Cyan, color);
    }
}
