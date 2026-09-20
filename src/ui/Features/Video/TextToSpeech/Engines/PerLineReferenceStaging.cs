using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The file-copy half of per-line voice cloning for the CrispASR engines whose backend resolves a
/// request's <c>voice</c> as a bare name inside <c>--voice-dir</c>: the clip a line was cut from
/// has to be copied into the engine's own voices folder before its stem means anything to the
/// server. Shared by <see cref="VibeVoiceCrispAsr"/> and <see cref="MossTtsCrispAsr"/>; the
/// older engines (<see cref="PocketTtsCrispAsr"/>, <see cref="Qwen3TtsCrispAsr"/>) carry their
/// own copy of the same steps.
/// </summary>
public static class PerLineReferenceStaging
{
    /// <summary>
    /// The prefix that marks a staged per-line reference in a voices folder: it is what keeps the
    /// user's imported voices out of <see cref="Clear"/> and the staged ones out of the voice
    /// combo. Same value in every engine, so an exported session's clips are recognised by
    /// whichever engine re-imports them.
    /// </summary>
    public const string Prefix = "se-per-line-";

    /// <summary>True when <paramref name="fileName"/> is a reference staged for one line.</summary>
    public static bool IsStaged(string fileName) =>
        Path.GetFileName(fileName).StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Copies <paramref name="clipFileName"/> into <paramref name="voicesFolder"/> under the
    /// staged name and returns that path, or null when the clip is missing or the copy failed.
    /// A plain copy, not an ffmpeg pass: the clips are cut as 24 kHz mono PCM16 already. Any
    /// <c>.txt</c> transcript beside the clip travels with it, in the layout the engines' own
    /// import uses, so an exported session keeps it.
    /// </summary>
    /// <param name="engineLabel">Names the engine in the error log.</param>
    public static string? Stage(string clipFileName, string voicesFolder, string engineLabel)
    {
        try
        {
            if (string.IsNullOrEmpty(clipFileName) || !File.Exists(clipFileName))
            {
                return null;
            }

            // An exported session carries the staged name, so re-staging it on import would
            // otherwise pile a second prefix on top - once per round trip.
            var baseName = Path.GetFileNameWithoutExtension(clipFileName);
            if (baseName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
            {
                baseName = baseName.Substring(Prefix.Length);
            }

            var stagedFileName = Path.Combine(voicesFolder, Prefix + baseName + ".wav");

            // Regenerate hands back the clip a line was generated from, which IS the staged copy:
            // copying a file onto itself only throws, and it is already in place.
            if (string.Equals(Path.GetFullPath(stagedFileName), Path.GetFullPath(clipFileName), StringComparison.OrdinalIgnoreCase))
            {
                return clipFileName;
            }

            Directory.CreateDirectory(voicesFolder);
            File.Copy(clipFileName, stagedFileName, overwrite: true);

            var transcriptFileName = Path.ChangeExtension(clipFileName, ".txt");
            var stagedTranscriptFileName = Path.ChangeExtension(stagedFileName, ".txt");
            if (File.Exists(transcriptFileName))
            {
                File.Copy(transcriptFileName, stagedTranscriptFileName, overwrite: true);
            }
            else if (File.Exists(stagedTranscriptFileName))
            {
                // A stale sidecar from an earlier run would otherwise describe a different clip.
                File.Delete(stagedTranscriptFileName);
            }

            return stagedFileName;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"{engineLabel}: staging the per-line reference '{clipFileName}' failed");
            return null;
        }
    }

    /// <summary>
    /// Removes every staged per-line reference (and its transcript) from
    /// <paramref name="voicesFolder"/>, leaving the user's imported voices alone. Called when a
    /// per-line run starts, so a run over a shorter subtitle cannot leave the previous run's
    /// extra lines lying in the folder.
    /// </summary>
    public static void Clear(string voicesFolder, string engineLabel)
    {
        try
        {
            if (!Directory.Exists(voicesFolder))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(voicesFolder, Prefix + "*"))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // A reference still open (the server may be reading it) is left for next time.
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"{engineLabel}: clearing the staged per-line references failed");
        }
    }
}
