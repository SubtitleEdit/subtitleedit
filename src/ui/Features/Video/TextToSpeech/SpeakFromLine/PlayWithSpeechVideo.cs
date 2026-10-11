using Nikse.SubtitleEdit.Controls.VideoPlayer;
using System;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.SpeakFromLine;

/// <summary>
/// The main window's video player for <see cref="PlayWithSpeechReader"/>. The player control is
/// looked up on every call, so going fullscreen or undocking keeps working.
/// </summary>
public sealed class PlayWithSpeechVideo : IPlayWithSpeechVideo
{
    private readonly Func<VideoPlayerControl?> _getControl;
    private readonly Action<VideoPlayerControl> _play;
    private readonly Action<VideoPlayerControl> _pause;
    private readonly Func<double> _getSpeed;
    private readonly Func<bool> _isVideoOpen;

    /// <param name="play">Starts playback the way the main window does (keeps the playhead in step).</param>
    /// <param name="pause">Pauses playback the way the main window does.</param>
    /// <param name="getSpeed">The playback speed picked in the waveform toolbar.</param>
    public PlayWithSpeechVideo(Func<VideoPlayerControl?> getControl, Action<VideoPlayerControl> play, Action<VideoPlayerControl> pause, Func<double> getSpeed, Func<bool> isVideoOpen)
    {
        _isVideoOpen = isVideoOpen;
        _getControl = getControl;
        _play = play;
        _pause = pause;
        _getSpeed = getSpeed;
    }

    public bool HasVideo => _getControl() != null && _isVideoOpen();
    public double Position => _getControl()?.VideoPlayer.Position ?? 0;
    public bool IsPlaying => _getControl()?.VideoPlayer.IsPlaying ?? false;
    public double Speed => _getSpeed();

    public double Volume
    {
        get => _getControl()?.Volume ?? 0;
        set
        {
            var control = _getControl();
            if (control != null)
            {
                control.Volume = value;
            }
        }
    }

    public void Play()
    {
        var control = _getControl();
        if (control != null)
        {
            _play(control);
        }
    }

    public void Pause()
    {
        var control = _getControl();
        if (control != null)
        {
            _pause(control);
        }
    }
}
