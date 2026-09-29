using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace LibSETests.Core.Cea608;

public class Cea608CueBuilderTest
{
    /// <summary>
    /// Roll-up captions change the screen with every character pair - one cue per line, not per
    /// character, and a new row starts a new cue (as ffmpeg's own decoder does).
    /// </summary>
    [Fact]
    public void RollUpLineGrowsOneCue()
    {
        var paragraphs = new List<Paragraph>();
        Cea608CueBuilder.Add(paragraphs, "(", 1000, 1100);
        Cea608CueBuilder.Add(paragraphs, "(<i>in</i>", 1100, 1133);
        Cea608CueBuilder.Add(paragraphs, "(<i>inau</i>", 1133, 1166);
        Cea608CueBuilder.Add(paragraphs, "(<i>inaudi</i>", 1166, 1200);
        Cea608CueBuilder.Add(paragraphs, "(<i>inaudibl</i>", 1200, 1233);
        Cea608CueBuilder.Add(paragraphs, "(<i>inaudible</i>)", 1233, 3000);
        Cea608CueBuilder.Add(paragraphs, "(<i>inaudible</i>)\n>>", 3000, 3033);
        Cea608CueBuilder.Add(paragraphs, "(<i>inaudible</i>)\n>> S", 3033, 3066);
        Cea608CueBuilder.Add(paragraphs, "(<i>inaudible</i>)\n>> Saf", 3066, 3100);
        Cea608CueBuilder.Add(paragraphs, "(<i>inaudible</i>)\n>> Safet", 3100, 3133);
        Cea608CueBuilder.Add(paragraphs, "(<i>inaudible</i>)\n>> Safety", 3133, 4000);

        Assert.Equal(2, paragraphs.Count);
        Assert.Equal("(<i>inaudible</i>)", paragraphs[0].Text);
        Assert.Equal(1000, paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3000, paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("(<i>inaudible</i>)\n>> Safety", paragraphs[1].Text);
        Assert.Equal(3000, paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(4000, paragraphs[1].EndTime.TotalMilliseconds);
    }

    /// <summary>
    /// Pop-on captions are separate cues even when they follow each other directly, and a gap
    /// (screen cleared in between) never merges.
    /// </summary>
    [Fact]
    public void PopOnCaptionsStaySeparate()
    {
        var paragraphs = new List<Paragraph>();
        Cea608CueBuilder.Add(paragraphs, "Hello", 1000, 2000);
        Cea608CueBuilder.Add(paragraphs, "World", 2000, 3000);
        Cea608CueBuilder.Add(paragraphs, "World again", 3500, 4000);
        Cea608CueBuilder.Add(paragraphs, "  ", 4000, 5000);

        Assert.Equal(new[] { "Hello", "World", "World again" }, paragraphs.ConvertAll(p => p.Text));
    }

    /// <summary>
    /// A pop-on caption that repeats the previous one and adds more text (EOC with no erase in
    /// between) is a new cue - only a screen growing by one character pair continues a cue.
    /// </summary>
    [Fact]
    public void BackToBackPopOnWithSamePrefixStaysSeparate()
    {
        var paragraphs = new List<Paragraph>();
        Cea608CueBuilder.Add(paragraphs, "YES.", 1000, 2000);
        Cea608CueBuilder.Add(paragraphs, "YES. I AGREE.", 2000, 3000);

        Assert.Equal(2, paragraphs.Count);
        Assert.Equal("YES.", paragraphs[0].Text);
        Assert.Equal(2000, paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal("YES. I AGREE.", paragraphs[1].Text);
        Assert.Equal(2000, paragraphs[1].StartTime.TotalMilliseconds);
    }
}
