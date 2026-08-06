using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;

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
}
