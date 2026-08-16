namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

/// <summary>
/// The "voice" a user picks when every line should be spoken in the voice heard in the video at
/// that line. It carries no speaker of its own - it is a marker that says "resolve me per
/// paragraph", and the generation loop replaces it with a real voice built from that line's own
/// audio before anything is synthesised.
/// </summary>
/// <remarks>
/// A distinct type rather than an empty <c>FilePath</c> on some engine's voice, so that an
/// unresolved marker reaching an engine fails loudly instead of quietly synthesising with the
/// engine's built-in speaker - which would look like "cloning did nothing".
///
/// <see cref="Name"/> is what gets persisted (the saved voice pick, cast rows), so
/// <see cref="ToString"/> is a fixed identifier and never localized; the translated label lives in
/// <c>Voice.DisplayName</c>.
/// </remarks>
public class PerLineCloneVoice
{
    /// <summary>The persisted identifier - see the remarks on why this is not translated.</summary>
    public const string Id = "Clone from video";

    public override string ToString() => Id;
}
