using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.AutoCast;

namespace UITests.Features.Video.TextToSpeech.AutoCast;

/// <summary>
/// Reading the speaker a diarizing engine writes in front of each line - and, just as important,
/// not reading one where there is none: a wrongly stripped line loses words, and a wrongly
/// detected speaker renames somebody's dialogue.
/// </summary>
public class SpeakerLabelParserTests
{
    [Theory]
    [InlineData("(Speaker 1) Hello there.", "Speaker 1", "Hello there.")]
    [InlineData("[SPEAKER 2] Hello there.", "Speaker 2", "Hello there.")]
    [InlineData("SPEAKER_00: Hello there.", "Speaker 0", "Hello there.")]
    [InlineData("Speaker 3: Hello there.", "Speaker 3", "Hello there.")]
    [InlineData("  ( speaker  4 )   Hello there.", "Speaker 4", "Hello there.")]
    [InlineData("[Speaker-5] Hello there.", "Speaker 5", "Hello there.")]
    public void ASpeakerLabelIsReadAndRemoved(string line, string expectedSpeaker, string expectedText)
    {
        Assert.True(SpeakerLabelParser.TrySplit(line, out var speaker, out var text));
        Assert.Equal(expectedSpeaker, speaker);
        Assert.Equal(expectedText, text);
    }

    [Theory]
    [InlineData("The speaker 1 was broken.")]         // mentions a speaker, is not labelled with one
    [InlineData("Hello (Speaker 1) there.")]          // a label only counts at the very start
    [InlineData("(Narrator) Hello there.")]           // not the diarization format
    [InlineData("")]
    [InlineData(null)]
    public void ALineWithoutALabelIsLeftAlone(string? line)
    {
        Assert.False(SpeakerLabelParser.TrySplit(line, out var speaker, out var text));
        Assert.Equal(string.Empty, speaker);
        Assert.Equal(line ?? string.Empty, text);
    }

    [Fact]
    public void TheSameSpeakerWrittenThreeWaysIsOneSpeaker()
    {
        // SPEAKER_00 and "speaker 0" come from different engines and mean the same person; the
        // actor field has to spell them the same or the cast gets two rows for one voice.
        SpeakerLabelParser.TrySplit("SPEAKER_00: a", out var underscored, out _);
        SpeakerLabelParser.TrySplit("(speaker 0) b", out var spaced, out _);
        SpeakerLabelParser.TrySplit("[Speaker-0] c", out var dashed, out _);

        Assert.Equal(underscored, spaced);
        Assert.Equal(spaced, dashed);
    }

    [Fact]
    public void LabelsMoveOutOfTheTextAndIntoTheActorField()
    {
        // Left in the text, every TTS engine would read "Speaker 1" out loud before the line.
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph("(Speaker 1) Hello.", 0, 1000));
        subtitle.Paragraphs.Add(new Paragraph("(Speaker 2) Goodbye.", 1000, 2000));
        subtitle.Paragraphs.Add(new Paragraph("No label here.", 2000, 3000));

        var moved = SpeakerLabelParser.MoveLabelsToActors(subtitle);

        Assert.Equal(2, moved);
        Assert.Equal("Speaker 1", subtitle.Paragraphs[0].Actor);
        Assert.Equal("Hello.", subtitle.Paragraphs[0].Text);
        Assert.Equal("Speaker 2", subtitle.Paragraphs[1].Actor);
        Assert.Equal("No label here.", subtitle.Paragraphs[2].Text);
        Assert.True(string.IsNullOrEmpty(subtitle.Paragraphs[2].Actor));
    }

    [Fact]
    public void AnExistingLineGetsTheSpeakerItOverlapsMost()
    {
        // The subtitle being dubbed is the user's own (often a translation), so its lines rarely
        // line up with the diarized segments - whoever talks for most of the line owns it.
        var lines = new List<Paragraph> { new(string.Empty, 1000, 3000) };
        var segments = new List<Paragraph>
        {
            new("(Speaker 1) a", 0, 1500),      // 500 ms of the line
            new("(Speaker 2) b", 1500, 3200),   // 1500 ms of the line
        };

        var assigned = SpeakerLabelParser.AssignSpeakersByOverlap(lines, segments);

        Assert.Equal("Speaker 2", assigned[lines[0]]);
    }

    [Fact]
    public void ALineOverlappingNothingIsLeftWithoutASpeaker()
    {
        // Music, on-screen text, or a line before anyone speaks: guessing an actor there would
        // hand somebody's cloned voice to a line they never said.
        var lines = new List<Paragraph> { new(string.Empty, 10_000, 12_000) };
        var segments = new List<Paragraph> { new("(Speaker 1) a", 0, 1500) };

        var assigned = SpeakerLabelParser.AssignSpeakersByOverlap(lines, segments);

        Assert.Empty(assigned);
    }

    [Fact]
    public void SegmentsWithoutLabelsAssignNobody()
    {
        var lines = new List<Paragraph> { new(string.Empty, 0, 2000) };
        var segments = new List<Paragraph> { new("Just text", 0, 2000) };

        Assert.Empty(SpeakerLabelParser.AssignSpeakersByOverlap(lines, segments));
    }
}
