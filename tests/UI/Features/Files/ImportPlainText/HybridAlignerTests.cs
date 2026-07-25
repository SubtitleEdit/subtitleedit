using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Files.ImportPlainText;

public class HybridAlignerTests
{
    public HybridAlignerTests()
    {
        Se.Settings.General.UseFrameMode = false;
        Se.Settings.General.MinimumBetweenLines = new MsOrFramesValue { Milliseconds = 24 };
        Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
        Se.Settings.General.SubtitleMaximumDisplayMilliseconds = 8000;
        Se.Settings.General.SubtitleOptimalCharactersPerSeconds = 15.0;
    }

    private sealed class FakeAudio : ForcedAligner.IAudioSource
    {
        private readonly string _folder;
        private int _index;

        public List<(double Start, double Duration)> Windows { get; } = new();

        public double TotalSeconds { get; }

        public FakeAudio(double totalSeconds, string folder)
        {
            TotalSeconds = totalSeconds;
            _folder = folder;
        }

        public Task<IReadOnlyList<OpenAiSttChunker.SilenceInterval>> DetectSilenceAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<OpenAiSttChunker.SilenceInterval>>(Array.Empty<OpenAiSttChunker.SilenceInterval>());

        public Task<string> ExtractWindowAsync(double startSeconds, double durationSeconds, CancellationToken cancellationToken)
        {
            Windows.Add((startSeconds, durationSeconds));
            var path = Path.Combine(_folder, $"r{_index++}.wav");
            File.WriteAllText(path, "not really audio");
            return Task.FromResult(path);
        }
    }

    /// <summary>Places each fed line back to back from the start of the region it was given.</summary>
    private sealed class FakeRunner : ForcedAligner.IRunner
    {
        public List<int> LinesPerRegion { get; } = new();

        public Task<string> AlignAsync(string audioFileName, string textFileName, CancellationToken cancellationToken)
        {
            var lines = File.ReadAllLines(textFileName).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            LinesPerRegion.Add(lines.Count);

            var srt = new System.Text.StringBuilder();
            var t = 0.0;
            for (var i = 0; i < lines.Count; i++)
            {
                var end = t + 2.0;
                srt.AppendLine((i + 1).ToString());
                srt.AppendLine($"{Fmt(t)} --> {Fmt(end)}");
                srt.AppendLine(lines[i]);
                srt.AppendLine();
                t = end + 0.5;
            }

            return Task.FromResult(srt.ToString());

            static string Fmt(double seconds)
            {
                var ts = TimeSpan.FromSeconds(seconds);
                return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00},{ts.Milliseconds:000}";
            }
        }
    }

    [Fact]
    public void PlanRegions_RunsBetweenDirectlyMatchedLines()
    {
        // Lines 0 and 3 matched the transcript directly; 1 and 2 were interpolated between
        // them and are exactly what the aligner should be asked to place properly.
        var starts = new List<double> { 10, 14, 18, 22 };
        var ends = new List<double> { 13, 17, 21, 25 };
        var matched = new List<bool> { true, false, false, true };

        var regions = HybridAligner.PlanRegions(starts, ends, matched);

        var region = Assert.Single(regions);
        Assert.Equal(0, region.FirstLine);
        Assert.Equal(3, region.LastLine);
        Assert.Equal(8, region.StartSeconds, 3);   // 10 - 2 s padding
        Assert.Equal(27, region.EndSeconds, 3);    // 25 + 2 s padding
    }

    [Fact]
    public void PlanRegions_EndsOnATrustedLine()
    {
        // The tail here is interpolated with nothing after it to anchor against, so the
        // region stops at the last line that actually matched.
        var starts = new List<double> { 10, 14, 18, 22 };
        var ends = new List<double> { 13, 17, 21, 25 };
        var matched = new List<bool> { true, true, false, false };

        var region = Assert.Single(HybridAligner.PlanRegions(starts, ends, matched));

        Assert.Equal(0, region.FirstLine);
        Assert.Equal(1, region.LastLine);
    }

    [Fact]
    public void PlanRegions_SkipsLinesWithNoTimeCodes()
    {
        var starts = new List<double> { 10, -1, -1, 40 };
        var ends = new List<double> { 13, -1, -1, 44 };
        var matched = new List<bool> { true, false, false, true };

        var regions = HybridAligner.PlanRegions(starts, ends, matched);

        // The untimed pair breaks the run, and neither side has two trusted lines to span.
        Assert.Empty(regions);
    }

    [Fact]
    public void PlanRegions_SplitsOverlyLongRuns()
    {
        // 40 lines over 20 minutes: one region would be far past the aligner's practical
        // window, so it gets broken at line boundaries.
        var starts = new List<double>();
        var ends = new List<double>();
        var matched = new List<bool>();
        for (var i = 0; i < 40; i++)
        {
            starts.Add(i * 30.0);
            ends.Add((i * 30.0) + 25);
            matched.Add(true);
        }

        var regions = HybridAligner.PlanRegions(starts, ends, matched);

        Assert.True(regions.Count > 1, "a 20 minute run should not be handed over in one piece");
        Assert.All(regions, r => Assert.True(
            r.DurationSeconds <= HybridAligner.MaxRegionSeconds + (2 * HybridAligner.RegionPaddingSeconds) + 30,
            $"region of {r.DurationSeconds:F0}s is too long"));

        // Every line must still be covered exactly once, in order.
        var covered = regions.SelectMany(r => Enumerable.Range(r.FirstLine, r.LineCount)).ToList();
        Assert.Equal(covered, covered.OrderBy(x => x).ToList());
        Assert.Equal(covered.Count, covered.Distinct().Count());
    }

    [Fact]
    public void PlanRegions_NoDirectMatchesMeansNothingToRefine()
    {
        var starts = new List<double> { 10, 14, 18 };
        var ends = new List<double> { 13, 17, 21 };
        var matched = new List<bool> { false, false, false };

        Assert.Empty(HybridAligner.PlanRegions(starts, ends, matched));
    }

    [Fact]
    public async Task RefineAsync_ReplacesCoarseTimesWithAlignedOnes()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var lines = new List<SubtitleLineViewModel>
            {
                Line("First spoken line here", 100, 104),
                Line("Second spoken line here", 105, 109),
                Line("Third spoken line here", 110, 114),
            };
            var matched = new List<bool> { true, false, true };

            var audio = new FakeAudio(600, folder);
            var result = await new HybridAligner(new FakeRunner(), audio).RefineAsync(lines, matched);

            Assert.Equal(3, result.Refined);

            // The region starts at 100 - 2 s padding, and the fake lays lines out from there.
            Assert.Equal(98, lines[0].StartTime.TotalSeconds, 2);
            Assert.Equal(100.5, lines[1].StartTime.TotalSeconds, 2);
            Assert.Equal(103, lines[2].StartTime.TotalSeconds, 2);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task RefineAsync_KeepsCoarseTimesWhenARegionFails()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var lines = new List<SubtitleLineViewModel>
            {
                Line("First spoken line here", 100, 104),
                Line("Second spoken line here", 105, 109),
            };
            var matched = new List<bool> { true, true };

            var result = await new HybridAligner(new ThrowingRunner(), new FakeAudio(600, folder))
                .RefineAsync(lines, matched);

            Assert.Equal(0, result.Refined);
            Assert.Equal(100, lines[0].StartTime.TotalSeconds, 2);
            Assert.Equal(105, lines[1].StartTime.TotalSeconds, 2);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    private sealed class ThrowingRunner : ForcedAligner.IRunner
    {
        public Task<string> AlignAsync(string audioFileName, string textFileName, CancellationToken cancellationToken)
            => throw new ForcedAlignerException("the aligner fell over");
    }

    private static SubtitleLineViewModel Line(string text, double startSeconds, double endSeconds) => new()
    {
        Text = text,
        StartTime = TimeSpan.FromSeconds(startSeconds),
        EndTime = TimeSpan.FromSeconds(endSeconds),
    };
}
