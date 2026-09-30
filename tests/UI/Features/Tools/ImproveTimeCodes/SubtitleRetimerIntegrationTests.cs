using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Tools.ImproveTimeCodes;

namespace UITests.Features.Tools.ImproveTimeCodes;

/// <summary>
/// Runs the real <c>crispasr --align-only</c> against a well-timed subtitle whose times have
/// been knocked about, and checks the retimer settles them in the same place whatever the error going in. Self-skips unless the machine
/// has everything (see <c>ForcedAlignerIntegrationTests</c> for why).
///
/// Set SE_ALIGN_TEST_CRISPASR, SE_ALIGN_TEST_ALIGNER, SE_RETIME_TEST_AUDIO (16 kHz mono WAV)
/// and SE_RETIME_TEST_SRT (accurately timed subtitle for that audio). The speech-to-text check
/// also needs SE_RETIME_TEST_STT_MODEL (a Parakeet v3 model).
/// </summary>
public class SubtitleRetimerIntegrationTests
{
    private static string? Audio => Environment.GetEnvironmentVariable("SE_RETIME_TEST_AUDIO");
    private static string? Srt => Environment.GetEnvironmentVariable("SE_RETIME_TEST_SRT");
    private static string? Executable => Environment.GetEnvironmentVariable("SE_ALIGN_TEST_CRISPASR");
    private static string? Model => Environment.GetEnvironmentVariable("SE_ALIGN_TEST_ALIGNER");
    private static string? SttModel => Environment.GetEnvironmentVariable("SE_RETIME_TEST_STT_MODEL");

    [Fact]
    public async Task RealAligner_PullsJitteredTimesBackTowardsTheTruth()
    {
        if (!File.Exists(Audio) || !File.Exists(Srt) || !File.Exists(Executable) || !File.Exists(Model))
        {
            return;
        }

        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, File.ReadAllLines(Srt!).ToList(), Srt!);
        var truth = subtitle.Paragraphs;

        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var totalSeconds = new FileInfo(Audio!).Length / (16000.0 * 2);
            using var audio = new FfmpegWindowAudioSource("ffmpeg", Audio!, totalSeconds, folder);
            var runner = new CrispAsrAlignOnlyRunner(Executable!, Model!);

            var first = await RetimeJitteredAsync(truth, 42, runner, audio);
            var second = await RetimeJitteredAsync(truth, 7, runner, audio);

            // The reference subtitle is only roughly right itself, so it cannot be the yardstick.
            // What can be measured is that the result does not depend on the error going in: two
            // differently jittered copies have to come out in the same place, to within a couple
            // of aligner frames (80 ms each).
            var spread = first.Output.Zip(second.Output, (a, b) => Math.Abs(a.StartSeconds - b.StartSeconds)).Average();
            var spreadIn = first.Input.Zip(second.Input, (a, b) => Math.Abs(a.StartSeconds - b.StartSeconds)).Average();
            var report = string.Join("\n", truth.Select((p, i) =>
                $"{i + 1,3} {first.Output[i].Status,-13} ref {p.StartTime.TotalSeconds,7:0.00}-{p.EndTime.TotalSeconds,7:0.00}  " +
                $"a {first.Output[i].StartSeconds,7:0.00}-{first.Output[i].EndSeconds,7:0.00}  b {second.Output[i].StartSeconds,7:0.00}-{second.Output[i].EndSeconds,7:0.00}"));

            Assert.True(spread < 0.1, $"mean start spread {spreadIn:0.000} s -> {spread:0.000} s\n{report}");
            Assert.True(first.Output.Count(r => r.Status == SubtitleRetimer.LineStatus.Retimed) > truth.Count * 0.8, report);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task RealSpeechToText_ConfirmsLoneLargeMovesThatAreRight()
    {
        if (!File.Exists(Audio) || !File.Exists(Srt) || !File.Exists(Executable) || !File.Exists(Model) || !File.Exists(SttModel))
        {
            return;
        }

        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, File.ReadAllLines(Srt!).ToList(), Srt!);
        var truth = subtitle.Paragraphs;

        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var totalSeconds = new FileInfo(Audio!).Length / (16000.0 * 2);
            using var audio = new FfmpegWindowAudioSource("ffmpeg", Audio!, totalSeconds, folder);
            var runner = new CrispAsrAlignOnlyRunner(Executable!, Model!);
            var copy = Path.Combine(folder, "audio.wav");
            File.Copy(Audio!, copy);
            var heard = await new CrispAsrWordTranscriber(Executable!, "parakeet", SttModel!, CrispAsrWordTranscriber.FindVadModel(Path.GetDirectoryName(Executable!)!))
                .TranscribeAsync(copy, "en", null, TestContext.Current.CancellationToken);

            // Every fifth line is a second late on its own: the neighbours stay put, so without
            // speech-to-text each of those moves could only be offered unticked.
            var lines = truth
                .Select((p, i) =>
                {
                    var shift = i % 5 == 2 ? 1.0 : 0.0;
                    return new SubtitleRetimer.Line(p.Text, p.StartTime.TotalSeconds + shift, p.EndTime.TotalSeconds + shift);
                })
                .ToList();

            var options = new SubtitleRetimer.Options { MaxShiftSeconds = 2.0 };
            var results = await new SubtitleRetimer(runner, audio, options)
                .RetimeAsync(lines, null, TestContext.Current.CancellationToken, heard);

            var report = string.Join("\n", truth.Select((p, i) =>
                $"{i + 1,3} {results[i].Status,-20} heard {results[i].HeardRatio ?? -1,5:0.00} ref {p.StartTime.TotalSeconds,7:0.00} " +
                $"in {lines[i].StartSeconds,7:0.00} out {results[i].StartSeconds,7:0.00}  {p.Text.Replace("\n", " ").Replace("\r", string.Empty)}"));

            var displaced = Enumerable.Range(0, truth.Count).Where(i => i % 5 == 2).ToList();
            var confirmed = displaced.Count(i => results[i].Status == SubtitleRetimer.LineStatus.ConfirmedBySpeech);
            var disputedInPlace = Enumerable.Range(0, truth.Count)
                .Count(i => i % 5 != 2 && results[i].Status == SubtitleRetimer.LineStatus.DisputedBySpeech);

            Assert.True(confirmed >= displaced.Count / 2 && disputedInPlace == 0,
                $"confirmed {confirmed} of {displaced.Count}, disputed in place {disputedInPlace}\n{report}");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task RealSpeechToText_SyncsAnOffsetAndDriftBeyondTheMaxShift()
    {
        if (!File.Exists(Audio) || !File.Exists(Srt) || !File.Exists(Executable) || !File.Exists(Model) || !File.Exists(SttModel))
        {
            return;
        }

        var subtitle = new Subtitle();
        new SubRip().LoadSubtitle(subtitle, File.ReadAllLines(Srt!).ToList(), Srt!);
        var truth = subtitle.Paragraphs;

        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var totalSeconds = new FileInfo(Audio!).Length / (16000.0 * 2);
            using var audio = new FfmpegWindowAudioSource("ffmpeg", Audio!, totalSeconds, folder);
            var runner = new CrispAsrAlignOnlyRunner(Executable!, Model!);
            var copy = Path.Combine(folder, "audio.wav");
            File.Copy(Audio!, copy);
            var heard = await new CrispAsrWordTranscriber(Executable!, "parakeet", SttModel!, CrispAsrWordTranscriber.FindVadModel(Path.GetDirectoryName(Executable!)!))
                .TranscribeAsync(copy, "en", null, TestContext.Current.CancellationToken);

            // Three seconds late, drifting a further two seconds a hundred seconds, with a little
            // jitter per line - far beyond the default max shift.
            var random = new Random(3);
            var lines = truth
                .Select(p =>
                {
                    var t = p.StartTime.TotalSeconds;
                    var shift = 3.0 + (t * 0.02) + ((random.NextDouble() - 0.5) * 0.4);
                    return new SubtitleRetimer.Line(p.Text, t + shift, p.EndTime.TotalSeconds + shift);
                })
                .ToList();

            var options = new SubtitleRetimer.Options();
            var sync = RoughSync.Measure(lines, heard);
            Assert.NotNull(sync);
            var synced = RoughSync.Apply(lines, sync!);
            var results = (await new SubtitleRetimer(runner, audio, options)
                .RetimeAsync(synced, null, TestContext.Current.CancellationToken, heard)).ToArray();
            RoughSync.Merge(lines, synced, results);

            var report = string.Join("\n", truth.Select((p, i) =>
                $"{i + 1,3} {results[i].Status,-20} ref {p.StartTime.TotalSeconds,7:0.00} in {lines[i].StartSeconds,7:0.00} " +
                $"sync {synced[i].StartSeconds,7:0.00} out {results[i].StartSeconds,7:0.00}  {p.Text.Replace("\n", " ").Replace("\r", string.Empty)}"));

            var spoken = Enumerable.Range(0, truth.Count).Where(i => results[i].Status != SubtitleRetimer.LineStatus.NoSpeech).ToList();
            var close = spoken.Count(i => Math.Abs(results[i].StartSeconds - truth[i].StartTime.TotalSeconds) < 0.3);
            Assert.True(close >= spoken.Count * 0.8, $"{close} of {spoken.Count} within 0.3 s, {sync!.AnchorLines} anchor lines\n{report}");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    private static async Task<(List<SubtitleRetimer.Line> Input, IReadOnlyList<SubtitleRetimer.LineResult> Output)> RetimeJitteredAsync(
        List<Paragraph> truth, int seed, ForcedAligner.IRunner runner, ForcedAligner.IAudioSource audio)
    {
        var random = new Random(seed);
        var jittered = truth
            .Select(p =>
            {
                var shift = (random.NextDouble() - 0.5) * 1.2; // +-600 ms
                return new SubtitleRetimer.Line(p.Text, Math.Max(0, p.StartTime.TotalSeconds + shift), p.EndTime.TotalSeconds + shift);
            })
            .ToList();

        // The jitter is larger than the default max shift, which would (rightly) refuse to follow it.
        var options = new SubtitleRetimer.Options { MaxShiftSeconds = 2.0 };
        var results = await new SubtitleRetimer(runner, audio, options).RetimeAsync(jittered, null, TestContext.Current.CancellationToken);
        return (jittered, results);
    }
}
