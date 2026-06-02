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
}
