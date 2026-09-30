using Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;
using static Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes.SubtitleRetimer;

namespace UITests.Features.Tools.ImproveTimeCodes;

public class SpeechToTextCheckTests
{
    /// <summary>What speech-to-text would report for these lines if it heard them at these times: 0.3 s a word.</summary>
    private static List<SpeechToTextCheck.HeardWord> Speak(params (string Text, double Start)[] said)
    {
        var words = new List<SpeechToTextCheck.HeardWord>();
        foreach (var (text, start) in said)
        {
            var t = start;
            foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                words.Add(new SpeechToTextCheck.HeardWord(word, t, t + 0.25));
                t += 0.3;
            }
        }

        return words;
    }

    private static readonly string[] Texts =
    {
        "Where have you been all night?",
        "Out looking for the dog.",
        "Did you find him?",
        "He was sleeping under the bridge.",
        "Of course he was.",
        "Bring him inside, it is cold.",
        "I will make some tea.",
    };

    /// <summary>Seven lines, three seconds apart, and speech-to-text hearing each where the line says.</summary>
    private static (List<Line> Lines, List<SpeechToTextCheck.HeardWord> Heard) InSync()
    {
        var lines = Texts.Select((t, i) => new Line(t, 10 + (i * 3), 12 + (i * 3))).ToList();
        var heard = Speak(Texts.Select((t, i) => (t, 10.0 + (i * 3))).ToArray());
        return (lines, heard);
    }

    private static LineResult[] Aligned(List<Line> lines) =>
        lines.Select(l => new LineResult(l.StartSeconds, l.EndSeconds, LineStatus.Retimed)).ToArray();

    [Fact]
    public void Match_FindsEveryLineWhereItWasSaid()
    {
        var (lines, heard) = InSync();

        var evidence = SpeechToTextCheck.Match(lines, heard, 3.5);

        for (var i = 0; i < lines.Count; i++)
        {
            Assert.Equal(1.0, evidence[i]!.HeardRatio);
            Assert.Equal(lines[i].StartSeconds, evidence[i]!.AnchorSeconds!.Value, 3);
            Assert.Equal(0, evidence[i]!.AnchorFraction);
        }
    }

    [Fact]
    public void Match_TextThatIsNotSaid_ShowsAsNotHeard()
    {
        var (lines, heard) = InSync();
        lines[3] = lines[3] with { Text = "Totally different words entirely." };

        var evidence = SpeechToTextCheck.Match(lines, heard, 3.5);

        Assert.Equal(0, evidence[3]!.HeardRatio);
        Assert.Null(evidence[3]!.AnchorSeconds);
        Assert.Equal(1.0, evidence[4]!.HeardRatio);
    }

    [Fact]
    public void Match_ANeighboursWordsAreNotBorrowed()
    {
        // "I will make some tea." is not said at all, but "I" and "will" are nearby in other lines.
        var lines = new List<Line>
        {
            new("I will go now.", 10, 12),
            new("I will make some tea.", 12.5, 14),
        };
        var heard = Speak(("I will go now.", 10.0));

        var evidence = SpeechToTextCheck.Match(lines, heard, 3.5);

        Assert.Equal(1.0, evidence[0]!.HeardRatio);
        Assert.Equal(0, evidence[1]!.HeardRatio);
    }

    [Fact]
    public void Match_AMissedFirstWord_AnchorsOnTheNextOne()
    {
        var (lines, heard) = InSync();
        heard.RemoveAll(w => w.Word == "Bring");

        var evidence = SpeechToTextCheck.Match(lines, heard, 3.5);

        Assert.Equal(heard.First(w => w.Word == "him").StartSeconds, evidence[5]!.AnchorSeconds!.Value, 3);
        Assert.True(evidence[5]!.AnchorFraction > 0);
    }

    [Fact]
    public void Match_NothingSpoken_HasNoEvidence()
    {
        var lines = new List<Line> { new("[door slams]", 10, 12) };

        Assert.Null(SpeechToTextCheck.Match(lines, Speak(("Hello", 10.0)), 3.5)[0]);
    }

    [Fact]
    public void Apply_ALargeMoveToWhereItIsHeard_IsConfirmed()
    {
        var (lines, heard) = InSync();
        lines[3] = lines[3] with { StartSeconds = lines[3].StartSeconds + 1, EndSeconds = lines[3].EndSeconds + 1 };
        var results = Aligned(InSync().Lines);
        results[3] = results[3] with { Status = LineStatus.LargeMoveUnconfirmed };

        SpeechToTextCheck.Apply(lines, results, SpeechToTextCheck.Match(lines, heard, 3.5));

        Assert.Equal(LineStatus.ConfirmedBySpeech, results[3].Status);
        Assert.Equal(1.0, results[3].HeardRatio);
    }

    [Fact]
    public void Apply_ALargeMoveAwayFromWhereItIsHeard_IsDisputed()
    {
        var (lines, heard) = InSync();
        var results = Aligned(lines);
        results[3] = new LineResult(lines[3].StartSeconds - 1, lines[3].EndSeconds - 1, LineStatus.LargeMoveUnconfirmed);

        SpeechToTextCheck.Apply(lines, results, SpeechToTextCheck.Match(lines, heard, 3.5));

        Assert.Equal(LineStatus.DisputedBySpeech, results[3].Status);
        Assert.True(IsUnconfirmed(results[3].Status));
    }

    [Fact]
    public void Apply_AnAcceptedMoveClearlyAwayFromWhereItIsHeard_IsDisputed()
    {
        var (lines, heard) = InSync();
        var results = Aligned(lines);
        results[2] = new LineResult(lines[2].StartSeconds + 0.5, lines[2].EndSeconds + 0.5, LineStatus.Retimed);

        SpeechToTextCheck.Apply(lines, results, SpeechToTextCheck.Match(lines, heard, 3.5));

        Assert.Equal(LineStatus.DisputedBySpeech, results[2].Status);
        Assert.All(results.Where((_, i) => i != 2), r => Assert.Equal(LineStatus.Retimed, r.Status));
    }

    [Fact]
    public void Apply_ASmallMove_IsNotSecondGuessed()
    {
        var (lines, heard) = InSync();
        var results = Aligned(lines);
        results[2] = new LineResult(lines[2].StartSeconds + 0.2, lines[2].EndSeconds + 0.2, LineStatus.Retimed);

        SpeechToTextCheck.Apply(lines, results, SpeechToTextCheck.Match(lines, heard, 3.5));

        Assert.Equal(LineStatus.Retimed, results[2].Status);
    }

    [Fact]
    public void Apply_WhenTheNeighboursOverruledTheAligner_AndTheSpeechSidesWithIt_TheAlignerWins()
    {
        var (truth, heard) = InSync();
        var lines = truth.Select(l => l with { StartSeconds = l.StartSeconds + 1, EndSeconds = l.EndSeconds + 1 }).ToList();
        var results = Aligned(truth);
        results[3] = new LineResult(lines[3].StartSeconds + 0.4, lines[3].EndSeconds + 0.4, LineStatus.MovedWithNeighbours)
        {
            AlignerOwn = (truth[3].StartSeconds, truth[3].EndSeconds),
        };

        SpeechToTextCheck.Apply(lines, results, SpeechToTextCheck.Match(lines, heard, 3.5));

        Assert.Equal(LineStatus.ConfirmedBySpeech, results[3].Status);
        Assert.Equal(truth[3].StartSeconds, results[3].StartSeconds, 3);
    }

    [Fact]
    public void Apply_ALineHardlyHeard_IsNotRefereed()
    {
        var (lines, heard) = InSync();
        lines[3] = lines[3] with { Text = "He was asleep beneath an old stone arch." }; // only "He was" is said
        var results = Aligned(lines);
        results[3] = new LineResult(lines[3].StartSeconds - 1, lines[3].EndSeconds - 1, LineStatus.LargeMoveUnconfirmed);

        SpeechToTextCheck.Apply(lines, results, SpeechToTextCheck.Match(lines, heard, 3.5));

        Assert.Equal(LineStatus.LargeMoveUnconfirmed, results[3].Status);
        Assert.True(results[3].HeardRatio < 0.5);
    }

    [Fact]
    public void Apply_ASteadyOffsetBetweenSpeechToTextAndTheAligner_IsAllowedFor()
    {
        // Speech-to-text marks every word 0.3 s early. The aligner put the large move exactly
        // where the speech is, which without the correction would look 0.3 s off - too far to
        // agree with.
        var (lines, _) = InSync();
        var heard = Speak(Texts.Select((t, i) => (t, 10.0 + (i * 3) - 0.3)).ToArray());
        var original = lines.ToList();
        original[3] = original[3] with { StartSeconds = original[3].StartSeconds + 0.6, EndSeconds = original[3].EndSeconds + 0.6 };
        var results = Aligned(lines);
        results[3] = results[3] with { Status = LineStatus.LargeMoveUnconfirmed };

        SpeechToTextCheck.Apply(original, results, SpeechToTextCheck.Match(original, heard, 3.5));

        Assert.Equal(LineStatus.ConfirmedBySpeech, results[3].Status);
    }

    [Theory]
    [InlineData("Don't,", "don't")]
    [InlineData("colour", "color")]
    public void Match_TreatsSpellingAndPunctuationVariantsAsTheSameWord(string written, string said)
    {
        var lines = new List<Line> { new($"Well {written} worry about it", 10, 12) };
        var heard = Speak(($"Well {said} worry about it", 10.0));

        Assert.Equal(1.0, SpeechToTextCheck.Match(lines, heard, 3.5)[0]!.HeardRatio);
    }

    [Fact]
    public void ParseWords_SharesOutACueThatHoldsSeveralWords()
    {
        var srt = new List<string>
        {
            "1", "00:00:01,000 --> 00:00:01,400", "Hello", "",
            "2", "00:00:02,000 --> 00:00:03,000", "big world", "",
        };

        var words = CrispAsrWordTranscriber.ParseWords(srt);

        Assert.Equal(3, words.Count);
        Assert.Equal("Hello", words[0].Word);
        Assert.Equal(2.0, words[1].StartSeconds, 3);
        Assert.Equal(2.375, words[2].StartSeconds, 3);
    }

    [Theory]
    [InlineData("crispasr: progress =  14% (1/7 slices)", true, 14)]
    [InlineData("crispasr: progress = 100% (7/7 slices)", true, 100)]
    [InlineData("[00:00:01.000 --> 00:00:01.400]  Hello", false, 0)]
    public void TryParseProgress_ReadsCrispAsrProgressLines(string line, bool expected, double percent)
    {
        Assert.Equal(expected, CrispAsrWordTranscriber.TryParseProgress(line, out var value));
        Assert.Equal(percent, value);
    }
}
