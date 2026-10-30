using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Video.OpenFromUrl;
using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

namespace UITests.Logic;

/// <summary>
/// The subtitle snapshot the ffmpeg player's control draws: tag stripping, italic and bold runs,
/// active-line lookup, secondary marking and the SMPTE stretch.
/// </summary>
public class FfmpegPreviewSubtitleTests
{
    private static Subtitle MakeSubtitle(params (double start, double end, string text)[] lines)
    {
        var subtitle = new Subtitle();
        foreach (var (start, end, text) in lines)
        {
            subtitle.Paragraphs.Add(new Paragraph(text, start * 1000.0, end * 1000.0));
        }

        return subtitle;
    }

    [Fact]
    public void ToPlainText_StripsTagsAndKeepsLineBreaks()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("<font face=\"Arial\">Hello</font>" + Environment.NewLine + "{\\an8}world", out var runs, out _);

        Assert.Equal("Hello" + Environment.NewLine + "world", text);
        Assert.Empty(runs);
    }

    [Fact]
    public void ToPlainText_WholeLineItalic_IsOneRun()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("<i>Hello world</i>", out var runs, out _);

        Assert.Equal("Hello world", text);
        Assert.Equal([new FfmpegPreviewSubtitle.StyleRun(0, 11, true, false)], runs);
    }

    [Fact]
    public void ToPlainText_PartialItalic_CoversOnlyTheTaggedWords()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("<i>Hello</i> world", out var runs, out _);

        Assert.Equal("Hello world", text);
        Assert.Equal([new FfmpegPreviewSubtitle.StyleRun(0, 5, true, false)], runs);
    }

    [Fact]
    public void ToPlainText_ItalicOnEachLine_KeepsOneRunPerLine()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("<i>one</i>\\N<i>two</i>", out var runs, out _);

        Assert.Equal("one" + Environment.NewLine + "two", text);
        Assert.Equal(
            [
                new FfmpegPreviewSubtitle.StyleRun(0, 3, true, false),
                new FfmpegPreviewSubtitle.StyleRun(3 + Environment.NewLine.Length, 3, true, false),
            ],
            runs);
    }

    [Fact]
    public void ToPlainText_AssTags_FollowOnOffAndWeight()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("{\\an8\\i1}one {\\i0\\b700}two{\\b0} three", out var runs, out _);

        Assert.Equal("one two three", text);
        Assert.Equal(
            [
                new FfmpegPreviewSubtitle.StyleRun(0, 4, true, false),
                new FfmpegPreviewSubtitle.StyleRun(4, 3, false, true),
            ],
            runs);
    }

    [Fact]
    public void ToPlainText_TagsThatOnlyLookLikeStyle_AreIgnored()
    {
        FfmpegPreviewSubtitle.ToPlainText("{\\blur2\\bord3\\be1\\iclip(0,0,1,1)}Hello", out var runs, out _);

        Assert.Empty(runs);
    }

    [Fact]
    public void ToPlainText_LeadingSpace_ShiftsRuns()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("  <b>Hello</b>  ", out var runs, out _);

        Assert.Equal("Hello", text);
        Assert.Equal([new FfmpegPreviewSubtitle.StyleRun(0, 5, false, true)], runs);
    }

    [Fact]
    public void ToPlainText_HardSpace_BecomesSpace()
    {
        Assert.Equal("a b", FfmpegPreviewSubtitle.ToPlainText("a\\hb", out _, out _));
    }

    [Theory]
    [InlineData("{\\p1}m 0 0 l 100 0 100 100 0 100{\\p0}")]
    [InlineData("{\\an7\\pos(10,10)\\p2}m 0 0 b 10 0 20 10 20 20 s 30 30 40 40 c")]
    public void ToPlainText_Drawing_LeavesNothing(string text)
    {
        Assert.Equal(string.Empty, FfmpegPreviewSubtitle.ToPlainText(text, out _, out _));
    }

    [Fact]
    public void ToPlainText_TextAfterDrawing_IsKept()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("{\\p1}m 0 0 l 10 0 10 10{\\p0} Hello", out _, out _);

        Assert.Equal("Hello", text);
    }

    [Fact]
    public void ToPlainText_PositionTag_IsNotADrawing()
    {
        Assert.Equal("Hello", FfmpegPreviewSubtitle.ToPlainText("{\\pos(10,10)}Hello", out _, out _));
    }

    [Theory]
    [InlineData("{\\an8}Hello", 8)]
    [InlineData("{\\an8}{\\an2}Hello", 8)]
    [InlineData("Hello", null)]
    [InlineData("{\\an7\\pos(10,10)}Hello", null)]
    [InlineData("{\\an7\\move(1,2,3,4)}Hello", null)]
    public void ToPlainText_Alignment(string text, int? expected)
    {
        FfmpegPreviewSubtitle.ToPlainText(text, out _, out var alignment);

        Assert.Equal(expected, alignment);
    }

    [Fact]
    public void ToPlainText_FontColor_IsARun_AndClosesToTheOuterColor()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("<font color=\"#ff0000\">a<font color=\"#00ff00\">b</font>c</font>d", out var runs, out _);

        Assert.Equal("abcd", text);
        Assert.Equal(
            [
                new FfmpegPreviewSubtitle.StyleRun(0, 1, false, false, Colors.Red),
                new FfmpegPreviewSubtitle.StyleRun(1, 1, false, false, Color.Parse("#00ff00")),
                new FfmpegPreviewSubtitle.StyleRun(2, 1, false, false, Colors.Red),
            ],
            runs);
    }

    [Fact]
    public void ToPlainText_AssColor_IsBlueGreenRed_AndCResets()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("{\\c&H0000FF&}red{\\c} plain {\\1c&HFF0000&}blue", out var runs, out _);

        Assert.Equal("red plain blue", text);
        Assert.Equal(
            [
                new FfmpegPreviewSubtitle.StyleRun(0, 3, false, false, Colors.Red),
                new FfmpegPreviewSubtitle.StyleRun(10, 4, false, false, Color.Parse("#0000ff")),
            ],
            runs);
    }

    [Fact]
    public void ToPlainText_ClipIsNotAColor()
    {
        FfmpegPreviewSubtitle.ToPlainText("{\\clip(0,0,1,1)\\3c&H0000FF&}Hello", out var runs, out _);

        Assert.Empty(runs);
    }

    [Fact]
    public void Build_SecondaryHeaderStyle_IsScaledToPreviewUnits()
    {
        var style = new SsaStyle { Name = "Second", FontName = "Georgia", FontSize = 150, Alignment = "4", MarginVertical = 75, OutlineWidth = 15, ShadowWidth = 0, Bold = true, BorderStyle = "3" };
        var secondary = MakeSubtitle((1, 3, "second"));
        SecondarySubtitleStyler.SetHeader(secondary, style, 1920, 1080);
        secondary.Paragraphs[0].Extra = "Second";

        var result = FfmpegPreviewSubtitle.Build(MakeSubtitle((1, 3, "main")), secondary, false).SecondaryStyle;

        Assert.NotNull(result);
        Assert.Equal("Georgia", result.FontName);
        Assert.Equal(150m * 288 / 1080, result.FontSize, 3);
        Assert.Equal(4, result.Alignment);
        Assert.Equal(3, result.BorderStyle);
        Assert.True(result.Bold);
    }

    [Fact]
    public void Build_SecondaryWithoutStyle_HasNoSecondaryStyle()
    {
        var preview = FfmpegPreviewSubtitle.Build(MakeSubtitle((1, 3, "main")), MakeSubtitle((1, 3, "second")), false);

        Assert.Null(preview.SecondaryStyle);
    }

    [Fact]
    public void Build_DrawingOnlyLine_IsSkipped()
    {
        var preview = FfmpegPreviewSubtitle.Build(MakeSubtitle((1, 3, "{\\p1}m 0 0 b 1 1 2 2 3 3{\\p0}"), (1, 3, "text")), null, false);

        Assert.Equal(["text"], preview.GetActive(2).Select(l => l.Text));
    }

    [Fact]
    public void GetActive_ReturnsLinesCoveringThePosition()
    {
        var preview = FfmpegPreviewSubtitle.Build(MakeSubtitle((1, 3, "one"), (2, 4, "two"), (10, 12, "ten")), null, false);

        Assert.Equal(["one", "two"], preview.GetActive(2.5).Select(l => l.Text));
        Assert.Equal(["two"], preview.GetActive(3.5).Select(l => l.Text));
        Assert.Empty(preview.GetActive(5));
        Assert.Empty(preview.GetActive(12)); // end is exclusive
    }

    [Fact]
    public void GetActive_FindsLongLineStartedBeforeManyShortOnes()
    {
        var lines = new List<(double, double, string)> { (0, 100, "long") };
        for (var i = 1; i < 90; i++)
        {
            lines.Add((i, i + 0.5, "short" + i));
        }

        var preview = FfmpegPreviewSubtitle.Build(MakeSubtitle(lines.ToArray()), null, false);

        Assert.Equal(["long", "short50"], preview.GetActive(50.2).Select(l => l.Text));
        Assert.Equal(["long"], preview.GetActive(95).Select(l => l.Text));
        Assert.Empty(preview.GetActive(100));
    }

    [Fact]
    public void GetActive_IncludesAllLinesStartingExactlyAtThePosition()
    {
        var preview = FfmpegPreviewSubtitle.Build(MakeSubtitle((1, 2, "before"), (5, 6, "a"), (5, 7, "b"), (5, 8, "c"), (9, 10, "after")), null, false);

        Assert.Equal(["a", "b", "c"], preview.GetActive(5).Select(l => l.Text).Order());
        Assert.Empty(preview.GetActive(0.5));
        Assert.Empty(FfmpegPreviewSubtitle.Empty.GetActive(1));
    }

    [Fact]
    public void Build_MarksSecondaryLines()
    {
        var preview = FfmpegPreviewSubtitle.Build(MakeSubtitle((1, 3, "main")), MakeSubtitle((1, 3, "second")), false);

        var active = preview.GetActive(2);
        Assert.Equal(2, active.Count);
        Assert.Contains(active, l => l.Text == "main" && !l.Secondary);
        Assert.Contains(active, l => l.Text == "second" && l.Secondary);
    }

    [Fact]
    public void Build_SmpteMode_StretchesTimes()
    {
        var preview = FfmpegPreviewSubtitle.Build(MakeSubtitle((1000, 1001, "late")), null, true);

        Assert.Empty(preview.GetActive(1000.5)); // 1000 s became 1001 s
        var line = Assert.Single(preview.GetActive(1001.5));
        Assert.Equal(1001.0, line.StartSeconds, 3);
    }

    [Fact]
    public void Build_SkipsEmptyAndZeroLengthLines()
    {
        var preview = FfmpegPreviewSubtitle.Build(MakeSubtitle((1, 3, "<i></i>"), (4, 4, "zero")), null, false);

        Assert.Equal(0, preview.Count);
    }
}
