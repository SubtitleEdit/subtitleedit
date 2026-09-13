using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// How much an engine cares about the spoken transcript ("ref-text") of a reference recording.
/// </summary>
public enum TranscriptRequirement
{
    /// <summary>The engine clones from a speaker embedding; a transcript is never read.</summary>
    NotUsed,

    /// <summary>Cloning works without one, but is noticeably better with an accurate transcript.</summary>
    Optional,

    /// <summary>Synthesis fails, or falls back to a generic voice, without the transcript.</summary>
    Required,
}

/// <summary>
/// The one place that knows which engines want the transcript of a reference recording, and how
/// that transcript is stored: a <c>.txt</c> sidecar with the same base name as the WAV.
/// </summary>
/// <remarks>
/// The voice-settings import dialog grew a per-engine if/else chain carrying this knowledge in
/// comments; the voice manager needs the same answers for its transcript editor and for copying
/// a voice between engines, so it is spelled out once here. Engine additions: put the engine in
/// <see cref="GetRequirement"/> and in <see cref="VoiceCloneImporter"/>.
/// </remarks>
public static class VoiceReferenceTranscript
{
    public static TranscriptRequirement GetRequirement(ITtsEngine? engine)
    {
        return engine switch
        {
            // CosyVoice3's s3tok tokenizer and the Qwen3 Base backend load the sidecar as ref-text
            // and error without it; audio.cpp answers a Fish voice_ref without reference_text with
            // a server error; OmniVoice (tts.cpp) silently falls back to its default voice.
            CosyVoice3CrispAsr => TranscriptRequirement.Required,
            FishTtsAudioCpp => TranscriptRequirement.Required,
            Qwen3TtsCrispAsr => TranscriptRequirement.Required,
            OmniVoiceTtsCpp => TranscriptRequirement.Required,

            F5TtsCrispAsr => TranscriptRequirement.Optional,
            FireRedTts3AudioCpp => TranscriptRequirement.Optional,
            HiggsTtsAudioCpp => TranscriptRequirement.Optional,
            MossTtsCrispAsr => TranscriptRequirement.Optional,
            OmniVoiceCrispAsr => TranscriptRequirement.Optional,
            VoxCPM2CrispAsr => TranscriptRequirement.Optional,

            _ => TranscriptRequirement.NotUsed,
        };
    }

    public static string GetSidecarFileName(string referenceFileName) =>
        Path.ChangeExtension(referenceFileName, ".txt");

    /// <summary>
    /// The transcript stored beside <paramref name="referenceFileName"/>, or null when there is
    /// none worth showing: no sidecar, or one holding an attribution blurb / subtitle markup
    /// rather than what is spoken (see <see cref="Qwen3TtsCrispAsr.LooksLikeUnusableTranscript"/>).
    /// </summary>
    public static string? Read(string referenceFileName)
    {
        var sidecar = GetSidecarFileName(referenceFileName);
        if (!File.Exists(sidecar))
        {
            return null;
        }

        try
        {
            var text = File.ReadAllText(sidecar).Trim();
            return Qwen3TtsCrispAsr.LooksLikeUnusableTranscript(text) ? null : text;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Reading voice transcript '{sidecar}' failed");
            return null;
        }
    }

    /// <summary>
    /// Writes <paramref name="text"/> as the sidecar of <paramref name="referenceFileName"/>; an
    /// empty text removes the sidecar rather than leaving a file claiming the clip is silent.
    /// </summary>
    public static bool Write(string referenceFileName, string? text, out string error)
    {
        error = string.Empty;
        var sidecar = GetSidecarFileName(referenceFileName);
        try
        {
            var trimmed = (text ?? string.Empty).Trim();
            if (trimmed.Length == 0)
            {
                if (File.Exists(sidecar))
                {
                    File.Delete(sidecar);
                }
            }
            else
            {
                File.WriteAllText(sidecar, trimmed);
            }

            // The cached prepared copy was made from the old (transcript, audio) pair.
            var prepared = CloneReferenceTail.GetPreparedFileName(referenceFileName);
            foreach (var stale in new[] { prepared, prepared + ".stamp" })
            {
                if (File.Exists(stale))
                {
                    File.Delete(stale);
                }
            }

            Se.WriteToolsLog($"TTS voice transcript saved: '{sidecar}'");
            return true;
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Writing voice transcript '{sidecar}' failed");
            error = ex.Message;
            return false;
        }
    }
}
