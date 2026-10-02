using Nikse.SubtitleEdit.Core.Cea608;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace LibSETests.Cea608;

public class SerializedScreenTextTest
{
    private static string Decode(params (int a, int b)[] pairs)
    {
        var result = new List<string>();
        var parser = new CcDataC608Parser();
        parser.DisplayScreen += data => result.Add(SerializedScreenText.GetText(data.Screen));
        var t = 0;
        foreach (var (a, b) in pairs)
        {
            parser.AddData(t, new[] { a, b });
            t += 100;
        }

        return Assert.Single(result);
    }

    private static (int, int)[] PopOn(params (int, int)[] body)
    {
        var pairs = new List<(int, int)> { (0x14, 0x20), (0x14, 0x2E) }; // RCL, ENM
        pairs.AddRange(body);
        pairs.Add((0x14, 0x2F)); // EOC
        pairs.Add((0x14, 0x2C)); // EDM
        return pairs.ToArray();
    }

    [Fact]
    public void PlainTextHasNoTags()
    {
        Assert.Equal("Hi", Decode(PopOn((0x14, 0x70), (0x48, 0x69))));
    }

    [Fact]
    public void ItalicPacWrapsRowInItalicTags()
    {
        // PAC row 15, white italics
        Assert.Equal("<i>Hi</i>", Decode(PopOn((0x14, 0x6E), (0x48, 0x69))));
    }

    [Fact]
    public void MidRowItalicsTagOnlyThePartAfterTheCode()
    {
        // "Hi " + mid-row italics + "yo " + mid-row white + "ok"; the closing tag goes before the space
        var text = Decode(PopOn((0x14, 0x70), (0x48, 0x69), (0x20, 0x00), (0x11, 0x2E), (0x79, 0x6F), (0x20, 0x00), (0x11, 0x20), (0x6F, 0x6B)));
        Assert.Equal("Hi <i>yo</i> ok", text);
    }

    /// <summary>
    /// A mid-row code takes up a column and shows as a space - encoders rely on it to separate the
    /// styled word ("that you<i>were</i>smelling" before).
    /// </summary>
    [Fact]
    public void MidRowCodeShowsAsSpace()
    {
        var text = Decode(PopOn((0x14, 0x70), (0x48, 0x69), (0x11, 0x2E), (0x79, 0x6F), (0x11, 0x20), (0x6F, 0x6B)));
        Assert.Equal("Hi <i>yo</i> ok", text);
    }

    [Fact]
    public void MidRowCodeInsideBracketsAddsNoSpace()
    {
        var text = Decode(PopOn((0x14, 0x70), (0x28, 0x00), (0x11, 0x2E), (0x79, 0x6F), (0x11, 0x20), (0x29, 0x00)));
        Assert.Equal("(<i>yo</i>)", text);
    }

    /// <summary>
    /// Control codes are sent twice - the copy right after is skipped, also for mid-row codes (one
    /// space) and special characters (one note).
    /// </summary>
    [Fact]
    public void DoubledMidRowCodeAndSpecialCharAreSkippedOnce()
    {
        var text = Decode(PopOn((0x14, 0x70), (0x48, 0x69), (0x11, 0x2E), (0x11, 0x2E), (0x79, 0x6F), (0x11, 0x37), (0x11, 0x37)));
        Assert.Equal("Hi <i>yo\u266A</i>", text);
    }

    /// <summary>
    /// The same tab offset again after some text is a new command, not the second copy of the first
    /// one - skipping it put the pen a column short, onto the last letter of the word before.
    /// </summary>
    [Fact]
    public void RepeatedTabOffsetAfterTextIsNotSkipped()
    {
        var text = Decode(PopOn((0x14, 0x70), (0x48, 0x69), (0x17, 0x21), (0x79, 0x6F), (0x17, 0x21), (0x6F, 0x6B)));
        Assert.Equal("Hi yo ok", text);
    }

    /// <summary>
    /// A PAC that puts the pen on a written char (the last letter of an italic word) must not
    /// restyle that char - "<i>wer</i>e" before.
    /// </summary>
    [Fact]
    public void PacOnWrittenCharKeepsItsStyle()
    {
        // mid-row italics at column 0, "abcd" at 1-4, PAC column 4 + TO1, mid-row white at 5, "ok"
        var text = Decode(PopOn((0x14, 0x70), (0x11, 0x2E), (0x61, 0x62), (0x63, 0x64), (0x14, 0x72), (0x17, 0x21), (0x11, 0x20), (0x6F, 0x6B)));
        Assert.Equal("<i>abcd</i> ok", text);
    }

    [Fact]
    public void EachRowGetsItsOwnTagsAndNoPadding()
    {
        // row 14 italics "Hi", row 15 plain "yo"
        var text = Decode(PopOn((0x14, 0x4E), (0x48, 0x69), (0x14, 0x70), (0x79, 0x6F)));
        Assert.Equal("<i>Hi</i>" + Environment.NewLine + "yo", text);
    }

    /// <summary>
    /// No mid-row space before closing punctuation - "Hi <i>yo</i> ." before.
    /// </summary>
    [Fact]
    public void MidRowCodeBeforeClosingPunctuationAddsNoSpace()
    {
        Assert.Equal("Hi <i>yo</i>.", Decode(PopOn((0x14, 0x70), (0x48, 0x69), (0x11, 0x2E), (0x79, 0x6F), (0x11, 0x20), (0x2E, 0x00))));
        Assert.Equal("Hi <i>yo</i>, o", Decode(PopOn((0x14, 0x70), (0x48, 0x69), (0x11, 0x2E), (0x79, 0x6F), (0x11, 0x20), (0x2C, 0x20), (0x6F, 0x00))));
    }

    [Fact]
    public void MidRowCodeInsideStraightQuotesAddsNoSpace()
    {
        // "yo" in italics, inside straight quotes
        Assert.Equal("\"<i>yo</i>\"", Decode(PopOn((0x14, 0x70), (0x22, 0x00), (0x11, 0x2E), (0x79, 0x6F), (0x11, 0x20), (0x22, 0x00))));
    }

    [Fact]
    public void MidRowCodeBeforeOpeningQuoteKeepsSpace()
    {
        // Hi + mid-row italics + yo + mid-row white + "ok" - the quote opens a new word
        Assert.Equal("Hi <i>yo</i> \"ok\"", Decode(PopOn((0x14, 0x70), (0x48, 0x69), (0x11, 0x2E), (0x79, 0x6F), (0x11, 0x20), (0x22, 0x6F), (0x6B, 0x22))));
    }

    /// <summary>
    /// Subtitle Edit's own SCC writer puts the mid-row reset right before the next char, so the
    /// decoder must not add a space there.
    /// </summary>
    [Theory]
    [InlineData("see <i>Jaws</i>?")]
    [InlineData("\"<i>Jaws</i>\"")]
    [InlineData("see \"<i>Jaws</i>\" now")]
    [InlineData("<i>Jaws</i>'s shark")]
    [InlineData("(<i>music</i>)")]
    [InlineData("Hi <i>yo</i> ok")]
    [InlineData("Hi <i>yo</i>, ok.")]
    public void SccWriterRoundTrip(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 1000, 3000));
        var scc = new ScenaristClosedCaptions().ToText(subtitle, "test");
        var pairs = new List<(int, int)>();
        foreach (var line in scc.SplitToLines())
        {
            var tab = line.IndexOf('\t');
            if (tab < 0)
            {
                continue;
            }

            foreach (var word in line.Substring(tab + 1).Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                pairs.Add((Convert.ToInt32(word.Substring(0, 2), 16), Convert.ToInt32(word.Substring(2, 2), 16)));
            }
        }

        Assert.Equal(text, Decode(pairs.ToArray()));
    }
}
