using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The per-engine half of <see cref="PerLineVoiceClone"/>: how a cut reference clip becomes a
/// voice this engine can speak with, how such a voice gives its recording back, and what a new
/// run has to clear. Implemented by every engine whose
/// <see cref="ITtsEngine.SupportsPerLineVoiceCloning"/> can be true - the capability flag says
/// the engine takes a reference per call, and this interface is where it says how. One without
/// the other is a wiring bug, which <see cref="PerLineVoiceClone.MakeVoiceForClip"/> asserts on
/// rather than quietly dubbing every line in a fallback voice.
/// </summary>
public interface IPerLineCloneEngine
{
    /// <summary>
    /// Wraps a cut reference clip as a voice this engine can speak with, staging the clip into
    /// the engine's own folders when the backend can only read it from there. Null when the clip
    /// cannot be used as a reference (e.g. no transcript beside it) - the caller then falls back
    /// to an ordinary voice for that line rather than failing the run.
    /// </summary>
    /// <param name="clipFileName">The reference clip, with its transcript sidecar beside it.</param>
    /// <param name="voiceName">
    /// What to call the voice, for the rows and lists that show it - already resolved by the
    /// caller, so never empty.
    /// </param>
    Voice? MakePerLineCloneVoice(string clipFileName, string voiceName);

    /// <summary>
    /// The recording <paramref name="voice"/> clones from, when the voice is this engine's own
    /// cloning voice type; null for another engine's voice, or for one that clones from nothing.
    /// </summary>
    /// <remarks>
    /// The counterpart of <see cref="MakePerLineCloneVoice"/>: whatever that hands out, this must
    /// be able to point back at, or export and regenerate lose the recording a line was
    /// generated from.
    /// </remarks>
    string? GetPerLineReferenceClip(Voice voice);

    /// <summary>
    /// Clears whatever a previous per-line run staged inside this engine's own folders; called as
    /// a new per-line run starts. Engines that speak straight from the clip's own path stage
    /// nothing and implement this as a no-op.
    /// </summary>
    void ResetStagedPerLineReferences();
}
