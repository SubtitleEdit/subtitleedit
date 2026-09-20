using System;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.Ffmpeg.Audio;

/// <summary>
/// Where decoded PCM goes. Interleaved signed 16-bit samples, the sample rate and channel count
/// given to <see cref="Open"/>. The sink is also the player's master clock while audio plays:
/// <see cref="PlayedSeconds"/> is how much of the audio written since the last <see cref="Reset"/>
/// has actually come out of the speaker, so video frames are shown against real audio time
/// rather than against a CPU timer that drifts from the sound card.
/// </summary>
public interface IAudioSink : IDisposable
{
    void Open(int sampleRate, int channels);

    /// <summary>
    /// Queue PCM for playback. Blocks while the device queue is full - that back pressure is
    /// what paces the audio decoder. Returns false when the sink was reset or closed while
    /// waiting, so the caller can drop the data instead of pushing stale audio after a seek.
    /// </summary>
    bool Write(ReadOnlySpan<byte> pcm);

    /// <summary>Seconds of audio played since the last <see cref="Reset"/>.</summary>
    double PlayedSeconds { get; }

    /// <summary>Drop everything queued and restart the played counter at zero (seek, stop).</summary>
    void Reset();

    void Pause();
    void Resume();
}
