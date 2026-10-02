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

    /// <summary>
    /// An extended character replaces the standard one sent right before it (' then ’) - the
    /// line is still being written, so it is no new cue (it was a 67 ms flash cue).
    /// </summary>
    [Fact]
    public void ExtendedCharacterReplacingPreviousGrowsCue()
    {
        var paragraphs = new List<Paragraph>();
        Cea608CueBuilder.Add(paragraphs, "SO,\nIT", 5000, 5033);
        Cea608CueBuilder.Add(paragraphs, "SO,\nIT'", 5033, 5066);
        Cea608CueBuilder.Add(paragraphs, "SO,\nIT’", 5066, 5100);
        Cea608CueBuilder.Add(paragraphs, "SO,\nIT’S", 5100, 6000);

        var paragraph = Assert.Single(paragraphs);
        Assert.Equal("SO,\nIT’S", paragraph.Text);
        Assert.Equal(5000, paragraph.StartTime.TotalMilliseconds);
        Assert.Equal(6000, paragraph.EndTime.TotalMilliseconds);
    }

    /// <summary>
    /// Only an extended character replacing the last char continues the cue - a short caption
    /// followed by another one that happens to share all but its last char is a new cue.
    /// </summary>
    [Theory]
    [InlineData("♪", "Hey!")]
    [InlineData("OK", "Oh!")]
    public void ShortCaptionFollowedByDifferentCaptionIsNewCue(string first, string second)
    {
        var paragraphs = new List<Paragraph>();
        Cea608CueBuilder.Add(paragraphs, first, 1000, 2000);
        Cea608CueBuilder.Add(paragraphs, second, 2000, 4000);

        Assert.Equal(new[] { first, second }, paragraphs.Select(p => p.Text));
        Assert.Equal(new double[] { 1000, 2000 }, paragraphs.Select(p => p.StartTime.TotalMilliseconds));
    }

    /// <summary>
    /// After a roll-up carriage return only the scrolled rows are on screen for the few frames
    /// until the next row's first characters arrive - that belongs to the next cue instead of
    /// flashing by on its own. A longer pause stays a cue of its own.
    /// </summary>
    [Fact]
    public void ShortRollUpGapFoldsIntoNextRowButPauseStays()
    {
        var paragraphs = new List<Paragraph>();
        Cea608CueBuilder.Add(paragraphs, "JUSTICE\nIT’S AN HONOR", 5000, 8322);
        Cea608CueBuilder.Add(paragraphs, "IT’S AN HONOR", 8322, 8455);
        Cea608CueBuilder.Add(paragraphs, "IT’S AN HONOR\nWI", 8455, 8488);
        Cea608CueBuilder.Add(paragraphs, "IT’S AN HONOR\nWITH", 8488, 8522);
        Cea608CueBuilder.Add(paragraphs, "IT’S AN HONOR\nWITH U", 8522, 8555);
        Cea608CueBuilder.Add(paragraphs, "IT’S AN HONOR\nWITH US.", 8555, 9000);
        Cea608CueBuilder.Add(paragraphs, "WITH US.", 9000, 9800);
        Cea608CueBuilder.Add(paragraphs, "WITH US.\nAS", 9800, 10000);

        Assert.Equal(new[] { "JUSTICE\nIT’S AN HONOR", "IT’S AN HONOR\nWITH US.", "WITH US.", "WITH US.\nAS" }, paragraphs.ConvertAll(p => p.Text));
        Assert.Equal(8322, paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(9000, paragraphs[1].EndTime.TotalMilliseconds);
    }
}
