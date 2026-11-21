using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls.VideoPlayer;
using Nikse.SubtitleEdit.Features.Main.MainHelpers;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class MainViewModel
{
    private (IVideoPlayer Player, double StartSeconds, double EndSeconds, long SeekTimestamp, LibMpvDynamicPlayer? BoundedPlayer)? _subtitleEndPreview;

    [RelayCommand]
    private void PlaySubtitleEnd()
    {
        var vp = GetVideoPlayerControl();
        if (vp == null || SelectedSubtitle is not { } line || string.IsNullOrEmpty(_videoFileName))
        {
            return;
        }

        var clockFactor = vp.IsSmpteTimingEnabled ? 1001.0 / 1000.0 : 1.0;
        var range = GetSubtitleEndPreviewRange(line.StartTime.TotalSeconds, line.EndTime.TotalSeconds,
            vp.Duration / clockFactor, Se.Settings.Waveform.SubtitleEndPreviewMs);
        if (range is not { } clip)
        {
            return;
        }

        vp.VideoPlayer.Pause();
        ClearSubtitleEndPreview();
        var seekTimestamp = Stopwatch.GetTimestamp();
        SeekVideoPlayer(vp, clip.Start);
        PinPlayheadTo(clip.Start);

        // A native bound keeps audio inside the cue even when the UI thread is busy.
        var mpv = vp.VideoPlayer as LibMpvDynamicPlayer;
        var boundedPlayer = mpv?.SetPlaybackEnd(clip.End * clockFactor) == true ? mpv : null;
        _playSelectionItem = new PlaySelectionItem([line], TimeSpan.FromSeconds(clip.End), false);
        PlayVideo(vp);
        _subtitleEndPreview = (vp.VideoPlayer, clip.Start, clip.End, seekTimestamp, boundedPlayer);
    }

    internal static (double Start, double End)? GetSubtitleEndPreviewRange(
        double subtitleStart, double subtitleEnd, double mediaDuration, int durationMs)
    {
        if (subtitleEnd <= subtitleStart || !double.IsFinite(mediaDuration) || mediaDuration <= 0)
        {
            return null;
        }

        var seconds = SeWaveform.NormalizeSubtitleEndPreviewMs(durationMs) / 1000.0;
        var start = Math.Max(0, Math.Max(subtitleStart, subtitleEnd - seconds));
        var end = Math.Min(subtitleEnd, mediaDuration);
        return end > start ? (start, end) : null;
    }

    private void ClearSubtitleEndPreview()
    {
        if (_subtitleEndPreview is not { } preview)
        {
            return;
        }

        _subtitleEndPreview = null;
        _playSelectionItem = null;
        preview.BoundedPlayer?.SetPlaybackEnd(null);
    }

    private void CancelSubtitleEndPreview()
    {
        if (_subtitleEndPreview is not { } preview)
        {
            return;
        }

        var vp = GetVideoPlayerControl();
        if (vp != null && ReferenceEquals(vp.VideoPlayer, preview.Player))
        {
            PauseVideoAndFreezePlayhead(vp);
        }

        ClearSubtitleEndPreview();
    }

    internal void UpdateSubtitleEndPreview(VideoPlayerControl vp)
    {
        if (_subtitleEndPreview is not { } preview)
        {
            return;
        }

        var player = vp.VideoPlayer;
        if (!ReferenceEquals(player, preview.Player))
        {
            ClearSubtitleEndPreview();
            return;
        }

        // A backward seek can still report the old position, even past the preview's end.
        var seekLanded = player.SupportsPlaybackRestartEvents
            ? player.HasPlaybackRestartedSince(preview.SeekTimestamp)
            : Stopwatch.GetElapsedTime(preview.SeekTimestamp).TotalMilliseconds >= FrameStepPlaySeekWaitMs;
        if (!seekLanded)
        {
            return;
        }

        var isPlaying = player.IsPlaying;
        var clockFactor = vp.IsSmpteTimingEnabled ? 1001.0 / 1000.0 : 1.0;
        var reachedEnd = preview.BoundedPlayer is { } mpv
            ? !isPlaying && mpv.IsPlaybackEndReached
            : player.Position >= preview.EndSeconds * clockFactor;
        if (reachedEnd)
        {
            FinishPlaySelection(vp, preview.StartSeconds, preview.EndSeconds);
        }
        else if (!isPlaying && !_playStartGate.IsPending(Stopwatch.GetTimestamp(), Stopwatch.Frequency))
        {
            ClearSubtitleEndPreview();
        }
    }
}
