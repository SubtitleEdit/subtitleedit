using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// Turning a transcription into cloning ref-text. The transcription comes from whatever the user
/// has configured under Audio to text, so a karaoke setup (faster-whisper
/// <c>--highlight_words</c>, whisper.cpp <c>-owts</c>) emits one cue per word, each repeating the
/// whole sentence with the current word underlined. Joined raw that became kilobytes of tag-laced
/// repetition, which CosyVoice3 rendered instead of the line it was asked to speak.
/// </summary>
public class RefTextFromTranscriptionTests
{
    [Fact]
    public void CollapsesKaraokeWordHighlightCues()
    {
        // Shape faster-whisper --highlight_words produces: same sentence per cue, moving <u>.
        var cues = new[]
        {
            "<u>being</u> a silent person, I said",
            "being <u>a</u> silent person, I said",
            "being a <u>silent</u> person, I said",
            "being a silent <u>person,</u> I said",
            "being a silent person, <u>I</u> said",
            "being a silent person, I <u>said</u>",
        };

        var refText = TextToSpeechViewModel.BuildRefTextFromTranscription(cues);

        Assert.Equal("being a silent person, I said", refText);
    }

    [Fact]
    public void KeepsDistinctSentencesInOrder()
    {
        var cues = new[] { "First sentence.", "Second sentence.", "Third one." };

        var refText = TextToSpeechViewModel.BuildRefTextFromTranscription(cues);

        Assert.Equal("First sentence. Second sentence. Third one.", refText);
    }

    [Fact]
    public void StripsFormattingAndAssaTags()
    {
        var cues = new[] { "<i>Hello</i> {\\an8}there", "<font color=\"#ff0000\">friend</font>" };

        var refText = TextToSpeechViewModel.BuildRefTextFromTranscription(cues);

        Assert.Equal("Hello there friend", refText);
    }

    [Fact]
    public void JoinsMultiLineCuesOntoOneLine()
    {
        var cues = new[] { "first line\nsecond line" };

        var refText = TextToSpeechViewModel.BuildRefTextFromTranscription(cues);

        Assert.Equal("first line second line", refText);
    }

    [Fact]
    public void SkipsEmptyAndWhitespaceCues()
    {
        var cues = new[] { "  ", null, "Real text.", string.Empty, "<i></i>" };

        var refText = TextToSpeechViewModel.BuildRefTextFromTranscription(cues);

        Assert.Equal("Real text.", refText);
    }

    [Theory]
    [InlineData("Yes", "Yesterday we left.", "Yes Yesterday we left.")]
    [InlineData("I", "It is fine.", "I It is fine.")]
    [InlineData("No", "Nothing happened.", "No Nothing happened.")]
    [InlineData("an", "Another day.", "an Another day.")]
    public void KeepsAShortCueThatIsOnlyASubstringOfALaterWord(string first, string second, string expected)
    {
        // Dedup is by whole words. Matching raw substrings dropped "Yes" into "Yesterday" and "I"
        // into "It" — losing words the speaker actually said, so the ref-text stopped matching the
        // reference audio.
        var refText = TextToSpeechViewModel.BuildRefTextFromTranscription(new[] { first, second });

        Assert.Equal(expected, refText);
    }

    [Fact]
    public void CollapsesGrowingPrefixCues()
    {
        // Some word-timestamp modes emit a sentence that grows cue by cue.
        var cues = new[] { "I really", "I really like", "I really like that." };

        var refText = TextToSpeechViewModel.BuildRefTextFromTranscription(cues);

        Assert.Equal("I really like that.", refText);
    }

    [Fact]
    public void ResultStaysPlausibleAsRefTextForAShortClip()
    {
        // Regression guard on the observed failure: 40 karaoke cues off one sentence used to
        // produce ~1.5 kB of ref-text. It must come back down to the sentence itself.
        var sentence = "I really like that. I really like hearing that.";
        var words = sentence.Split(' ');
        var cues = new string[words.Length];
        for (var i = 0; i < words.Length; i++)
        {
            var copy = (string[])words.Clone();
            copy[i] = "<u>" + copy[i] + "</u>";
            cues[i] = string.Join(' ', copy);
        }

        var refText = TextToSpeechViewModel.BuildRefTextFromTranscription(cues);

        Assert.Equal(sentence, refText);
        Assert.DoesNotContain("<u>", refText);
    }
}

/// <summary>
/// A sidecar that cannot be a spoken transcription must read as "no transcript" so it is never
/// passed as ref-text and the OmniVoice backfill can replace it.
/// </summary>
public class UnusableTranscriptTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyIsUnusable(string? text)
    {
        Assert.True(Qwen3TtsCrispAsr.LooksLikeUnusableTranscript(text));
    }

    [Fact]
    public void AttributionBlurbIsUnusable()
    {
        Assert.True(Qwen3TtsCrispAsr.LooksLikeUnusableTranscript(
            "https://commons.wikimedia.org/wiki/File:Clint_Eastwood.flac\nDescription\nEnglish: The speaking voice of Clint Eastwood."));
    }

    [Fact]
    public void KaraokeMarkupIsUnusable()
    {
        // The real corruption: a karaoke transcription written straight into the sidecar. It is
        // neither empty nor a blurb, so it used to survive normalization untouched.
        Assert.True(Qwen3TtsCrispAsr.LooksLikeUnusableTranscript(
            "<u>being</u> a silent person, I said, it's not that I said, I wish I could do that, it's just being"));
    }

    [Theory]
    [InlineData("<i>italic</i> text")]
    [InlineData("<font color=\"#ff0000\">coloured</font>")]
    [InlineData("{\\an8}positioned")]
    public void OtherSubtitleMarkupIsUnusable(string text)
    {
        Assert.True(Qwen3TtsCrispAsr.LooksLikeUnusableTranscript(text));
    }

    [Fact]
    public void PlainSpokenTranscriptIsUsable()
    {
        Assert.False(Qwen3TtsCrispAsr.LooksLikeUnusableTranscript(
            "being a silent person, I said, it's not that I said, I wish I could do that."));
    }

    [Fact]
    public void TextWithAngleBracketsButNoTagsIsUsable()
    {
        // "<" alone is not markup — do not throw away a real transcript over punctuation.
        Assert.False(Qwen3TtsCrispAsr.LooksLikeUnusableTranscript("five < ten and ten > five"));
    }
}
