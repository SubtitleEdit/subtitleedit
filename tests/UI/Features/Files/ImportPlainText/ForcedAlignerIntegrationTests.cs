using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.ImportPlainText;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Files.ImportPlainText;

/// <summary>
/// Exercises the real <c>crispasr --align-only</c> path end to end. Everything needed
/// (the engine binary, an aligner GGUF, ffmpeg, and a test recording) is large and
/// machine-specific, so these self-skip unless a machine happens to have them - they are
/// here to be run by hand when touching the aligner, not to gate CI.
///
/// Set SE_ALIGN_TEST_AUDIO to a WAV file and SE_ALIGN_TEST_SCRIPT to a matching text file
/// (one spoken line per row) to run them.
/// </summary>
public class ForcedAlignerIntegrationTests
{
    private static string? Audio => Environment.GetEnvironmentVariable("SE_ALIGN_TEST_AUDIO");
    private static string? Script => Environment.GetEnvironmentVariable("SE_ALIGN_TEST_SCRIPT");
    private static string? Executable => Environment.GetEnvironmentVariable("SE_ALIGN_TEST_CRISPASR");
    private static string? Model => Environment.GetEnvironmentVariable("SE_ALIGN_TEST_ALIGNER");

    private static bool Ready =>
        File.Exists(Audio) && File.Exists(Script) && File.Exists(Executable) && File.Exists(Model);

    /// <summary>
    /// The case forced alignment alone cannot do: a script that omits something actually
    /// spoken. Needs SE_ALIGN_TEST_TRANSCRIPT pointing at an ASR transcript of the audio
    /// on top of the variables the other test uses.
    /// </summary>
    [Fact]
    public async Task RealHybrid_AlignsAScriptThatOmitsSpokenText()
    {
        var transcriptPath = Environment.GetEnvironmentVariable("SE_ALIGN_TEST_TRANSCRIPT");
        if (!Ready || !File.Exists(transcriptPath))
        {
            return;
        }

        Se.Settings.General.UseFrameMode = false;
        Se.Settings.General.MinimumBetweenLines = new MsOrFramesValue { Milliseconds = 24 };
        Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;
        Se.Settings.General.SubtitleMaximumDisplayMilliseconds = 8000;
        Se.Settings.General.SubtitleOptimalCharactersPerSeconds = 15.0;

        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var lines = File.ReadAllLines(Script!)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => new SubtitleLineViewModel { Text = l.Trim() })
                .ToList();

            var transcription = new Subtitle();
            new SubRip().LoadSubtitle(transcription, File.ReadAllLines(transcriptPath!).ToList(), transcriptPath!);
            Assert.NotEmpty(transcription.Paragraphs);

            var coarse = ScriptSyncService.SyncScript(lines, transcription);

            var totalSeconds = new FileInfo(Audio!).Length / (16000.0 * 2);
            using var audio = new FfmpegWindowAudioSource("ffmpeg", Audio!, totalSeconds, folder);
            var runner = new CrispAsrAlignOnlyRunner(Executable!, Model!);

            var result = await new HybridAligner(runner, audio).RefineAsync(lines, coarse.DirectMatches);

            Assert.True(coarse.MatchedLines > lines.Count * 0.8,
                $"only {coarse.MatchedLines} of {lines.Count} lines were found in the transcription");
            Assert.True(result.Refined > 0, "no line had its time codes refined");

            var dump = Environment.GetEnvironmentVariable("SE_ALIGN_TEST_DUMP");
            if (!string.IsNullOrEmpty(dump))
            {
                File.WriteAllLines(dump, lines.Select(l =>
                    l.StartTime.TotalSeconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + " " +
                    l.EndTime.TotalSeconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)));
            }
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task RealAligner_AlignsAScriptInChunks()
    {
        if (!Ready)
        {
            return;
        }

        Se.Settings.General.UseFrameMode = false;
        Se.Settings.General.MinimumBetweenLines = new MsOrFramesValue { Milliseconds = 24 };
        Se.Settings.General.SubtitleMinimumDisplayMilliseconds = 1000;

        var folder = Directory.CreateTempSubdirectory().FullName;
        try
        {
            var lines = File.ReadAllLines(Script!)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => new SubtitleLineViewModel { Text = l.Trim() })
                .ToList();

            var totalSeconds = new FileInfo(Audio!).Length / (16000.0 * 2);
            using var audio = new FfmpegWindowAudioSource("ffmpeg", Audio!, totalSeconds, folder);
            var runner = new CrispAsrAlignOnlyRunner(Executable!, Model!);

            var result = await new ForcedAligner(runner, audio).AlignAsync(lines);

            Assert.True(result.AlignedLines > lines.Count * 0.95,
                $"only {result.AlignedLines} of {lines.Count} lines aligned");

            for (var i = 1; i < result.AlignedLines; i++)
            {
                Assert.True(lines[i].StartTime >= lines[i - 1].EndTime, $"line {i} overlaps line {i - 1}");
            }

            Assert.True(lines[0].StartTime.TotalSeconds < 30, "the first line should land near the start of the audio");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
