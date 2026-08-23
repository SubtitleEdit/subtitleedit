using Nikse.SubtitleEdit.Core.Common;

namespace LibSETests.Common;

public class NetflixImsc11JapaneseToAssTest
{
    private static List<string> DialogueLines(Subtitle subtitle, int width = 1280, int height = 720)
    {
        return NetflixImsc11JapaneseToAss.Convert(subtitle, width, height)
            .SplitToLines()
            .Where(l => l.StartsWith("Dialogue:", StringComparison.Ordinal))
            .ToList();
    }

    private static Subtitle SubtitleWith(string text)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 1000, 3000));
        return subtitle;
    }

    [Fact]
    public void RubyBecomesItsOwnRenderLineAboveTheBase()
    {
        var lines = DialogueLines(SubtitleWith("（バシン）<ruby-container><ruby-base>遅</ruby-base><ruby-text>おく</ruby-text></ruby-container>れた～"));

        Assert.Equal(2, lines.Count);

        // The furigana line repeats the preceding text invisibly so libass advances the pen for us,
        // then draws the reading at half size.
        Assert.Contains(@"{\alpha&FF&}（バシン）{\alpha&0&}", lines[0], StringComparison.Ordinal);
        Assert.EndsWith(@"{\fs20}おく", lines[0], StringComparison.Ordinal);

        // The base line keeps the base text and drops the reading.
        Assert.EndsWith("（バシン）遅れた～", lines[1], StringComparison.Ordinal);

        // Both sit at the same x, one line height apart, clear of the bottom edge.
        Assert.Contains(@"{\pos(460,656)}", lines[0], StringComparison.Ordinal);
        Assert.Contains(@"{\pos(460,690)}", lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public void VerticalCueIsLaidOutOneCharacterPerLine()
    {
        var lines = DialogueLines(SubtitleWith(@"{\an9}（兵士）ハッ…"));

        var line = Assert.Single(lines);
        Assert.Contains(@"{\an9\pos(", line, StringComparison.Ordinal);

        // Brackets, the prolonged sound mark and the ellipsis have dedicated vertical glyphs.
        Assert.EndsWith(@"︵\N兵\N士\N︶\Nハ\Nッ\N⋮", line, StringComparison.Ordinal);
    }

    [Fact]
    public void BoutenBecomesAnEmphasisMarkPerCharacter()
    {
        var lines = DialogueLines(SubtitleWith("これは<bouten-filled-sesame-outside>強調</bouten-filled-sesame-outside>です"));

        // One mark line per emphasized character, plus the text itself.
        Assert.Equal(3, lines.Count);
        Assert.EndsWith(@"{\alpha&FF&}これは{\alpha&0&}﹅", lines[0], StringComparison.Ordinal);
        Assert.EndsWith(@"{\alpha&FF&}これは強{\alpha&0&}﹅", lines[1], StringComparison.Ordinal);
        Assert.EndsWith("これは強調です", lines[2], StringComparison.Ordinal);
    }

    [Fact]
    public void NoJapaneseProfileTagSurvivesIntoTheAss()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<ruby-container><ruby-base>私</ruby-base><ruby-text>わたし</ruby-text></ruby-container>は", 1000, 3000));
        subtitle.Paragraphs.Add(new Paragraph(@"{\an7}<horizontalDigit>12</horizontalDigit>時", 4000, 6000));
        subtitle.Paragraphs.Add(new Paragraph("<bouten-auto>だめ</bouten-auto>", 7000, 9000));

        var raw = NetflixImsc11JapaneseToAss.Convert(subtitle, 1280, 720);

        // Anything left behind is drawn by libass as literal text - that is the bug in issue #13861.
        Assert.DoesNotContain("<ruby", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("<bouten", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("<horizontalDigit", raw, StringComparison.Ordinal);
    }

    [Fact]
    public void ItalicBecomesAnAssOverrideTag()
    {
        var lines = DialogueLines(SubtitleWith("<i>（ナレーション）</i>"));

        var line = Assert.Single(lines);
        Assert.EndsWith(@"{\i1}（ナレーション）{\i0}", line, StringComparison.Ordinal);
    }

    [Fact]
    public void HeaderCarriesThePlayResolutionAndAScaledFont()
    {
        var raw = NetflixImsc11JapaneseToAss.Convert(SubtitleWith("テスト"), 1920, 1080);

        Assert.Contains("PlayResX: 1920", raw, StringComparison.Ordinal);
        Assert.Contains("PlayResY: 1080", raw, StringComparison.Ordinal);

        // Subtitle Edit 4's 40 pixel font was tuned against 720p; 1080p scales to 60.
        Assert.Contains("Style: Default,Arial,60,", raw, StringComparison.Ordinal);
    }

    [Fact]
    public void MultipleLinesShareOneLeftEdge()
    {
        var lines = DialogueLines(SubtitleWith("短い" + Environment.NewLine + "ずっと長い行のテキスト"));

        Assert.Equal(2, lines.Count);

        // multiRowAlign="start": the block is centered on its widest line and both lines start there.
        var x = @"{\pos(420,";
        Assert.Contains(x, lines[0], StringComparison.Ordinal);
        Assert.Contains(x, lines[1], StringComparison.Ordinal);
    }

    [Fact]
    public void PlainCueIsCenteredNearTheBottom()
    {
        var lines = DialogueLines(SubtitleWith("こんにちは"));

        var line = Assert.Single(lines);

        // Five full width characters at font size 40 = 200 wide, centered in 1280, one row up
        // from the bottom margin.
        Assert.Contains(@"{\an1}{\pos(540,690)}", line, StringComparison.Ordinal);
    }
}
