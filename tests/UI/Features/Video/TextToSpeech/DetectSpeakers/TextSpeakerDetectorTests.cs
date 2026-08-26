using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DetectSpeakers;

namespace UITests.Features.Video.TextToSpeech.DetectSpeakers;

public class TextSpeakerDetectorTests
{
    [Theory]
    [InlineData("MIKE: Text", "MIKE", "Text")]
    [InlineData("Joe: How are you?", "Joe", "How are you?")]
    [InlineData("[NARRATOR] Once upon a time.", "NARRATOR", "Once upon a time.")]
    [InlineData("(Speaker 1) Hello.", "Speaker 1", "Hello.")]
    [InlineData("Mr. SMITH: Yes.", "Mr. SMITH", "Yes.")]
    public void ASpeakerTagIsReadAndRemoved(string line, string expectedSpeaker, string expectedText)
    {
        Assert.True(TextSpeakerDetector.TrySplit(line, hasFollowingLine: false, out var speaker, out var text));
        Assert.Equal(expectedSpeaker, speaker);
        Assert.Equal(expectedText, text);
    }

    [Fact]
    public void AColonTagAloneOnItsLineIsASpeakerWhenSpeechFollows()
    {
        Assert.True(TextSpeakerDetector.TrySplit("NARRATOR:", hasFollowingLine: true, out var speaker, out var text));
        Assert.Equal("NARRATOR", speaker);
        Assert.Equal(string.Empty, text);
    }

    /// <summary>"[door slams]" alone is a sound annotation - the noise prompt's business.</summary>
    [Fact]
    public void ABracketAloneOnASingleLineIsNotASpeaker()
    {
        Assert.False(TextSpeakerDetector.TrySplit("[door slams]", hasFollowingLine: false, out _, out _));
    }

    [Theory]
    [InlineData("Meet me at 3:30.")]
    [InlineData("Just some text.")]
    [InlineData(": no name at all")]
    [InlineData("")]
    public void PlainTextIsNotASpeaker(string line)
    {
        Assert.False(TextSpeakerDetector.TrySplit(line, hasFollowingLine: false, out _, out _));
    }

    /// <summary>SDH names ("NARRATOR", "Speaker 1") are pre-checked; "Warning:" is for the user to judge.</summary>
    [Theory]
    [InlineData("NARRATOR", true)]
    [InlineData("MIKE", true)]
    [InlineData("Speaker 1", true)]
    [InlineData("speaker_02", true)]
    [InlineData("Warning", false)]
    [InlineData("Joe", false)]
    public void OnlySdhStyleNamesAreConfident(string name, bool expected)
    {
        Assert.Equal(expected, TextSpeakerDetector.IsConfidentSpeakerName(name));
    }

    /// <summary>The exact shape from issue #14106: tags name only the speaker changes.</summary>
    [Fact]
    public void ApplyWithStickySpeakersCarriesTheSpeakerToUntaggedLines()
    {
        var subtitle = MakeIssueExample();
        var confirmed = TextSpeakerDetector.Detect(subtitle);

        var applied = TextSpeakerDetector.Apply(subtitle, confirmed, stickySpeakers: true);

        Assert.Equal(2, applied);
        Assert.Equal("NARRATOR", subtitle.Paragraphs[0].Actor);
        Assert.Equal("Text", subtitle.Paragraphs[0].Text);
        Assert.Equal("NARRATOR", subtitle.Paragraphs[1].Actor);
        Assert.Equal("MIKE", subtitle.Paragraphs[2].Actor);
        Assert.Equal("Text3", subtitle.Paragraphs[2].Text);
        Assert.Equal("MIKE", subtitle.Paragraphs[3].Actor);
    }

    [Fact]
    public void ApplyWithoutStickySpeakersLeavesUntaggedLinesAlone()
    {
        var subtitle = MakeIssueExample();
        var confirmed = TextSpeakerDetector.Detect(subtitle);

        TextSpeakerDetector.Apply(subtitle, confirmed, stickySpeakers: false);

        Assert.Equal("NARRATOR", subtitle.Paragraphs[0].Actor);
        Assert.True(string.IsNullOrEmpty(subtitle.Paragraphs[1].Actor));
        Assert.Equal("MIKE", subtitle.Paragraphs[2].Actor);
        Assert.True(string.IsNullOrEmpty(subtitle.Paragraphs[3].Actor));
    }

    /// <summary>
    /// Two speakers tagged inside one paragraph: the actor can only be one (the first), both tags
    /// leave the text, and stickiness continues from the last tag.
    /// </summary>
    [Fact]
    public void TwoSpeakersInOneParagraphKeepTheFirstAsActor()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("JOE: Hi" + Environment.NewLine + "JANE: Hello", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("How are you?", 2000, 3000));
        var confirmed = TextSpeakerDetector.Detect(subtitle);

        TextSpeakerDetector.Apply(subtitle, confirmed, stickySpeakers: true);

        Assert.Equal("JOE", subtitle.Paragraphs[0].Actor);
        Assert.Equal("Hi" + Environment.NewLine + "Hello", subtitle.Paragraphs[0].Text);
        Assert.Equal("JANE", subtitle.Paragraphs[1].Actor);
    }

    private static Subtitle MakeIssueExample()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("NARRATOR:" + Environment.NewLine + "Text", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("Text2", 2000, 3000));
        subtitle.Paragraphs.Add(new Paragraph("MIKE: Text3", 4000, 5000));
        subtitle.Paragraphs.Add(new Paragraph("More text", 6000, 7000));
        return subtitle;
    }
}
