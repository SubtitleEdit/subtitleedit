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
/// and SE_RETIME_TEST_SRT (accurately timed subtitle for that audio).
/// </summary>
public class SubtitleRetimerIntegrationTests
{
    private static string? Audio => Environment.GetEnvironmentVariable("SE_RETIME_TEST_AUDIO");
    private static string? Srt => Environment.GetEnvironmentVariable("SE_RETIME_TEST_SRT");
    private static string? Executable => Environment.GetEnvironmentVariable("SE_ALIGN_TEST_CRISPASR");
    private static string? Model => Environment.GetEnvironmentVariable("SE_ALIGN_TEST_ALIGNER");

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
