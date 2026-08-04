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
        Se.Settings.General.SubtitleMaximumDisplayMilliseconds = 8000;
        Se.Settings.General.SubtitleOptimalCharactersPerSeconds = 15.0;
    }

    /// <summary>
    /// Records progress reports on the reporting thread, synchronously.
    /// <para>
    /// <see cref="Progress{T}"/> posts to the captured synchronization context instead, so the
    /// reports land some unknown time after the run finishes - the test used to sleep 200 ms and
    /// hope, which failed under load, and appended to a plain List from a pool thread while the
    /// assertions read it. Reporting inline removes both problems: nothing here needs the
    /// callbacks marshalled anywhere.
    /// </para>
    /// </summary>
    private sealed class CollectingProgress : IProgress<ForcedAligner.Progress>
    {
        private readonly List<ForcedAligner.Progress> _reports = new();

        public IReadOnlyList<ForcedAligner.Progress> Reports
        {
            get
            {
                lock (_reports)
                {
                    return _reports.ToList();
                }
            }
        }

        public void Report(ForcedAligner.Progress value)
        {
            lock (_reports)
            {
                _reports.Add(value);
            }
        }
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

            var result = await new ForcedAligner(runner, audio).AlignAsync(lines, null, TestContext.Current.CancellationToken);

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

            // The bug this guards is catastrophic drift - the script sliding minutes behind
            // the audio - which measured in the hundreds of seconds. A chunk boundary can
            // cost a few seconds against the fake's constant speaking rate; that is noise.
            Assert.True(worst < 15.0, $"worst line was {worst:F1} s away from where it is spoken");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task ShortAudio_AlignsEveryLineInOrder()
    {
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var audio = new FakeAudio(60, folder);
            var lines = Script(12);

            var result = await new ForcedAligner(new FakeRunner(60), audio).AlignAsync(lines, null, TestContext.Current.CancellationToken);

            // Chunks are small by design, so even short audio may take more than one pass -
            // what matters is that every line comes out timed and in order.
            Assert.Equal(12, result.AlignedLines);
            Assert.Equal(0, result.UnalignedLines);
            for (var i = 1; i < lines.Count; i++)
            {
                Assert.True(lines[i].StartTime >= lines[i - 1].StartTime, $"line {i} is out of order");
            }
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

            var result = await new ForcedAligner(new FakeRunner(240), audio).AlignAsync(lines, null, TestContext.Current.CancellationToken);

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
            var progress = new CollectingProgress();

            await new ForcedAligner(new FakeRunner(240), audio)
                .AlignAsync(Script(300), progress, TestContext.Current.CancellationToken);

            var reports = progress.Reports;

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

    /// <summary>
    /// Stands in for an aligner on sparse audio: it ends each cue where the next one
    /// begins, so a line followed by music or action absorbs all of it.
    /// </summary>
    private sealed class StretchingRunner : ForcedAligner.IRunner
    {
        public Task<string> AlignAsync(string audioFileName, string textFileName, CancellationToken cancellationToken)
        {
            var lines = File.ReadAllLines(textFileName).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            var srt = new System.Text.StringBuilder();
            var t = 0.0;
            for (var i = 0; i < lines.Count; i++)
            {
                // 3 s of speech, then a 12 s gap swallowed by the same cue.
                var end = t + 15.0;
                srt.AppendLine((i + 1).ToString());
                srt.AppendLine($"{Fmt(t)} --> {Fmt(end)}");
                srt.AppendLine(lines[i]);
                srt.AppendLine();
                t = end;
            }

            return Task.FromResult(srt.ToString());

            static string Fmt(double seconds)
            {
                var t = TimeSpan.FromSeconds(seconds);
                return $"{t.Hours:00}:{t.Minutes:00}:{t.Seconds:00},{t.Milliseconds:000}";
            }
        }
    }

    [Fact]
    public async Task CuesStretchedOverSilence_AreTrimmedToReadingTime()
    {
        // A forced aligner ends a segment where the next one starts, so on a recording
        // that is half music the cue before each gap runs for the whole gap. Measured on
        // 56% speech audio this produced 40 cues over ten seconds, one of them 16.8 s,
        // against a true maximum of 4.4 s.
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var lines = Script(20);

            await new ForcedAligner(new StretchingRunner(), new FakeAudio(300, folder)).AlignAsync(lines, null, TestContext.Current.CancellationToken);

            var maxDurationMs = Se.Settings.General.SubtitleMaximumDisplayMilliseconds;
            var timed = lines.Where(l => l.EndTime > TimeSpan.Zero).ToList();
            Assert.True(timed.Count >= 15, $"expected most lines to get time codes, only {timed.Count} did");
            foreach (var line in timed)
            {
                Assert.True(
                    line.Duration.TotalMilliseconds <= maxDurationMs,
                    $"a cue lasted {line.Duration.TotalSeconds:F1} s, over the {maxDurationMs / 1000.0:F0} s maximum");
            }
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task BlankLinesInTheScript_DoNotShiftEveryFollowingLine()
    {
        // crispasr silently drops blank and whitespace-only rows from its --text-file:
        // feeding it 5 lines with 2 blanks returns 3 cues. Matching cue N to line N then
        // shifts every later line, cumulatively, for the rest of the file - which is what
        // "the timing is way off" looked like on a real script. FakeRunner reproduces the
        // dropping because it filters empty lines the same way.
        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var lines = new List<SubtitleLineViewModel>();
            for (var i = 0; i < 12; i++)
            {
                lines.Add(new SubtitleLineViewModel { Text = $"Spoken line {i} with several words" });
                if (i % 3 == 2)
                {
                    lines.Add(new SubtitleLineViewModel { Text = i % 6 == 2 ? string.Empty : "   " });
                }
            }

            var spoken = lines.Where(l => !string.IsNullOrWhiteSpace(l.Text)).ToList();

            await new ForcedAligner(new FakeRunner(240), new FakeAudio(240, folder)).AlignAsync(lines, null, TestContext.Current.CancellationToken);

            // Each spoken line must land where the fake actually speaks it: 10 chars/sec
            // over the spoken lines only, blanks contributing nothing.
            var trueStart = 0.0;
            foreach (var line in spoken)
            {
                Assert.True(
                    Math.Abs(line.StartTime.TotalSeconds - trueStart) < 1.0,
                    $"\"{line.Text}\" starts at {line.StartTime.TotalSeconds:F1}s, spoken at {trueStart:F1}s");
                trueStart += line.Text.Length / 10.0;
            }
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
                .AlignAsync(new List<SubtitleLineViewModel>(), null, TestContext.Current.CancellationToken);

            Assert.Equal(0, result.TotalLines);
            Assert.Equal(0, result.AlignedLines);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
