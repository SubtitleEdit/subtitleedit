using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;

namespace UITests.Features.Video.SpeechToText;

public class OnlineSttSentenceSplitTests
{
    [Fact]
    public void SplitIntoSentences_LatinPunctuation_SplitsAndKeepsTerminators()
    {
        var sentences = SpeechToTextViewModel.SplitIntoSentences("Hello there. How are you? I am fine!");

        Assert.Equal(3, sentences.Count);
        Assert.Equal("Hello there.", sentences[0]);
        Assert.Equal("How are you?", sentences[1]);
        Assert.Equal("I am fine!", sentences[2]);
    }

    [Fact]
    public void SplitIntoSentences_CjkPunctuation_Splits()
    {
        var sentences = SpeechToTextViewModel.SplitIntoSentences("你好。今天天气怎么样？");

        Assert.Equal(2, sentences.Count);
        Assert.Equal("你好。", sentences[0]);
        Assert.Equal("今天天气怎么样？", sentences[1]);
    }

    [Fact]
    public void SplitIntoSentences_NoPunctuation_ReturnsWhole()
    {
        var sentences = SpeechToTextViewModel.SplitIntoSentences("just some words without any punctuation");

        Assert.Single(sentences);
        Assert.Equal("just some words without any punctuation", sentences[0]);
    }

    [Fact]
    public void AddTextAsTimedSentences_DistributesSpanAcrossSentences()
    {
        var subtitle = new Subtitle();
        SpeechToTextViewModel.AddTextAsTimedSentences(subtitle, "One. Two. Three.", 0, 3000);

        Assert.Equal(3, subtitle.Paragraphs.Count);
        // First cue starts at the window start, last cue ends exactly at the window end.
        Assert.Equal(0, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(3000, subtitle.Paragraphs[^1].EndTime.TotalMilliseconds, 1);
        // Cues are contiguous and non-overlapping.
        Assert.True(subtitle.Paragraphs[1].StartTime.TotalMilliseconds >= subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
        Assert.True(subtitle.Paragraphs[2].StartTime.TotalMilliseconds >= subtitle.Paragraphs[1].EndTime.TotalMilliseconds);
    }

    [Fact]
    public void AddTextAsTimedSentences_SingleSentence_OneCueSpanningWindow()
    {
        var subtitle = new Subtitle();
        SpeechToTextViewModel.AddTextAsTimedSentences(subtitle, "A single sentence.", 1000, 5000);

        Assert.Single(subtitle.Paragraphs);
        Assert.Equal(1000, subtitle.Paragraphs[0].StartTime.TotalMilliseconds, 1);
        Assert.Equal(5000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds, 1);
    }
}
