using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The one step every CrispASR TTS engine's voice seeding shares: put a reference WAV into the
/// engine's voices folder at the sample rate that engine clones from, falling back to a plain
/// copy when ffmpeg cannot do it.
///
/// The engines differ in source folder, target rate and sidecar handling, so only this inner
/// step is shared - but it is the part with the traps: an ffmpeg that never exits used to hang
/// the seed forever, and a failed run left a truncated WAV behind that the "already seeded"
/// check then treated as a good voice for the rest of the install's life.
/// </summary>
public static class VoiceSeedHelper
{
    /// <summary>
    /// Reference WAVs are a few seconds each, so a resample is a second or two; past this
    /// something is wrong with the child process and the plain copy is the better answer.
    /// </summary>
    private static readonly TimeSpan ConvertTimeout = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Writes <paramref name="src"/> to <paramref name="dest"/> resampled to mono at
    /// <paramref name="sampleRateHz"/>, or copies it verbatim when ffmpeg cannot be run or does
    /// not produce a usable file. Does nothing when <paramref name="dest"/> already exists.
    /// Never throws: seeding is best-effort and must not take the voice list down with it.
    /// </summary>
    /// <param name="sampleRateHz">24000 for most backends, 16000 for CosyVoice3's s3tok.</param>
    /// <param name="enginePrefix">Engine name used in log lines, e.g. "OmniVoice (CrispASR)".</param>
    /// <param name="copyOnFailure">
    /// False for backends that reject a reference at any other sample rate (Qwen3 Base), where a
    /// verbatim copy would seed a voice the server refuses. Those skip the voice instead.
    /// </param>
    public static void CopyOrResample(
        string src,
        string dest,
        int sampleRateHz,
        string enginePrefix,
        bool copyOnFailure = true)
    {
        if (File.Exists(dest))
        {
            return;
        }

        try
        {
            if (TryConvert(src, dest, sampleRateHz, enginePrefix))
            {
                return;
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"{enginePrefix}: resample seed '{src}' failed; falling back to plain copy");
        }

        // The conversion failed, so whatever it wrote is not a voice - and a truncated WAV is
        // still non-empty, so "does the file exist / is it non-empty" cannot be the test here.
        // Drop it unconditionally, or the copy below is skipped and the engine keeps a broken
        // voice for the rest of the install's life.
        TryDelete(dest);

        if (!copyOnFailure)
        {
            return;
        }

        try
        {
            if (!File.Exists(dest))
            {
                File.Copy(src, dest);
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"{enginePrefix}: seeding '{src}' failed");
        }
    }

    private static bool TryConvert(string src, string dest, int sampleRateHz, string enginePrefix)
    {
        using var ffmpeg = sampleRateHz == 16000
            ? FfmpegGenerator.ConvertToMono16kHzWav(src, dest)
            : FfmpegGenerator.ConvertToMono24kHzWav(src, dest);
        if (!ffmpeg.Start())
        {
            // ffmpeg unavailable - the caller's plain copy seeds the voice at its original rate.
            return false;
        }

        if (!ffmpeg.WaitForExit((int)ConvertTimeout.TotalMilliseconds))
        {
            Se.LogError($"{enginePrefix}: resample seed '{src}' timed out after {ConvertTimeout.TotalSeconds:0} seconds; falling back to plain copy");
            try
            {
                ffmpeg.Kill(entireProcessTree: true);
            }
            catch
            {
                // Already gone, or not ours to kill - either way the fallback copy still applies.
            }

            return false;
        }

        if (ffmpeg.ExitCode != 0)
        {
            Se.LogError($"{enginePrefix}: resample seed '{src}' exited with code {ffmpeg.ExitCode}; falling back to plain copy");
            return false;
        }

        return IsUsable(dest);
    }

    private static bool IsUsable(string path)
    {
        try
        {
            return File.Exists(path) && new FileInfo(path).Length > 0;
        }
        catch
        {
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Best-effort. If it cannot be removed the fallback copy is skipped, but the
            // failure that got us here is already logged above.
        }
    }
}
