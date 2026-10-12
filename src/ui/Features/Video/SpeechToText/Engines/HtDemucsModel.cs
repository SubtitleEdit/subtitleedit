using Nikse.SubtitleEdit.UiLogic.AudioToText;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;

/// <summary>
/// The fast source separation model for text-to-speech "Remove original speech" (#15928). CrispASR's
/// "--separate" runs it like <see cref="SpeechIsolationModel"/> (the architecture is read from the
/// GGUF), about three times faster but with a little more of the music lost.
/// </summary>
/// <remarks>
/// f16 only: the q8_0 file aborts CrispASR on Metal ("unsupported op 'CONT'") up to v0.8.42 (fixed in v0.8.43), see
/// CrispStrobe/CrispASR#537.
/// </remarks>
public static class HtDemucsModel
{
    public const string FileName = "htdemucs-f16.gguf";
    public const string Url = "https://huggingface.co/cstr/htdemucs-GGUF/resolve/main/" + FileName;
    public const string Size = "81 MB";
    public const string DisplayName = "HTDemucs";

    /// <summary>
    /// HTDemucs splits into drums, bass, other and vocals - everything but the speech is the first
    /// three, which have to be mixed back together.
    /// </summary>
    public static readonly string[] BackgroundStems = { "drums", "bass", "other" };

    /// <summary>The model's trained window: 7.8 s at 44.1 kHz.</summary>
    public const int ChunkSamples = 343980;

    /// <summary>Windows advance by 75% of their length (25% overlap).</summary>
    public const int StrideSamples = 257985;

    public static WhisperModel ToWhisperModel() => new()
    {
        Name = FileName,
        Size = Size,
        Urls = new[] { Url },
    };
}
