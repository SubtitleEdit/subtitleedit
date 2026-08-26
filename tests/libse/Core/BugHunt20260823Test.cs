using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Common.TextEffect;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;

namespace LibSETests.Core;

public class BugHunt20260823Test
{
    [Fact]
    public void ActorConverter_ColonSource_AppliesColor()
    {
        var c = new ActorConverter(new SubRip(), "en") { ToSquare = true };
        var p = new Paragraph { Text = "Joe: How are you?" };
        var result = c.FixActorsFromBeforeColon(p, ':', null, SKColors.Red);
        Assert.StartsWith("<font color=\"#ff0000", result.Paragraph.Text);
        Assert.EndsWith("\">[Joe]</font> How are you?", result.Paragraph.Text);
    }

    [Fact]
    public void ActorConverter_ColonSource_LeadingWhitespace()
    {
        var c = new ActorConverter(new SubRip(), "en") { ToSquare = true };
        var p = new Paragraph { Text = "   Joe: How are you?" };
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("[Joe] How are you?", result.Paragraph.Text);
    }

    [Fact]
    public void ActorConverter_ColonSource_ToActor_SetsActor()
    {
        var c = new ActorConverter(new SubRip(), "en") { ToActor = true };
        var p = new Paragraph { Text = "Joe: How are you?" };
        var result = c.FixActorsFromBeforeColon(p, ':', null, null);
        Assert.Equal("How are you?", result.Paragraph.Text);
        Assert.Equal("Joe", result.Paragraph.Actor);
    }

    [Fact]
    public void ActorConverter_TitleIsAllowedInActorName()
    {
        var dataDir = Path.Combine(Path.GetTempPath(), "SeBugHunt20260823_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(dataDir, "Dictionaries"));
        File.WriteAllText(Path.Combine(dataDir, "Dictionaries", "names.xml"), "<names><name>John</name><blacklist></blacklist></names>");
        var oldDataDir = Configuration.DataDirectory;
        Configuration.DataDirectory = dataDir;
        try
        {
            var c = new ActorConverter(new SubRip(), "en") { ToSquare = true };
            Assert.True(c.FixActors(new Paragraph { Text = "[John] How are you?" }, '[', ']', null, null).Selected);
            Assert.True(c.FixActors(new Paragraph { Text = "[Mr. John] How are you?" }, '[', ']', null, null).Selected);
            Assert.False(c.FixActors(new Paragraph { Text = "[Mr. 3] How are you?" }, '[', ']', null, null).Selected);
        }
        finally
        {
            Configuration.DataDirectory = oldDataDir;
            Directory.Delete(dataDir, true);
        }
    }

    [Fact]
    public void FixCasing_IIf_NoDoubleSpace()
    {
        var fc = new FixCasing("en") { FixNormal = true, Format = new SubRip() };
        var s = new Subtitle(new List<Paragraph> { new Paragraph("I-if you say so.", 0, 2000) });
        fc.Fix(s);
        Assert.Equal("I-If you say so.", s.Paragraphs[0].Text);
    }

    [Fact]
    public void FixStutter_AtEndOfText()
    {
        Assert.Equal("N-N-No", FixCasing.FixStutter("N-n-no"));
        Assert.Equal("N-No", FixCasing.FixStutter("N-no"));
    }

    [Fact]
    public void UnknownSubtitle33_SecondsRoundingCarries()
    {
        var sub = new Subtitle();
        sub.Paragraphs.Add(new Paragraph("Hello", new TimeCode(0, 0, 59, 600).TotalMilliseconds, new TimeCode(0, 1, 2, 0).TotalMilliseconds));
        Assert.StartsWith("00:01:00", new UnknownSubtitle33().ToText(sub, "t"));
        Assert.StartsWith("00:01:00", new UnknownSubtitle34().ToText(sub, "t"));
        Assert.StartsWith("00:01:00", new UnknownSubtitle59().ToText(sub, "t"));
    }

    [Fact]
    public void KaraokeWordTransform_TagAfterSpaceIsKeptIntact()
    {
        var res = new KaraokeWordTransform().Transform("<font color=\"#ffffff\">Hello <font face=\"Arial\">world</font>");
        Assert.Equal(2, res.Length);
        Assert.Equal("<font color=\"#ffffff\">Hello</font> <font face=\"Arial\">world</font>", res[0]);
        Assert.Equal("<font color=\"#ffffff\">Hello <font face=\"Arial\">world</font></font>", res[1]);
    }

    [Fact]
    public void KaraokeWordTransform_MultipleSpaces()
    {
        var res = new KaraokeWordTransform().Transform("a  b c");
        Assert.Equal(new[] { "a</font>  b c", "a  b</font> c", "a  b c</font>" }, res);
    }
}
