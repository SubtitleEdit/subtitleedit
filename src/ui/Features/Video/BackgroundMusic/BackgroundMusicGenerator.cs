using Avalonia.Controls;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Media;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.BackgroundMusic;

/// <summary>
/// A generated clip and where it loops. Kept un-looped so it can be rendered to any length later —
/// the TTS window only knows the final length once the speech is merged.
/// </summary>
public sealed class GeneratedMusic
{
    public required MusicAudio Clip { get; init; }
    public required MusicLoop Loop { get; init; }
    public required string Prompt { get; init; }
    public required int Bpm { get; init; }
    public required int GenerateSeconds { get; init; }

    /// <summary>The "seconds to generate" setting it was made with - <see cref="GenerateSeconds"/> is that capped for the target it was made for.</summary>
    public required int PreferredSeconds { get; init; }
    public required long Seed { get; init; }

    /// <summary>Looped (or trimmed) to <paramref name="targetSeconds"/>, faded out and normalized.</summary>
    public MusicAudio Render(double targetSeconds) =>
        MusicLooper.Normalize(MusicLooper.Render(Clip, Loop, targetSeconds));

    /// <summary>
    /// True when this clip was made with these settings and is long enough for
    /// <paramref name="targetSeconds"/> (null = any target). The generated length is not compared
    /// as such: it depends on the target, and the dialog (video length) and the text-to-speech run
    /// (speech length) have different targets - which threw away the music just listened to and
    /// made another, different clip.
    /// </summary>
    public bool Matches(string prompt, int bpm, int preferredSeconds, double? targetSeconds = null) =>
        Prompt == prompt.Trim() && Bpm == bpm && PreferredSeconds == preferredSeconds &&
        (targetSeconds == null || GenerateSeconds >= BackgroundMusicGenerator.GetGenerateSeconds(preferredSeconds, targetSeconds.Value));
}

/// <summary>Shared by Video &gt; Generate background music and the TTS window.</summary>
public static class BackgroundMusicGenerator
{
    /// <summary>
    /// The audio.cpp runtime with the <c>ace_step</c> family, its CLI, and the model — asking before
    /// each download. Returns false when the user declines or something is missing; throws
    /// <see cref="OperationCanceledException"/> when a download is cancelled.
    /// </summary>
    public static async Task<bool> EnsureInstalledAsync(
        Window window,
        IWindowService windowService,
        IAceStepAudioCppDownloadService downloadService,
        Action? onModelDownloadStarting,
        IProgress<float>? downloadProgress,
        CancellationToken cancellationToken)
    {
        var l = Se.Language.Video.BackgroundMusic;
        if (!await TtsVoiceInstaller.EnsureAudioCppRuntime(window, windowService, forceRedownload: false, AceStepAudioCpp.DisplayName, AceStepAudioCpp.FamilyName))
        {
            return false;
        }

        if (!File.Exists(AceStepAudioCpp.GetCliExecutable()))
        {
            await MessageBox.Show(window, l.UnableToGenerateMusic, "audiocpp_cli not found: " + AceStepAudioCpp.GetCliExecutable(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (AceStepAudioCpp.IsModelInstalled())
        {
            return true;
        }

        var answer = await MessageBox.Show(
            window,
            l.DownloadModelTitle,
            string.Format(l.DownloadModelQuestionX, AceStepAudioCpp.ModelSizeText),
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return false;
        }

        onModelDownloadStarting?.Invoke();
        try
        {
            await downloadService.DownloadModel(downloadProgress, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, "Background music: model download failed");
            await MessageBox.Show(window, Se.Language.General.DownloadFailed, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        return AceStepAudioCpp.IsModelInstalled();
    }

    /// <summary>Generates a clip and finds its loop. The runtime and model must be installed.</summary>
    public static async Task<GeneratedMusic> GenerateAsync(
        string prompt,
        int bpm,
        int preferredSeconds,
        double targetSeconds,
        long seed,
        string tempFolder,
        IProgress<MusicGenerationProgress>? progress,
        CancellationToken cancellationToken)
    {
        var generateSeconds = GetGenerateSeconds(preferredSeconds, targetSeconds);
        Directory.CreateDirectory(tempFolder);
        var rawFileName = Path.Combine(tempFolder, "generated-" + Guid.NewGuid().ToString("N") + ".wav");
        try
        {
            var request = new MusicGenerationRequest
            {
                Prompt = prompt.Trim(),
                Bpm = bpm,
                DurationSeconds = generateSeconds,
                Seed = seed,
                OutputFileName = rawFileName,
            };

            await AceStepAudioCpp.RunAsync(request, progress, cancellationToken);

            return await Task.Run(() =>
            {
                var clip = MusicAudio.ReadWav(rawFileName);
                var loop = MusicLooper.FindLoop(clip, bpm);
                Se.WriteToolsLog($"Background music: {generateSeconds} s clip, seed {seed}, loop {loop.StartSeconds:0.00}-{loop.EndSeconds:0.00} s " +
                                 $"(bar aligned: {loop.IsBarAligned}, bpm {loop.MeasuredBpm:0.0}, seam {loop.SeamScore:0.00})");
                return new GeneratedMusic
                {
                    Clip = clip,
                    Loop = loop,
                    Prompt = request.Prompt,
                    Bpm = bpm,
                    GenerateSeconds = generateSeconds,
                    PreferredSeconds = preferredSeconds,
                    Seed = seed,
                };
            }, cancellationToken);
        }
        finally
        {
            try
            {
                File.Delete(rawFileName);
            }
            catch
            {
                // best effort
            }
        }
    }

    /// <summary>
    /// How much to generate for <paramref name="targetSeconds"/> of music: a short target needs
    /// little more than itself (plus room for the model's outro), a long one loops a clip.
    /// </summary>
    public static int GetGenerateSeconds(int preferredSeconds, double targetSeconds) =>
        Math.Clamp(Math.Min(preferredSeconds, (int)Math.Ceiling(targetSeconds) + 10), 10, 240);

    /// <summary>
    /// Mixes music under speech with sidechain ducking: the music dips while someone talks and comes
    /// back up in the gaps. <paramref name="musicVolumePercent"/> sets its level between lines.
    /// <para>
    /// Every branch gets an explicit format after the split: ffmpeg 4.x cannot negotiate one for
    /// sidechaincompress otherwise ("could not choose their formats"). The sidechain key is padded
    /// with silence because sidechaincompress stops at its shorter input, which cut the music off
    /// where the speech ended even when the video runs longer.
    /// </para>
    /// </summary>
    public static string BuildMixUnderSpeechArguments(string speechFileName, string musicFileName, string outputFileName, int musicVolumePercent)
    {
        const string format = "aformat=sample_fmts=fltp:sample_rates=48000:channel_layouts=stereo";
        var volume = Math.Clamp(musicVolumePercent / 100.0, 0.0, 2.0).ToString("0.00", CultureInfo.InvariantCulture);
        return $"-y -i \"{speechFileName}\" -i \"{musicFileName}\" -filter_complex " +
               $"\"[0:a]asplit=2[s0][k0];[s0]{format}[speech];[k0]{format},apad[key];" +
               $"[1:a]volume={volume},{format}[music];" +
               "[music][key]sidechaincompress=threshold=0.02:ratio=6:attack=20:release=400[ducked];" +
               "[speech][ducked]amix=inputs=2:duration=longest:normalize=0[out]\" " +
               $"-map \"[out]\" -c:a pcm_s16le \"{outputFileName}\"";
    }

    public static async Task MixUnderSpeechAsync(string speechFileName, string musicFileName, string outputFileName, int musicVolumePercent, CancellationToken cancellationToken)
    {
        var arguments = BuildMixUnderSpeechArguments(speechFileName, musicFileName, outputFileName, musicVolumePercent);
        var log = new System.Text.StringBuilder();
        using var process = FfmpegGenerator.GetProcess(arguments, (_, e) =>
        {
            if (e.Data != null)
            {
                lock (log)
                {
                    log.AppendLine(e.Data);
                }
            }
        });

        Se.WriteToolsLog("Background music: mixing under speech: ffmpeg " + process.StartInfo.Arguments);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                process.Kill(true);
            }
            catch
            {
                // it may have exited in between
            }

            throw;
        }

        if (process.ExitCode != 0 || !File.Exists(outputFileName))
        {
            string tail;
            lock (log)
            {
                tail = string.Join(Environment.NewLine, log.ToString().Split('\n')[^Math.Min(8, log.ToString().Split('\n').Length)..]);
            }

            throw new InvalidOperationException("Mixing background music failed: " + tail);
        }
    }
}
