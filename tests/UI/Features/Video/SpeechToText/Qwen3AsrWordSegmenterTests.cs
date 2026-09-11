using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

public class Qwen3AsrWordSegmenterTests
{
    private static Qwen3AsrWord W(string word, double start, double end) => new(word, start, end);

    [Fact]
    public void Chinese_PerCharacterTokens_SplitOnSentencePunctuation_WithoutSpaces()
    {
        // Real qwen3-asr-cli --transcribe-align output for a four-sentence clip (issue #14631):
        // one token per character, punctuation as its own (often zero-length) token.
        var words = new List<Qwen3AsrWord>
        {
            W("今", 0.000, 0.160), W("天", 0.160, 0.480), W("天", 0.480, 0.800), W("气", 0.800, 1.040),
            W("很", 1.040, 1.200), W("好", 1.200, 1.680), W("，", 1.760, 1.760),
            W("我", 1.760, 1.920), W("们", 1.920, 2.080), W("去", 2.080, 2.320), W("公", 2.320, 2.560),
            W("园", 2.560, 2.800), W("散", 2.800, 3.040), W("步", 3.040, 3.200), W("吧", 3.200, 3.440),
            W("。", 3.440, 3.440),
            W("好", 3.600, 3.840), W("的", 3.840, 4.080), W("，", 4.080, 4.320),
            W("我", 4.480, 4.480), W("马", 4.480, 4.640), W("上", 4.640, 4.960), W("就", 4.960, 5.200),
            W("来", 5.200, 5.600), W("。", 5.760, 5.920),
            W("然", 5.920, 6.000), W("后", 6.000, 6.240), W("我", 6.240, 6.400), W("们", 6.400, 6.560),
            W("再", 6.560, 6.800), W("去", 6.800, 7.120), W("吃", 7.120, 7.360), W("饭", 7.360, 7.668),
            W("。", 7.668, 7.668),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        var texts = subtitle.Paragraphs.Select(p => p.Text).ToList();
        Assert.Equal(new[] { "今天天气很好，我们去公园散步吧。", "好的，我马上就来。", "然后我们再去吃饭。" }, texts);
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
        Assert.Equal(3440, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(3600, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
        Assert.Equal(5920, subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
        Assert.Equal(5920, subtitle.Paragraphs[2].StartTime.TotalMilliseconds);
        Assert.Equal(7668, subtitle.Paragraphs[2].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void English_WordsWithAttachedPunctuation_SplitOnSentenceEnd_WithSpaces()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("Hello", 0.0, 0.3), W("world.", 0.3, 0.7),
            W("How", 0.8, 1.0), W("are", 1.0, 1.2), W("you?", 1.2, 1.5),
            W("Fine", 1.6, 1.9), W("thanks", 1.9, 2.2),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Equal(new[] { "Hello world.", "How are you?", "Fine thanks" }, subtitle.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void English_PunctuationAsSeparateToken_GetsNoLeadingSpace()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("Yes", 0.0, 0.3), W(",", 0.3, 0.3), W("sir", 0.4, 0.6), W(".", 0.6, 0.6),
            W("Go", 0.7, 0.9), W("!", 0.9, 0.9),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Equal(new[] { "Yes, sir.", "Go!" }, subtitle.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void DecimalNumber_DoesNotEndSentence()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("Version", 0.0, 0.3), W("3.5", 0.3, 0.6), W("is", 0.6, 0.7), W("out.", 0.7, 1.0),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Version 3.5 is out.", subtitle.Paragraphs[0].Text);
    }

    [Fact]
    public void LongPause_SplitsWithoutPunctuation()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("one", 0.0, 0.3), W("two", 0.3, 0.6),
            W("three", 2.0, 2.3), W("four", 2.3, 2.6),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Equal(new[] { "one two", "three four" }, subtitle.Paragraphs.Select(p => p.Text));
        Assert.Equal(600, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(2000, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void ShortPause_SplitsOnlyAfterClauseBoundary()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("one", 0.0, 0.3), W("two", 1.0, 1.3),            // 0.7 s gap, no punctuation: keep
            W("three,", 1.3, 1.6), W("four", 2.3, 2.6),        // 0.7 s gap after a comma: split
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Equal(new[] { "one two three,", "four" }, subtitle.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void LengthCap_SplitsRunOnText()
    {
        var words = Enumerable.Range(0, 12).Select(i => W("word", i * 0.2, i * 0.2 + 0.2)).ToList();

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words, maxCharsLatin: 20, maxCharsCjk: 10);

        Assert.All(subtitle.Paragraphs, p => Assert.True(p.Text.Length <= 20, p.Text));
        Assert.Equal(12, subtitle.Paragraphs.Sum(p => p.Text.Split(' ').Length));
    }

    [Fact]
    public void LengthCap_UsesCjkCapForIdeographs()
    {
        var words = Enumerable.Range(0, 12).Select(i => W("字", i * 0.2, i * 0.2 + 0.2)).ToList();

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words, maxCharsLatin: 100, maxCharsCjk: 5);

        Assert.All(subtitle.Paragraphs, p => Assert.True(p.Text.Length <= 5, p.Text));
        Assert.Equal(12, subtitle.Paragraphs.Sum(p => p.Text.Length));
    }

    [Fact]
    public void StrayClosingMark_AfterSentenceBreak_AttachesToPreviousCue()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("「", 0.0, 0.0), W("好", 0.0, 0.3), W("。", 0.3, 0.3), W("」", 0.3, 0.5),
            W("走", 0.6, 0.9), W("。", 0.9, 0.9),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Equal(new[] { "「好。」", "走。" }, subtitle.Paragraphs.Select(p => p.Text));
        Assert.Equal(500, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void OpeningMark_AfterSentenceBreak_StartsTheNextCue()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("你", 0.0, 0.2), W("好", 0.2, 0.4), W("。", 0.4, 0.4),
            W("「", 0.5, 0.5), W("我", 0.5, 0.7), W("来", 0.7, 0.9), W("了", 0.9, 1.1), W("。", 1.1, 1.1), W("」", 1.1, 1.2),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Equal(new[] { "你好。", "「我来了。」" }, subtitle.Paragraphs.Select(p => p.Text));
        Assert.Equal(400, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.Equal(500, subtitle.Paragraphs[1].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void DialogDash_AfterSentenceBreak_StartsTheNextCue()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("-", 0.0, 0.0), W("Hi.", 0.0, 0.3),
            W("-", 0.5, 0.5), W("Hello.", 0.5, 0.9),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Equal(new[] { "- Hi.", "- Hello." }, subtitle.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void OpeningCurlyQuote_AfterSentenceBreak_StartsTheNextCue()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("He", 0.0, 0.2), W("said.", 0.2, 0.4),
            W("“", 0.5, 0.5), W("Go", 0.5, 0.7), W("home.", 0.7, 0.9), W("”", 0.9, 1.0),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Equal(new[] { "He said.", "“Go home.”" }, subtitle.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void NewlineInsideToken_ForcesBreak()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("first", 0.0, 0.3), W("part\n", 0.3, 0.6), W("second", 0.6, 0.9),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Equal(new[] { "first part", "second" }, subtitle.Paragraphs.Select(p => p.Text));
    }

    [Fact]
    public void EmptyAndWhitespaceTokens_AreIgnored()
    {
        var words = new List<Qwen3AsrWord>
        {
            W("", 0.0, 0.0), W("  ", 0.0, 0.1), W("hi", 0.1, 0.3), W("", 0.3, 0.3),
        };

        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(words);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal("hi", subtitle.Paragraphs[0].Text);
        Assert.Equal(100, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
    }

    [Fact]
    public void NoWords_ProducesEmptySubtitle()
    {
        var subtitle = Qwen3AsrWordSegmenter.BuildSubtitle(new List<Qwen3AsrWord>());

        Assert.Empty(subtitle.Paragraphs);
    }
}
