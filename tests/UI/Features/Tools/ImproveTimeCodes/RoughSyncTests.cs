using Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;
using static Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes.SubtitleRetimer;

namespace UITests.Features.Tools.ImproveTimeCodes;

public class RoughSyncTests
{
    private static readonly string[] Syllables = { "ka", "lo", "mi", "ren", "sto", "vu", "par", "de", "ni", "tho", "gle", "ser" };

    /// <summary>
    /// A made-up script: 120 lines of six words, mostly long distinctive ones with some short
    /// common ones among them, said 3 s apart - and speech-to-text hearing every word of it.
    /// </summary>
    private static (List<Line> Truth, List<SpeechToTextCheck.HeardWord> Heard) Script(int seed = 1)
    {
        var random = new Random(seed);
        string Word() => random.Next(4) == 0
            ? new[] { "the", "and", "you", "was", "not" }[random.Next(5)]
            : string.Concat(Enumerable.Range(0, 3).Select(_ => Syllables[random.Next(Syllables.Length)]));

        var truth = new List<Line>();
        var heard = new List<SpeechToTextCheck.HeardWord>();
        for (var i = 0; i < 120; i++)
        {
            var words = Enumerable.Range(0, 6).Select(_ => Word()).ToList();
            var start = 5 + (i * 3.0);
            truth.Add(new Line(string.Join(" ", words) + ".", start, start + 2.2));
            for (var k = 0; k < words.Count; k++)
            {
                heard.Add(new SpeechToTextCheck.HeardWord(words[k], start + (k * 0.35), start + (k * 0.35) + 0.3));
            }
        }

        return (truth, heard);
    }

    private static List<Line> Shift(List<Line> truth, Func<int, Line, double> offset)
        => truth.Select((l, i) => l with { StartSeconds = l.StartSeconds + offset(i, l), EndSeconds = l.EndSeconds + offset(i, l) }).ToList();

    private static void AssertSyncedBack(List<Line> truth, List<Line> synced, int allowedMisses = 0)
    {
        var misses = Enumerable.Range(0, truth.Count).Where(i => Math.Abs(synced[i].StartSeconds - truth[i].StartSeconds) > 0.3).ToList();
        Assert.True(misses.Count <= allowedMisses,
            $"{misses.Count} lines more than 0.3 s off: " + string.Join(", ", misses.Select(i => $"#{i} {synced[i].StartSeconds - truth[i].StartSeconds:0.00}")));
    }

    [Fact]
    public void AConstantOffset_IsMeasuredAndTakenOut()
    {
        var (truth, heard) = Script();
        var lines = Shift(truth, (_, _) => 12.0);

        var sync = RoughSync.Measure(lines, heard);

        Assert.NotNull(sync);
        Assert.InRange(sync!.MaxAbsOffset, 11.8, 12.2);
        AssertSyncedBack(truth, RoughSync.Apply(lines, sync));
    }

    [Fact]
    public void ADrift_FromAFrameRateMismatch_IsFollowed()
    {
        var (truth, heard) = Script();
        var lines = Shift(truth, (_, l) => l.StartSeconds * 25.0 / 23.976 - l.StartSeconds); // up to 15 s by the end

        var sync = RoughSync.Measure(lines, heard);

        AssertSyncedBack(truth, RoughSync.Apply(lines, sync!));
    }

    [Fact]
    public void AJump_WhereAScenesWasCut_StaysSharp()
    {
        var (truth, heard) = Script();
        var lines = Shift(truth, (i, _) => i < 60 ? 2.0 : -5.0);

        var sync = RoughSync.Measure(lines, heard);

        // Only lines right at the cut may be caught between the two offsets.
        AssertSyncedBack(truth, RoughSync.Apply(lines, sync!), allowedMisses: 2);
    }

    [Fact]
    public void MissedAndMisheardWords_DoNotThrowItOff()
    {
        var (truth, heard) = Script();
        var random = new Random(9);
        var noisy = heard
            .Where(_ => random.Next(5) != 0) // a fifth never heard
            .Select(w => random.Next(10) == 0 ? w with { Word = "zzz" + w.Word } : w) // a tenth misheard
            .ToList();
        var lines = Shift(truth, (_, _) => -7.5);

        var sync = RoughSync.Measure(lines, noisy);

        AssertSyncedBack(truth, RoughSync.Apply(lines, sync!));
    }

    [Fact]
    public void AWrongMatchOnTheFirstLine_IsOutvoted()
    {
        var (truth, heard) = Script();
        var lines = Shift(truth, (_, _) => 4.0);
        // The first line's words are heard 30 s later too, and not where it was said.
        heard.RemoveAll(w => w.StartSeconds < truth[1].StartSeconds);
        heard.AddRange(truth[0].Text.TrimEnd('.').Split(' ').Select((w, k) => new SpeechToTextCheck.HeardWord(w, 36.1 + (k * 0.35), 36.4 + (k * 0.35))));
        heard.Sort((a, b) => a.StartSeconds.CompareTo(b.StartSeconds));

        var sync = RoughSync.Measure(lines, heard);

        Assert.Equal(-4.0, sync!.Offsets[0], 0);
    }

    [Fact]
    public void ASubtitleInSync_IsLeftWhereItIs()
    {
        var (truth, heard) = Script();

        var sync = RoughSync.Measure(truth, heard);

        Assert.True(sync!.MaxAbsOffset < 0.1);
    }

    [Fact]
    public void TextThatIsNotInTheAudio_GivesNoSync()
    {
        var (truth, _) = Script(1);
        var (_, otherHeard) = Script(2);

        Assert.Null(RoughSync.Measure(truth, otherHeard));
    }

    [Fact]
    public void NoLineIsMovedBeforeTheStartOfTheAudio()
    {
        var (truth, heard) = Script();
        var lines = Shift(truth, (_, _) => -20.0).Select(l => l with { StartSeconds = Math.Max(0, l.StartSeconds) }).ToList();

        var synced = RoughSync.Apply(lines, RoughSync.Measure(lines, heard)!);

        Assert.All(synced, l => Assert.True(l.StartSeconds >= 0));
    }

    [Fact]
    public void Merge_LinesTheAlignerLeftAlone_KeepTheSyncsMove()
    {
        var original = new List<Line> { new("a", 10, 12), new("b", 14, 16), new("c", 18, 20), new("d", 22, 24) };
        var synced = original.Select(l => l with { StartSeconds = l.StartSeconds - 5, EndSeconds = l.EndSeconds - 5 }).ToList();
        synced[3] = original[3];
        var results = new[]
        {
            new LineResult(5.1, 7, LineStatus.Retimed),
            new LineResult(9, 11, LineStatus.Unchanged),
            new LineResult(13, 15, LineStatus.NoSpeech),
            new LineResult(22, 24, LineStatus.Unchanged),
        };

        RoughSync.Merge(original, synced, results);

        Assert.Equal(LineStatus.Retimed, results[0].Status);
        Assert.Equal(5.1, results[0].StartSeconds);
        Assert.Equal(LineStatus.MovedWithSync, results[1].Status);
        Assert.Equal(LineStatus.MovedWithSync, results[2].Status);
        Assert.Equal(13, results[2].StartSeconds);
        Assert.Equal(LineStatus.Unchanged, results[3].Status);
        Assert.True(IsMove(LineStatus.MovedWithSync));
        Assert.False(IsUnconfirmed(LineStatus.MovedWithSync));
    }

    [Fact]
    public void Apply_LinesGivenDifferentOffsets_DoNotRunIntoEachOther()
    {
        var lines = new List<SubtitleRetimer.Line> { new("a", 10, 12), new("b", 12.1, 14), new("c", 20, 22) };
        var sync = new RoughSync.Result(new[] { 1.5, 1.2, 1.2 }, 3, 3);

        var synced = RoughSync.Apply(lines, sync);

        Assert.Equal(11.5, synced[0].StartSeconds, 3);
        Assert.Equal(13.2, synced[0].EndSeconds, 3); // the 0.1 s gap there was is kept
        Assert.Equal(13.3, synced[1].StartSeconds, 3);
    }

    [Fact]
    public void Merge_AMovedLine_FallsBackToItsSyncedPlace()
    {
        var original = new List<SubtitleRetimer.Line> { new("a", 10, 12) };
        var synced = new List<SubtitleRetimer.Line> { new("a", 12, 14) };
        var results = new[] { new SubtitleRetimer.LineResult(11.6, 13.6, SubtitleRetimer.LineStatus.DisputedBySpeech) };

        RoughSync.Merge(original, synced, results);

        Assert.Equal((12.0, 14.0), results[0].Fallback);
    }
}
