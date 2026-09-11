using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// Speaks every line in the voice that says it in the video: before synthesis each subtitle line's
/// own audio is cut out of the video and handed to the engine as that line's cloning reference.
/// </summary>
/// <remarks>
/// The point is that a video with several speakers dubs itself without any setup - no reference
/// recording to find, cut, name and import, and no cast to assign. Whoever speaks line 12 in the
/// original speaks line 12 in the dub, because the reference for line 12 *is* line 12.
///
/// It needs an engine that takes a reference per synthesis call. Engines that read the reference
/// once at server start (most of the CrispASR ones) would restart and reload the model for every
/// single line, so they are excluded via <see cref="ITtsEngine.SupportsPerLineVoiceCloning"/>.
/// </remarks>
public static class PerLineVoiceClone
{
    /// <summary>
    /// How long a reference clip should be, in seconds. Short clips are what make zero-shot clones
    /// sound wrong, so a clip below this is grown into the silence around the line (never into a
    /// neighbouring line, which would fold another speaker into the reference).
    /// </summary>
    internal const double PreferredReferenceSeconds = 3.0;

    /// <summary>
    /// The shortest reference clip handed to an engine, in seconds. A line that is shorter and
    /// has no silence to grow into (neighbouring lines on both sides) is padded with trailing
    /// silence up to this instead of being sent as cut.
    /// </summary>
    /// <remarks>
    /// Higgs Audio v3's reference encoder in audio.cpp (higgs_audio_tts/codec.cpp) pads its
    /// 24 kHz acoustic branch up to one second but not its 16 kHz semantic branch, so every
    /// reference under roughly 965 ms fails the "frame counts do not match" check with a 500
    /// and the line goes missing from the dub (#14480). One second of audio clears it for all
    /// lengths; the added silence is inaudible in the clone.
    /// </remarks>
    internal const double MinimumReferenceSeconds = 1.0;

    /// <summary>Reference clips are cut at the rate the cloning models work at.</summary>
    private const int ReferenceSampleRate = 24000;

    /// <summary>ffmpeg cuts are IO bound and independent, so a few run at once.</summary>
    private const int MaxParallelCuts = 4;

    /// <summary>The pseudo-voice the user picks to turn this on.</summary>
    public static Voice CreateVoice() =>
        new Voice(new PerLineCloneVoice(), Se.Language.Video.TextToSpeech.CloneVoicePerLine);

    /// <summary>True when <paramref name="voice"/> is the marker rather than a real voice.</summary>
    public static bool IsSelected(Voice? voice) => voice?.EngineVoice is PerLineCloneVoice;

    /// <summary>
    /// Whether the pseudo-voice should be offered at all: the engine has to take a per-call
    /// reference, and there has to be a video to take the references from.
    /// </summary>
    public static bool CanBeOffered(ITtsEngine? engine, string? videoFileName) =>
        engine?.SupportsPerLineVoiceCloning == true && !string.IsNullOrEmpty(videoFileName);

    /// <summary>
    /// Cuts one reference clip per paragraph, in parallel, and writes each clip's spoken text
    /// beside it as the sidecar the cloning engines read as ref-text.
    /// </summary>
    /// <param name="referenceTextOf">
    /// What is spoken in the video for a paragraph - the original-language line when a source
    /// subtitle is loaded, which is what the reference clip actually contains - or null when
    /// that is unknown. Unknown gets no sidecar; the line's own (possibly translated) text is
    /// never written as a stand-in, because a transcript that is the translation of the clip
    /// makes the engines replay the clip instead of speaking the line (#14480).
    /// </param>
    /// <returns>
    /// The clip per paragraph. Paragraphs whose cut failed are absent rather than mapped to a
    /// broken file, so the caller can fall back to a normal voice for those lines.
    /// </returns>
    public static async Task<Dictionary<Paragraph, string>> CutReferenceClipsAsync(
        string videoFileName,
        IReadOnlyList<Paragraph> paragraphs,
        Func<Paragraph, string?> referenceTextOf,
        string outputFolder,
        double videoDurationSeconds,
        int audioTrackFfIndex,
        Action<int, int>? progress,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(outputFolder);

        var clips = new Dictionary<Paragraph, string>();
        var clipsLock = new object();
        var done = 0;
        using var throttle = new SemaphoreSlim(MaxParallelCuts);

        var cuts = paragraphs.Select(async (paragraph, index) =>
        {
            await throttle.WaitAsync(cancellationToken);
            try
            {
                var clipFileName = await CutReferenceClipAsync(
                    videoFileName,
                    paragraphs,
                    index,
                    referenceTextOf(paragraph),
                    outputFolder,
                    videoDurationSeconds,
                    audioTrackFfIndex,
                    cancellationToken);
                if (clipFileName == null)
                {
                    return;
                }

                lock (clipsLock)
                {
                    clips[paragraph] = clipFileName;
                }
            }
            finally
            {
                throttle.Release();
                progress?.Invoke(Interlocked.Increment(ref done), paragraphs.Count);
            }
        });

        await Task.WhenAll(cuts);
        return clips;
    }

    /// <summary>
    /// Cuts the reference clip for one paragraph and writes its transcript sidecar when
    /// <paramref name="referenceText"/> is known. Returns null when the clip could not be
    /// produced - the caller decides what that line falls back to.
    /// </summary>
    public static async Task<string?> CutReferenceClipAsync(
        string videoFileName,
        IReadOnlyList<Paragraph> paragraphs,
        int index,
        string? referenceText,
        string outputFolder,
        double videoDurationSeconds,
        int audioTrackFfIndex,
        CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(outputFolder);

            var range = GetReferenceRange(paragraphs, index, videoDurationSeconds);
            var clipFileName = Path.Combine(outputFolder, $"line-{index + 1:0000}.wav");
            if (!await CutClipAsync(videoFileName, range.StartSeconds, range.DurationSeconds, clipFileName, audioTrackFfIndex, cancellationToken, MinimumReferenceSeconds))
            {
                return null;
            }

            // The engines that clone from a recording read what is spoken in it from a sibling
            // .txt. When the caller knows it (an original-language subtitle is loaded) nobody
            // has to type it or run speech-to-text over the clip; when it does not, no sidecar
            // is written and each engine decides what an unknown transcript means to it - a
            // stale .txt from an earlier cut into the same folder must not survive either.
            var sidecar = Path.ChangeExtension(clipFileName, ".txt");
            if (!string.IsNullOrWhiteSpace(referenceText))
            {
                await File.WriteAllTextAsync(sidecar, referenceText, cancellationToken);
            }
            else if (File.Exists(sidecar))
            {
                File.Delete(sidecar);
            }

            return clipFileName;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"Per-line voice clone: cutting the reference for line {index + 1} failed");
            return null;
        }
    }

    /// <summary>
    /// Cuts exactly the given range out of the video as a cloning-ready mono clip.
    /// </summary>
    /// <param name="minimumSeconds">
    /// Pad the clip with trailing silence up to this length when the range is shorter; zero
    /// keeps the range as is. The per-line references pass <see cref="MinimumReferenceSeconds"/>;
    /// the auto-cast parts do not, since they are joined into one long reference anyway.
    /// </param>
    /// <returns>False when ffmpeg failed or produced no audio; the caller decides what that means.</returns>
    public static async Task<bool> CutClipAsync(
        string videoFileName,
        double startSeconds,
        double durationSeconds,
        string outputFileName,
        int audioTrackFfIndex,
        CancellationToken cancellationToken,
        double minimumSeconds = 0)
    {
        var arguments = FfmpegGenerator.ExtractCloneReferenceClipParameters(
            videoFileName,
            startSeconds,
            durationSeconds,
            outputFileName,
            audioTrackFfIndex,
            ReferenceSampleRate,
            minimumSeconds);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputFileName)!);

            using var process = FfmpegGenerator.GetProcess(arguments, (_, _) => { });
            await process.StartAndWaitAsync(cancellationToken);

            // A 44-byte file is a WAV header with no samples after it - ffmpeg "succeeded" on a
            // range with nothing in it, and handing that to a cloning model is worse than not
            // cloning at all.
            if (process.ExitCode != 0 || !File.Exists(outputFileName) || new FileInfo(outputFileName).Length <= 44)
            {
                Se.WriteToolsLog($"Voice clone: no audio cut from the video ({arguments})");
                return false;
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Se.LogError(exception, $"Voice clone: cutting audio from the video failed ({arguments})");
            return false;
        }
    }

    /// <summary>
    /// The slice of video to clone line <paramref name="index"/> from: the line itself, grown
    /// symmetrically towards <see cref="PreferredReferenceSeconds"/> when it is shorter.
    /// </summary>
    /// <remarks>
    /// Growing stops at the neighbouring lines. A one-second line surrounded by other speakers
    /// stays one second long - a short reference clones a bit worse, but a reference with two
    /// speakers in it clones the wrong person, which is not a bit worse.
    /// </remarks>
    internal static (double StartSeconds, double DurationSeconds) GetReferenceRange(
        IReadOnlyList<Paragraph> paragraphs,
        int index,
        double videoDurationSeconds)
    {
        var paragraph = paragraphs[index];
        var start = Math.Max(0, paragraph.StartTime.TotalSeconds);
        var end = Math.Max(start, paragraph.EndTime.TotalSeconds);

        var missing = PreferredReferenceSeconds - (end - start);
        if (missing > 0)
        {
            var earliest = index > 0 ? Math.Max(0, paragraphs[index - 1].EndTime.TotalSeconds) : 0;
            var latest = index < paragraphs.Count - 1
                ? paragraphs[index + 1].StartTime.TotalSeconds
                : double.MaxValue;
            if (videoDurationSeconds > 0)
            {
                latest = Math.Min(latest, videoDurationSeconds);
            }

            start = Math.Max(earliest, start - missing / 2);
            end = Math.Min(latest, end + missing / 2);

            // One side may have had no room; spend what is left on the other.
            var stillMissing = PreferredReferenceSeconds - (end - start);
            if (stillMissing > 0)
            {
                start = Math.Max(earliest, start - stillMissing);
                end = Math.Min(latest, end + (PreferredReferenceSeconds - (end - start)));
            }
        }

        return (start, Math.Max(0.1, end - start));
    }

    /// <summary>
    /// Every engine that knows how to clone per line. Taken from the shared catalog rather than
    /// listed here, so an engine added there (implementing <see cref="IPerLineCloneEngine"/>) is
    /// covered by <see cref="TryGetReferenceClip"/> for free.
    /// </summary>
    private static readonly Lazy<IPerLineCloneEngine[]> CloneEngines = new(() =>
        TtsEngineCatalog.CreateVoiceCloningEngines().OfType<IPerLineCloneEngine>().ToArray());

    /// <summary>
    /// Wraps a cut clip as a voice <paramref name="engine"/> understands.
    /// </summary>
    /// <remarks>
    /// The per-engine knowledge lives on the engine itself, behind
    /// <see cref="IPerLineCloneEngine"/>. Returning null means the engine could not use the clip
    /// as a reference (e.g. no transcript beside it) - the caller then falls back to the ordinary
    /// voice rather than silently synthesising with the wrong speaker. An engine that sets
    /// <see cref="ITtsEngine.SupportsPerLineVoiceCloning"/> without implementing the interface is
    /// a wiring bug, not a fallback case, and asserts in debug builds.
    /// </remarks>
    /// <param name="voiceName">
    /// What to call the voice, for the rows and lists that show it. Defaults to the clip's file
    /// name; an imported session passes the name the clip had when it was exported, so a line
    /// keeps the voice name it was generated with instead of picking up the exported clip's.
    /// </param>
    public static Voice? MakeVoiceForClip(ITtsEngine engine, string clipFileName, string? voiceName = null)
    {
        if (engine is not IPerLineCloneEngine cloneEngine)
        {
            // The capability flag promises per-line cloning; the interface is how it is
            // delivered. One without the other would offer "Clone from video" and then dub
            // every line in the fallback voice.
            Debug.Assert(!engine.SupportsPerLineVoiceCloning,
                $"{engine.Name} sets {nameof(ITtsEngine.SupportsPerLineVoiceCloning)} but does not implement {nameof(IPerLineCloneEngine)}");
            return null;
        }

        var name = string.IsNullOrEmpty(voiceName) ? Path.GetFileNameWithoutExtension(clipFileName) : voiceName;
        return cloneEngine.MakePerLineCloneVoice(clipFileName, name);
    }

    /// <summary>
    /// The recording <paramref name="voice"/> clones from, or null when it clones from nothing -
    /// or when no engine could be handed it back.
    /// </summary>
    /// <remarks>
    /// Asks every <see cref="IPerLineCloneEngine"/> whether the voice is its own; each engine
    /// keeps <see cref="IPerLineCloneEngine.MakePerLineCloneVoice"/> and
    /// <see cref="IPerLineCloneEngine.GetPerLineReferenceClip"/> in step in one place. The
    /// callers are export (which copies the recording so the session can be re-imported
    /// elsewhere) and regenerate (which reuses it), and a reference no engine can be handed back
    /// is worth neither.
    /// </remarks>
    public static string? TryGetReferenceClip(Voice? voice)
    {
        if (voice == null)
        {
            return null;
        }

        foreach (var engine in CloneEngines.Value)
        {
            if (engine.GetPerLineReferenceClip(voice) is { } clip)
            {
                return clip;
            }
        }

        return null;
    }

    /// <summary>
    /// Clears what a previous run staged inside an engine's own folders, and is called as a
    /// per-line run starts.
    /// </summary>
    /// <remarks>
    /// Engines that can only read a reference from their voices folder keep a copy of every
    /// line's clip there (see <see cref="IPerLineCloneEngine.MakePerLineCloneVoice"/>). Cutting
    /// the clips into a fresh folder each run - which the caller does - says nothing about those
    /// copies, so a run over a shorter subtitle than the last would leave the previous run's
    /// extra lines behind. Engines that take the clip's own path stage nothing and implement
    /// this as a no-op.
    /// </remarks>
    public static void ResetStagedReferences(ITtsEngine? engine) =>
        (engine as IPerLineCloneEngine)?.ResetStagedPerLineReferences();
}
