using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg;

namespace UITests.Logic;

/// <summary>
/// The subtitle snapshot the ffmpeg player's control draws: tag stripping, whole-line italic,
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
        var text = FfmpegPreviewSubtitle.ToPlainText("<b>Hello</b>" + Environment.NewLine + "{\\an8}world", out var italic);

        Assert.Equal("Hello" + Environment.NewLine + "world", text);
        Assert.False(italic);
    }

    [Fact]
    public void ToPlainText_WholeLineItalic_IsDetected()
    {
        var text = FfmpegPreviewSubtitle.ToPlainText("<i>Hello world</i>", out var italic);

        Assert.Equal("Hello world", text);
        Assert.True(italic);
    }

    [Fact]
    public void ToPlainText_PartialItalic_IsNotWholeLineItalic()
    {
        FfmpegPreviewSubtitle.ToPlainText("<i>Hello</i> world", out var italic);

        Assert.False(italic);
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
