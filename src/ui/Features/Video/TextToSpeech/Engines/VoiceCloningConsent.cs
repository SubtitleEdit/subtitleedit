using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;

/// <summary>
/// The one-time consent a user gives before SE will clone a voice from a reference recording.
///
/// Cloning is the one thing SE's TTS does that can land the user in legal trouble on its own: a
/// voice is personal data under the GDPR and a personality right in most of Europe, so cloning
/// somebody's voice without their permission is not the user's call to make silently. On top of
/// that the EU AI Act (Regulation (EU) 2024/1689) Article 50(4), applicable from 2 August 2026,
/// puts a disclosure duty on whoever publishes audio that imitates a real person — and SE
/// deliberately turns off the engine's own provenance marking (see
/// <see cref="CrispAsrTtsProvenance"/>: no spoken disclaimer, no AudioSeal watermark, no C2PA
/// manifest) so the rendered audio can be muxed into a video untouched. That opt-out is what makes
/// the disclosure the user's job, so the user has to be told about it once, in words, before the
/// first clone.
///
/// The answer is stamped with <see cref="ConsentVersion"/> rather than a bool, so wording that
/// materially changes what is being agreed to can ask again by bumping the version — the same
/// scheme <see cref="IndexTts25AudioCpp.LicenseVersion"/> uses for the model licence.
/// </summary>
public static class VoiceCloningConsent
{
    /// <summary>
    /// Bump when the terms in the dialog change in substance; everyone is then asked again.
    /// </summary>
    public const string ConsentVersion = "voice-clone-consent-2026-08";

    /// <summary>
    /// True when this user has accepted the current terms and voice cloning is still switched on.
    /// Reading <see cref="SeVideoTextToSpeech.AcceptVoiceCloning"/> as well means turning the
    /// setting off (the only revoke there is) brings the dialog back on the next clone instead of
    /// leaving a stamped-but-disabled state that can never be recovered from the UI.
    /// </summary>
    public static bool IsAccepted =>
        Se.Settings.Video.TextToSpeech.AcceptVoiceCloning &&
        string.Equals(
            Se.Settings.Video.TextToSpeech.VoiceCloningConsent,
            ConsentVersion,
            StringComparison.Ordinal);

    /// <summary>
    /// Records the answer. Persisting it is the caller's job — see
    /// <c>VoiceCloneConsentViewModel</c>, which saves straight away so an answer given once is not
    /// lost if the session ends badly.
    /// </summary>
    public static void Accept()
    {
        Se.Settings.Video.TextToSpeech.AcceptVoiceCloning = true;
        Se.Settings.Video.TextToSpeech.VoiceCloningConsent = ConsentVersion;
    }

    /// <summary>
    /// Declining means "not now": the version stays unstamped, so the clone is refused and the next
    /// attempt asks again.
    /// </summary>
    /// <remarks>
    /// Deliberately does not switch <see cref="SeVideoTextToSpeech.AcceptVoiceCloning"/> off, even
    /// though that would make the engines refuse to clone by themselves. That setting also governs
    /// the spoken AI disclaimer CrispASR prepends to <em>all</em> its output, cloned or not — so
    /// clearing it here would mean someone who closed this window to read it later found every
    /// later generation, on voices that clone nothing, announcing itself out loud. The gates in
    /// front of import and synthesis are the enforcement; this only records that no answer was
    /// given.
    /// </remarks>
    public static void Decline()
    {
        Se.Settings.Video.TextToSpeech.VoiceCloningConsent = string.Empty;
    }

    /// <summary>
    /// Whether handing a recording to <paramref name="engine"/> means cloning a voice.
    /// </summary>
    /// <remarks>
    /// Reads <see cref="ITtsEngine.SupportsVoiceCloning"/>, which is false for Piper — its import
    /// takes a trained .onnx model, and no speaker is being imitated there, so gating it would
    /// only train users to click through the dialog.
    /// </remarks>
    public static bool RequiresConsent(ITtsEngine? engine) => engine?.SupportsVoiceCloning == true;

    /// <summary>
    /// Whether <paramref name="voice"/> speaks by imitating a reference recording rather than a
    /// baked-in speaker.
    /// </summary>
    /// <remarks>
    /// The convention across every cloning engine is a non-empty <c>FilePath</c> on its own voice
    /// type — a preset, a designed voice or a built-in speaker leaves it empty. The types share no
    /// interface, hence the list; a cloning engine added later has to be added here too, and until
    /// it is, its clones simply are not gated at synthesis (the import gate still covers them).
    ///
    /// This is what catches reference WAVs that never went through the import dialog — dropped into
    /// the voices folder by hand from the settings dialogs' "open voices folder", or seeded from a
    /// downloaded voice pack — plus everyone who imported their clones before this gate existed.
    /// Callers pass the globally selected voice; a clone used only as a per-actor cast row, with a
    /// non-clone voice selected globally, is still asked about at import but not at synthesis.
    /// </remarks>
    public static bool IsCloneVoice(Voice? voice) => voice?.EngineVoice switch
    {
        // Clones every speaker in the video, one per line - the most cloning any single pick does.
        PerLineCloneVoice => true,
        ChatterboxVoice v => !string.IsNullOrEmpty(v.FilePath),
        CosyVoice3Voice v => !string.IsNullOrEmpty(v.FilePath),
        DotsTtsVoice v => !string.IsNullOrEmpty(v.FilePath),
        F5TtsVoice v => !string.IsNullOrEmpty(v.FilePath),
        IndexTtsVoice v => !string.IsNullOrEmpty(v.FilePath),
        MossTtsVoice v => !string.IsNullOrEmpty(v.FilePath),
        OmniVoice v => !string.IsNullOrEmpty(v.FilePath),
        OmniVoiceCrispAsrVoice v => !string.IsNullOrEmpty(v.FilePath),
        Qwen3TtsVoice v => !string.IsNullOrEmpty(v.FilePath),
        VibeVoice v => !string.IsNullOrEmpty(v.FilePath),
        VoxCPM2Voice v => !string.IsNullOrEmpty(v.FilePath),
        ZonosTtsVoice v => !string.IsNullOrEmpty(v.FilePath),
        _ => false,
    };
}
