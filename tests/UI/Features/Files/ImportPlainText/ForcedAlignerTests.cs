using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Files.ImportPlainText;

public class ForcedAlignerTests
{
    public ForcedAlignerTests()
    {
        Se.Settings.General.UseFrameMode = false;
        Se.Settings.General.MinimumBetweenLines = new MsOrFramesValue { Milliseconds = 24 };
        Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
    }

    /// <summary>
    /// Stands in for ffmpeg: reports a duration and hands out window file names without
    /// touching the disk for audio.
    /// </summary>
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
            var path = Path.Combine(_folder, $"w{_index++}.wav");
            File.WriteAllText(path, "not really audio");
            return Task.FromResult(path);
        }
    }

    /// <summary>
    /// Stands in for the aligner without needing a 442 MB model. Lines are placed at a
    /// fixed speaking rate, the way real speech sits in real audio - text that doesn't
    /// fill the window ends early, and text that overflows gets compressed into the tail,
    /// which is exactly what a CTC aligner does when it runs short of audio.
    /// </summary>
    private sealed class FakeRunner : ForcedAligner.IRunner
    {
        private const double CharsPerSecond = 10.0;
        private readonly double _windowSeconds;

        public List<int> LinesFedPerWindow { get; } = new();

        public FakeRunner(double windowSeconds) => _windowSeconds = windowSeconds;

        public Task<string> AlignAsync(string audioFileName, string textFileName, CancellationToken cancellationToken)
        {
            var lines = File.ReadAllLines(textFileName).Where(l => l.Length > 0).ToList();
            LinesFedPerWindow.Add(lines.Count);

            var srt = new System.Text.StringBuilder();
            var t = 0.0;
            for (var i = 0; i < lines.Count; i++)
            {
                var duration = lines[i].Length / CharsPerSecond;

                // Once the window runs out of audio the remaining lines pile up in what is
                // left of it, which is how CTC alignment degrades when it is given more
                // text than was spoken: the earlier cues stay anchored to real speech and
                // only the excess is crammed into the tail.
                if (t + duration > _windowSeconds)
                {
                    var leftover = lines.Count - i;
                    var crammed = Math.Max(0.01, (_windowSeconds - t) / leftover);
                    for (var j = i; j < lines.Count; j++)
                    {
                        AppendCue(srt, j + 1, t, Math.Min(_windowSeconds, t + crammed), lines[j]);
                        t += crammed;
                    }

                    break;
                }

                AppendCue(srt, i + 1, t, t + duration - 0.05, lines[i]);
                t += duration;
            }

            return Task.FromResult(srt.ToString());

            static void AppendCue(System.Text.StringBuilder sb, int number, double startSeconds, double endSeconds, string text)
            {
                sb.AppendLine(number.ToString());
                sb.AppendLine($"{Fmt(TimeSpan.FromSeconds(startSeconds))} --> {Fmt(TimeSpan.FromSeconds(Math.Max(startSeconds, endSeconds)))}");
                sb.AppendLine(text);
                sb.AppendLine();
            }

            static string Fmt(TimeSpan t) => $"{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00},{t.Milliseconds:000}";
        }
    }

    private static List<SubtitleLineViewModel> Script(int count)
        => Enumerable.Range(0, count)
            .Select(i => new SubtitleLineViewModel { Text = $"Line number {i} with some words in it" })
            .ToList();

    [Fact]
    public async Task TwoHourAudio_IsAlignedInBoundedWindows()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            // 2 hours, ~1800 lines. The point is that no single window is ever handed the
            // whole file - that is what makes a full-length video alignable at all.
            var audio = new FakeAudio(7200, folder);
            var runner = new FakeRunner(240);
            var lines = Script(1800);

            var result = await new ForcedAligner(runner, audio).AlignAsync(lines);

            Assert.True(audio.Windows.Count >= 20, $"expected many windows, got {audio.Windows.Count}");
            Assert.All(audio.Windows, w => Assert.InRange(w.Duration, 1, 300));
            Assert.Equal(1800, result.TotalLines);
            Assert.True(result.AlignedLines > 1700, $"only {result.AlignedLines} of 1800 lines aligned");

            // Time codes must be ordered and inside the audio.
            for (var i = 1; i < result.AlignedLines; i++)
            {
                Assert.True(lines[i].StartTime >= lines[i - 1].EndTime, $"line {i} overlaps line {i - 1}");
            }

            Assert.True(lines[result.AlignedLines - 1].EndTime.TotalSeconds <= 7201);

            // The real check: every line must land where it is actually spoken. The fake
            // speaks at a constant 10 chars/second, so a line's true start is just the
            // characters before it divided by that rate. This is what catches the script
            // drifting behind the audio when only part of each window is trusted.
            var trueStart = 0.0;
            var worst = 0.0;
            for (var i = 0; i < result.AlignedLines; i++)
            {
                worst = Math.Max(worst, Math.Abs(lines[i].StartTime.TotalSeconds - trueStart));
                trueStart += lines[i].Text.Length / 10.0;
            }

            Assert.True(worst < 5.0, $"worst line was {worst:F1} s away from where it is spoken");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task ShortAudio_UsesASingleWindowAndAlignsEveryLine()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var audio = new FakeAudio(60, folder);
            var lines = Script(12);

            var result = await new ForcedAligner(new FakeRunner(60), audio).AlignAsync(lines);

            Assert.Single(audio.Windows);
            Assert.Equal(12, result.AlignedLines);
            Assert.Equal(0, result.UnalignedLines);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task EveryLineIsFedExactlyOnce_NoneDroppedOrDuplicated()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var audio = new FakeAudio(1200, folder);
            var lines = Script(300);

            var result = await new ForcedAligner(new FakeRunner(240), audio).AlignAsync(lines);

            // Losing a line at a window seam is the failure mode that matters here.
            Assert.Equal(300, result.AlignedLines);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task ProgressIsReportedPerWindow()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var audio = new FakeAudio(1200, folder);
            var reports = new List<ForcedAligner.Progress>();
            var progress = new Progress<ForcedAligner.Progress>(p => reports.Add(p));

            await new ForcedAligner(new FakeRunner(240), audio)
                .AlignAsync(Script(300), progress);

            // Progress<T> marshals asynchronously; give the posted callbacks a moment.
            await Task.Delay(200);

            Assert.NotEmpty(reports);
            Assert.All(reports, r => Assert.Equal(300, r.LinesTotal));
            Assert.True(reports.Last().LinesAligned >= reports.First().LinesAligned);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task Cancellation_StopsTheRun()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                new ForcedAligner(new FakeRunner(240), new FakeAudio(3600, folder))
                    .AlignAsync(Script(500), null, cts.Token));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task EmptyScript_IsNotAnError()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var result = await new ForcedAligner(new FakeRunner(240), new FakeAudio(600, folder))
                .AlignAsync(new List<SubtitleLineViewModel>());

            Assert.Equal(0, result.TotalLines);
            Assert.Equal(0, result.AlignedLines);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
