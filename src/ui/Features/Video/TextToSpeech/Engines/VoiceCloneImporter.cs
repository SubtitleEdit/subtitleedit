namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// Imports a reference recording as a cloned voice, handing the spoken text to the engines that
/// can make use of it.
/// </summary>
/// <remarks>
/// Several engines take the reference text ("ref-text") alongside the WAV - CosyVoice3 fails
/// synthesis outright without it, and the rest clone noticeably better with it - but they expose
/// it as their own two-argument overload rather than through <see cref="ITtsEngine"/>, so
/// something has to know which is which. The voice-settings dialog asks the user for that text;
/// callers that already know what is said in the clip (the waveform menu clones a subtitle line,
/// so its text *is* the transcript) come here instead.
/// </remarks>
public static class VoiceCloneImporter
{
    /// <summary>
    /// Imports <paramref name="fileName"/> into <paramref name="engine"/>'s voices folder.
    /// </summary>
    /// <param name="transcript">
    /// What is spoken in the recording. Empty or unknown is fine - the engines that want it then
    /// get the same import the plain <see cref="ITtsEngine.ImportVoice"/> does, rather than an
    /// empty sidecar claiming the clip is silent.
    /// </param>
    public static bool Import(ITtsEngine engine, string fileName, string? transcript)
    {
        var text = (transcript ?? string.Empty).Trim();
        if (text.Length == 0)
        {
            return engine.ImportVoice(fileName);
        }

        return engine switch
        {
            CosyVoice3CrispAsr e => e.ImportVoice(fileName, text),
            F5TtsCrispAsr e => e.ImportVoice(fileName, text),
            FireRedTts3AudioCpp e => e.ImportVoice(fileName, text),
            FishTtsAudioCpp e => e.ImportVoice(fileName, text),
            HiggsTtsAudioCpp e => e.ImportVoice(fileName, text),
            MossTtsCrispAsr e => e.ImportVoice(fileName, text),
            OmniVoiceCrispAsr e => e.ImportVoice(fileName, text),
            OmniVoiceTtsCpp e => e.ImportVoice(fileName, text),
            Qwen3TtsCrispAsr e => e.ImportVoice(fileName, text),
            VoxCPM2CrispAsr e => e.ImportVoice(fileName, text),
            _ => engine.ImportVoice(fileName),
        };
    }
}
