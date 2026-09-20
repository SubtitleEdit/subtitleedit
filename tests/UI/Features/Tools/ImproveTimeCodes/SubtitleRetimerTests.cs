using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using System.Globalization;
using System.Text;

namespace UITests.Features.Tools.ImproveTimeCodes;

public class SubtitleRetimerTests : IDisposable
{
    private readonly string _folder = Directory.CreateTempSubdirectory().FullName;

    public void Dispose() => Directory.Delete(_folder, true);

    /// <summary>Hands out window file names and remembers where each window was cut.</summary>
    private sealed class FakeAudio : ForcedAligner.IAudioSource
    {
        private readonly string _folder;
        public List<(double Start, double Duration)> Windows { get; } = new();
        public double TotalSeconds { get; init; } = 600;

        public FakeAudio(string folder) => _folder = folder;

        public Task<IReadOnlyList<OpenAiSttChunker.SilenceInterval>> DetectSilenceAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OpenAiSttChunker.SilenceInterval>>(Array.Empty<OpenAiSttChunker.SilenceInterval>());

        public Task<string> ExtractWindowAsync(double startSeconds, double durationSeconds, CancellationToken cancellationToken)
        {
            Windows.Add((startSeconds, durationSeconds));
            return Task.FromResult(Path.Combine(_folder, $"w{Windows.Count - 1}.wav"));
        }
    }

    /// <summary>
    /// An aligner that knows the truth: each fed text is looked up in <see cref="Truth"/> and
    /// reported relative to the window the audio source last cut.
    /// </summary>
    private sealed class OracleRunner : ForcedAligner.IRunner
    {
        private readonly FakeAudio _audio;
        public Dictionary<string, (double Start, double End)> Truth { get; } = new();
        public List<string[]> Fed { get; } = new();
        public Func<int, bool>? FailBatch { get; init; }

        public OracleRunner(FakeAudio audio) => _audio = audio;

        public Task<string> AlignAsync(string audioFileName, string textFileName, CancellationToken cancellationToken)
        {
            var texts = File.ReadAllLines(textFileName);
            Fed.Add(texts);
            if (FailBatch?.Invoke(Fed.Count - 1) == true)
            {
                throw new ForcedAlignerException("boom");
            }

            var windowStart = _audio.Windows[^1].Start;
            var sb = new StringBuilder();
            for (var i = 0; i < texts.Length; i++)
            {
                var (start, end) = Truth[texts[i]];
                sb.AppendLine((i + 1).ToString(CultureInfo.InvariantCulture));
                sb.AppendLine($"{Format(start - windowStart)} --> {Format(end - windowStart)}");
                sb.AppendLine(texts[i]);
                sb.AppendLine();
            }

            return Task.FromResult(sb.ToString());
        }

        private static string Format(double seconds)
            => TimeSpan.FromSeconds(Math.Max(0, seconds)).ToString(@"hh\:mm\:ss\,fff", CultureInfo.InvariantCulture);
    }

    private static List<SubtitleRetimer.Line> MakeLines(int count, double spacing = 3.0, double duration = 2.0)
        => Enumerable.Range(0, count)
            .Select(i => new SubtitleRetimer.Line($"Line number {i}", 10 + (i * spacing), 10 + (i * spacing) + duration))
            .ToList();

    [Fact]
    public void PlanBatches_FullBatchesBorrowSentinels_GapsDoNot()
    {
        var lines = MakeLines(20);
        // A long silence after line 11.
        for (var i = 12; i < 20; i++)
        {
            lines[i] = lines[i] with { StartSeconds = lines[i].StartSeconds + 30, EndSeconds = lines[i].EndSeconds + 30 };
        }

        var batches = SubtitleRetimer.PlanBatches(lines, new SubtitleRetimer.Options { BatchLines = 8 });

        Assert.Equal(3, batches.Count);
        Assert.Equal(new SubtitleRetimer.Batch(0, 7, false, true), batches[0]);
        Assert.Equal(new SubtitleRetimer.Batch(8, 11, true, false), batches[1]);
        Assert.Equal(new SubtitleRetimer.Batch(12, 19, false, false), batches[2]);
    }

    [Theory]
    [InlineData("<i>Hello</i> there", "Hello there")]
    [InlineData("{\\an8}- Hi.\n- Hello.", "Hi. Hello.")]
    [InlineData("[door slams]", "")]
    [InlineData("♪ ♪", "")]
    [InlineData("(sighs) Fine.", "Fine.")]
    [InlineData("♪ Let it be ♪", "Let it be")]
    [InlineData("", "")]
    public void GetSpokenText_KeepsOnlyWhatIsSaid(string text, string expected)
        => Assert.Equal(expected, SubtitleRetimer.GetSpokenText(text));

    [Fact]
    public async Task Retime_MovesLinesOntoTheAlignedTimes_AndDiscardsSentinels()
    {
        var lines = MakeLines(12);
        var audio = new FakeAudio(_folder);
        var runner = new OracleRunner(audio);
        foreach (var l in lines)
        {
            runner.Truth[l.Text] = (l.StartSeconds + 0.3, l.EndSeconds - 0.2);
        }

        var results = await new SubtitleRetimer(runner, audio, new SubtitleRetimer.Options { BatchLines = 8 })
            .RetimeAsync(lines, null, TestContext.Current.CancellationToken);

        Assert.Equal(2, runner.Fed.Count);
        Assert.Equal(9, runner.Fed[0].Length); // 8 + trailing sentinel
        Assert.Equal(5, runner.Fed[1].Length); // leading sentinel + 4
        for (var i = 0; i < lines.Count; i++)
        {
            Assert.Equal(SubtitleRetimer.LineStatus.Retimed, results[i].Status);
            Assert.Equal(lines[i].StartSeconds + 0.3, results[i].StartSeconds, 2);
            Assert.Equal(lines[i].EndSeconds - 0.2, results[i].EndSeconds, 2);
        }
    }

    [Fact]
    public async Task Retime_LeavesALineAlone_WhenItWouldMoveTooFar()
    {
        var lines = MakeLines(4);
        var audio = new FakeAudio(_folder);
        var runner = new OracleRunner(audio);
        foreach (var l in lines)
        {
            runner.Truth[l.Text] = (l.StartSeconds + 0.1, l.EndSeconds);
        }

        runner.Truth[lines[2].Text] = (lines[2].StartSeconds - 2.6, lines[2].EndSeconds - 2.6);

        var results = await new SubtitleRetimer(runner, audio).RetimeAsync(lines, null, TestContext.Current.CancellationToken);

        Assert.Equal(SubtitleRetimer.LineStatus.ShiftTooLarge, results[2].Status);
        Assert.Equal(lines[2].StartSeconds, results[2].StartSeconds);
        Assert.Equal(lines[2].EndSeconds, results[2].EndSeconds);
        Assert.Equal(SubtitleRetimer.LineStatus.Retimed, results[1].Status);
    }

    [Fact]
    public async Task Retime_AnEndHeldForReading_IsNotPulledInToTheSpeech_ItTravelsWithTheStart()
    {
        var lines = MakeLines(3);
        var audio = new FakeAudio(_folder);
        var runner = new OracleRunner(audio);
        foreach (var l in lines)
        {
            runner.Truth[l.Text] = (l.StartSeconds + 0.2, l.EndSeconds);
        }

        // The speech of line 1 stops 1.2 s before the subtitle goes away.
        runner.Truth[lines[1].Text] = (lines[1].StartSeconds + 0.2, lines[1].EndSeconds - 1.2);

        var results = await new SubtitleRetimer(runner, audio).RetimeAsync(lines, null, TestContext.Current.CancellationToken);

        Assert.Equal(SubtitleRetimer.LineStatus.Retimed, results[1].Status);
        Assert.Equal(lines[1].StartSeconds + 0.2, results[1].StartSeconds, 2);
        Assert.Equal(lines[1].EndSeconds + 0.2, results[1].EndSeconds, 2);
    }

    [Fact]
    public async Task Retime_SkipsLinesWithNothingSpoken_AndKeepsTheirTimes()
    {
        var lines = MakeLines(3);
        lines[1] = lines[1] with { Text = "[music]" };
        var audio = new FakeAudio(_folder);
        var runner = new OracleRunner(audio);
        runner.Truth[lines[0].Text] = (lines[0].StartSeconds + 0.2, lines[0].EndSeconds);
        runner.Truth[lines[2].Text] = (lines[2].StartSeconds + 0.2, lines[2].EndSeconds);

        var results = await new SubtitleRetimer(runner, audio).RetimeAsync(lines, null, TestContext.Current.CancellationToken);

        Assert.Equal(SubtitleRetimer.LineStatus.NoSpeech, results[1].Status);
        Assert.Equal(lines[1].StartSeconds, results[1].StartSeconds);
        Assert.Equal(2, runner.Fed.Single().Length);
    }

    [Fact]
    public async Task Retime_AnEdgeThatIsPinnedToTheWindow_IsNotTrusted()
    {
        var lines = MakeLines(2);
        var audio = new FakeAudio(_folder);
        var runner = new OracleRunner(audio);
        // The last cue runs on to the end of the window, as a real aligner's does.
        runner.Truth[lines[0].Text] = (lines[0].StartSeconds + 0.2, lines[0].EndSeconds - 0.1);
        runner.Truth[lines[1].Text] = (lines[1].StartSeconds + 0.2, lines[1].EndSeconds + 2.5);

        var results = await new SubtitleRetimer(runner, audio).RetimeAsync(lines, null, TestContext.Current.CancellationToken);

        Assert.Equal(lines[1].StartSeconds + 0.2, results[1].StartSeconds, 2);
        Assert.Equal(lines[1].EndSeconds + 0.2, results[1].EndSeconds, 2); // not measured, so it keeps the duration
    }

    [Fact]
    public async Task Retime_KeepsAShortenedLineUpLongEnoughToRead_ButNotIntoTheNextLine()
    {
        var lines = new List<SubtitleRetimer.Line>
        {
            new("A fairly long line that takes a while to read", 10, 13),
            new("Next", 13.1, 15),
        };
        var audio = new FakeAudio(_folder);
        var runner = new OracleRunner(audio);
        runner.Truth[lines[0].Text] = (10.1, 11.0);
        runner.Truth[lines[1].Text] = (12.5, 13.4);

        var options = new SubtitleRetimer.Options { MaxShiftSeconds = 3, ReadingCharsPerSecond = 20, MinDurationSeconds = 1.0, MinGapSeconds = 0.1 };
        var results = await new SubtitleRetimer(runner, audio, options).RetimeAsync(lines, null, TestContext.Current.CancellationToken);

        // 45 chars at 20 cps wants 2.25 s, but the next line now starts at 12.5.
        Assert.Equal(10.1, results[0].StartSeconds, 2);
        Assert.Equal(12.35, results[0].EndSeconds, 2);
        Assert.Equal(12.5, results[1].StartSeconds, 2);
    }

    private static (List<SubtitleRetimer.Line> Lines, SubtitleRetimer.LineResult[] Results) MakeShifted(params double?[] shifts)
    {
        var lines = MakeLines(shifts.Length);
        var results = lines
            .Select((l, i) => shifts[i] is { } shift
                ? new SubtitleRetimer.LineResult(l.StartSeconds + shift, l.EndSeconds + shift, SubtitleRetimer.LineStatus.Retimed)
                : new SubtitleRetimer.LineResult(l.StartSeconds, l.EndSeconds, SubtitleRetimer.LineStatus.ShiftTooLarge))
            .ToArray();
        return (lines, results);
    }

    [Fact]
    public void FollowNeighbours_ALineThatLeavesAConsistentNeighbourhood_GetsTheNeighboursOffset()
    {
        // The subtitle is a second late throughout; line 3's text opens after words it leaves out.
        var (lines, results) = MakeShifted(-1.0, -1.05, -0.95, -0.2, -1.0, -1.1, -0.9);

        SubtitleRetimer.FollowNeighbours(lines, results, new SubtitleRetimer.Options { MaxShiftSeconds = 2.0 });

        Assert.Equal(SubtitleRetimer.LineStatus.MovedWithNeighbours, results[3].Status);
        Assert.Equal(lines[3].StartSeconds - 1.0, results[3].StartSeconds, 2);
        Assert.Equal(lines[3].EndSeconds - 1.0, results[3].EndSeconds, 2);
        Assert.All(results.Where((_, i) => i != 3), r => Assert.Equal(SubtitleRetimer.LineStatus.Retimed, r.Status));
    }

    [Fact]
    public void FollowNeighbours_ALineRefusedForGoingTooFar_StillGetsTheOffset()
    {
        var (lines, results) = MakeShifted(-1.0, -1.05, null, -0.95, -1.0);

        SubtitleRetimer.FollowNeighbours(lines, results, new SubtitleRetimer.Options { MaxShiftSeconds = 2.0 });

        Assert.Equal(SubtitleRetimer.LineStatus.MovedWithNeighbours, results[2].Status);
        Assert.Equal(lines[2].StartSeconds - 1.0, results[2].StartSeconds, 2);
    }

    [Fact]
    public void FollowNeighbours_InASubtitleThatIsInSync_ALoneLargeMoveIsOfferedNotMade()
    {
        var (lines, results) = MakeShifted(0.05, -0.04, 0.02, 0.8, 0.03, -0.05, 0.04);

        SubtitleRetimer.FollowNeighbours(lines, results, new SubtitleRetimer.Options { MaxShiftSeconds = 2.0 });

        Assert.Equal(SubtitleRetimer.LineStatus.LargeMoveUnconfirmed, results[3].Status);
        Assert.Equal(lines[3].StartSeconds + 0.8, results[3].StartSeconds, 2); // the aligner's answer is kept
    }

    [Fact]
    public void Tidy_KeepsClearOfBothPositionsOfAnUnconfirmedLine()
    {
        var lines = new List<SubtitleRetimer.Line> { new("First line", 10, 12), new("Second line", 12.1, 14) };
        var results = new[]
        {
            new SubtitleRetimer.LineResult(10.2, 12.6, SubtitleRetimer.LineStatus.Retimed),
            new SubtitleRetimer.LineResult(12.9, 14.8, SubtitleRetimer.LineStatus.LargeMoveUnconfirmed),
        };

        SubtitleRetimer.Tidy(lines, results, new SubtitleRetimer.Options { MinGapSeconds = 0.1 }, 0);

        Assert.Equal(12.0, results[0].EndSeconds, 3); // the second line may yet stay at 12.1
    }

    [Fact]
    public void FollowNeighbours_WhenTheNeighboursDisagree_TheAlignerIsBelieved()
    {
        var (lines, results) = MakeShifted(-1.2, 0.9, -0.4, 1.4, 0.6, -0.9, 0.1);
        var before = results.ToArray();

        SubtitleRetimer.FollowNeighbours(lines, results, new SubtitleRetimer.Options { MaxShiftSeconds = 2.0 });

        Assert.Equal(before, results);
    }

    [Fact]
    public void FollowNeighbours_SmallDifferencesBetweenLines_AreLeftToTheAligner()
    {
        var (lines, results) = MakeShifted(-0.2, 0.15, -0.1, 0.3, 0.1, -0.25, 0.2);
        var before = results.ToArray();

        SubtitleRetimer.FollowNeighbours(lines, results, new SubtitleRetimer.Options());

        Assert.Equal(before, results);
    }

    [Fact]
    public async Task Retime_OneFailedBatch_LeavesItsLinesAndCarriesOn()
    {
        var lines = MakeLines(12);
        var audio = new FakeAudio(_folder);
        var runner = new OracleRunner(audio) { FailBatch = b => b == 0 };
        foreach (var l in lines)
        {
            runner.Truth[l.Text] = (l.StartSeconds + 0.3, l.EndSeconds);
        }

        var results = await new SubtitleRetimer(runner, audio, new SubtitleRetimer.Options { BatchLines = 8 })
            .RetimeAsync(lines, null, TestContext.Current.CancellationToken);

        Assert.All(results.Take(8), r => Assert.Equal(SubtitleRetimer.LineStatus.Failed, r.Status));
        Assert.All(results.Skip(8), r => Assert.Equal(SubtitleRetimer.LineStatus.Retimed, r.Status));
    }

    [Fact]
    public async Task Retime_EveryBatchFailing_Throws()
    {
        var lines = MakeLines(3);
        var audio = new FakeAudio(_folder);
        var runner = new OracleRunner(audio) { FailBatch = _ => true };

        await Assert.ThrowsAsync<ForcedAlignerException>(() =>
            new SubtitleRetimer(runner, audio).RetimeAsync(lines, null, TestContext.Current.CancellationToken));
    }
}
