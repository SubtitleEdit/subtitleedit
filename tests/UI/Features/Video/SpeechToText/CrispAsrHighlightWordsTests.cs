using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace UITests.Features.Video.SpeechToText;

// Crisp ASR "Highlight word" (-ml 1 -sow) only gets one word per cue out of crispasr (Parakeet
// included); the post-processor then merged the words back into plain lines, so nothing was
// ever highlighted.
public class CrispAsrHighlightWordsTests
{
    private static ResultText R(string text, decimal start, decimal end) => new() { Text = text, Start = start, End = end };

    [Theory]
    [InlineData("-ml 1 -sow", true)]
    [InlineData("--max-len 1 --split-on-word", true)]
    [InlineData("--vad -ml 1 -sow --print-progress", true)]
    [InlineData("-ml 1", false)]
    [InlineData("-sow", false)]
    [InlineData("-ml 42 -sow", false)]
    [InlineData("-ml 10 -sow", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsActive_NeedsMaxLenOneAndSplitOnWord(string? parameters, bool expected)
    {
        Assert.Equal(expected, CrispAsrHighlightWords.IsActive(parameters));
    }

    [Fact]
    public void Build_UnderlinesEachWordOfItsLine()
    {
        // Real crispasr --backend parakeet -ml 1 -sow output.
        var words = new List<ResultText>
        {
            R("Henry.", 0.08m, 0.64m),
            R("We're", 0.88m, 1.04m), R("here.", 1.04m, 1.20m),
            R("Sorry", 1.92m, 2.32m), R("about", 2.32m, 2.56m), R("that.", 2.56m, 3.04m),
        };

        var result = CrispAsrHighlightWords.Build(words, 86);

        Assert.Equal(new[]
        {
            "<u>Henry.</u>",
            "<u>We're</u> here.",
            "We're <u>here.</u>",
            "<u>Sorry</u> about that.",
            "Sorry <u>about</u> that.",
            "Sorry about <u>that.</u>",
        }, result.Select(p => p.Text));

        // Each copy lasts until the next word lights up, the last one until the line ends.
        Assert.Equal(0.88m, result[1].Start);
        Assert.Equal(1.04m, result[1].End);
        Assert.Equal(1.04m, result[2].Start);
        Assert.Equal(1.20m, result[2].End);
        Assert.Equal(3.04m, result[5].End);
    }

    [Fact]
    public void Build_PunctuationTokenIsShownButNeverHighlightedAlone()
    {
        var words = new List<ResultText>
        {
            R("Hello", 0.0m, 0.3m), R("world", 0.3m, 0.6m), R(".", 0.6m, 0.6m),
        };

        var result = CrispAsrHighlightWords.Build(words, 86);

        Assert.Equal(new[] { "<u>Hello</u> world.", "Hello <u>world</u>." }, result.Select(p => p.Text));
        Assert.Equal(0.6m, result[1].End);
    }

    [Fact]
    public void PostProcessor_DoesNotMergeOrAddPeriodsToHighlightedLines()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("<u>Sorry</u> about that", 0, 400));
        subtitle.Paragraphs.Add(new Paragraph("Sorry <u>about</u> that", 400, 640));
        subtitle.Paragraphs.Add(new Paragraph("Sorry about <u>that</u>", 640, 1100));
        subtitle.Paragraphs.Add(new Paragraph("<u>Next</u> one", 3000, 3400));
        subtitle.Paragraphs.Add(new Paragraph("Next <u>one</u>", 3400, 3800));

        var pp = new SpeechToTextPostProcessor("en") { EngineCommandLineArguments = "-ml 1 -sow" };
        var result = pp.Fix(SpeechToTextPostProcessor.Engine.Whisper, subtitle, true, true, true, false, false, true, false, Avalonia.Media.Colors.Red);

        Assert.Equal(new[]
        {
            "<u>Sorry</u> about that",
            "Sorry <u>about</u> that",
            "Sorry about <u>that</u>",
            "<u>Next</u> one",
            "Next <u>one</u>",
        }, result.Paragraphs.Select(p => p.Text));
    }
}
