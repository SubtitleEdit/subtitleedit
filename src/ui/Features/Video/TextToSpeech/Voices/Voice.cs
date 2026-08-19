namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

public class Voice
{
    public object? EngineVoice { get; set; }
    public string Name => ToString();

    /// <summary>
    /// Optional combo-box label for when the bare voice name doesn't say enough on its own - e.g.
    /// CosyVoice3, whose list concatenates baked engine presets and imported clones with nothing
    /// marking which is which (#13272). Falls back to <see cref="Name"/>.
    /// </summary>
    /// <remarks>
    /// Deliberately separate from <see cref="Name"/>: Name is the value that gets persisted (the
    /// saved voice pick, and every cast-row mapping) and matched back by string on load, and a
    /// cast row whose saved name no longer matches falls silently through to the engine's first
    /// voice. Decorating Name itself would therefore re-point saved casts at the wrong speaker.
    /// </remarks>
    public string DisplayName => string.IsNullOrEmpty(_displayName) ? Name : _displayName;

    private readonly string _displayName;

    public Voice(object voice) : this(voice, string.Empty)
    {
    }

    public Voice(object voice, string displayName)
    {
        EngineVoice = voice;
        _displayName = displayName;
    }

    public override string ToString()
    {
        return (EngineVoice == null ? "Unknown" : EngineVoice.ToString()) ?? "Unknown";
    }
}
