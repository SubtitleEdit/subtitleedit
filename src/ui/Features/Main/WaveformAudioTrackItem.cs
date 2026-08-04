using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace Nikse.SubtitleEdit.Features.Main;

/// <summary>
/// One entry in the waveform toolbar's audio-track picker: the underlying mpv track plus a
/// pre-built display label (codec, channel layout, language and a rough extraction-time estimate).
/// </summary>
public class WaveformAudioTrackItem
{
    public AudioTrackInfo Track { get; init; } = null!;

    public string Display { get; init; } = string.Empty;

    public override string ToString() => Display;
}
