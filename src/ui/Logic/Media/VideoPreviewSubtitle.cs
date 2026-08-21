using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;
using System;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Draws the subtitle on a dialog's own video player, the way the main window draws it on the main
/// preview. Dialogs that exist to line a subtitle up against the picture used to show the video
/// bare, which is exactly the comparison they are for (discussion #13767 - SE4 did show it).
///
/// Wraps the same "write a temp ASSA file and hand it to the player" reloaders the main window
/// uses, plus the retry they need: mpv rejects sub-add until the file is actually playing, and a
/// swallowed rejection leaves the player without subtitles for good (issue #13407).
///
/// One instance per player - the reloaders keep per-player state (temp file, pushed-text memo), so
/// two players sharing one instance would fight over the same temp file.
/// </summary>
public class VideoPreviewSubtitle : IVideoPreviewSubtitle
{
    private const int RetryDelayMs = 400;

    private readonly IMpvReloader _mpvReloader;
    private readonly IVlcReloader _vlcReloader;

    private bool _dirty = true;
    private bool _busy;
    private long _retryNotBeforeMs;

    public VideoPreviewSubtitle(IMpvReloader mpvReloader, IVlcReloader vlcReloader)
    {
        _mpvReloader = mpvReloader;
        _vlcReloader = vlcReloader;
    }

    /// <summary>
    /// Pushes the subtitle when something has changed since the last push. Meant to be called from
    /// a dialog's position timer: on the ticks where nothing changed it only reads a flag, so the
    /// subtitle is serialized once per change instead of once per tick.
    /// </summary>
    /// <param name="getSubtitle">
    /// Builds the subtitle to show. Only called when a push is actually due - the sync dialogs
    /// rebuild it from their line view models, which is not worth doing several times a second.
    /// </param>
    public void Refresh(IVideoPlayer? videoPlayer, Func<Subtitle> getSubtitle, VideoPreviewSubtitleContext context)
    {
        if (!_dirty || _busy)
        {
            return;
        }

        // No player, or a player with no file: there is nothing to draw a subtitle on. The dirty
        // flag stays set, so opening a video pushes right away.
        if (videoPlayer == null || string.IsNullOrEmpty(videoPlayer.FileName))
        {
            return;
        }

        if (Environment.TickCount64 < _retryNotBeforeMs)
        {
            return;
        }

        if (videoPlayer is LibMpvDynamicPlayer mpv)
        {
            var subtitle = getSubtitle();
            _dirty = false; // only after the snapshot was taken
            _mpvReloader.SmpteMode = context.SmpteMode;
            _ = RunRefresh(() => _mpvReloader.RefreshMpv(mpv, subtitle, null, context.Format));
        }
        else if (videoPlayer is LibVlcDynamicPlayer vlc)
        {
            var subtitle = getSubtitle();
            _dirty = false;
            _vlcReloader.SmpteMode = context.SmpteMode;
            _ = RunRefresh(async () =>
            {
                await _vlcReloader.RefreshVlc(vlc, subtitle, null, context.Format);
                return true;
            });
        }
    }

    /// <summary>
    /// Re-push on the next <see cref="Refresh"/> - after a sync moved the time codes, for example.
    /// </summary>
    public void Invalidate()
    {
        _dirty = true;
    }

    /// <summary>
    /// Forgets the subtitle handed to the player and deletes the temp file behind it. Use when the
    /// player was given another video (the old external subtitle is gone with the old file, so it
    /// has to be added again rather than reloaded) and when the dialog closes.
    /// </summary>
    public void Reset()
    {
        _mpvReloader.Reset();
        _vlcReloader.Reset();
        _dirty = true;
        _retryNotBeforeMs = 0;
    }

    private async Task RunRefresh(Func<Task<bool>> refresh)
    {
        _busy = true;
        try
        {
            if (!await refresh())
            {
                RetryLater();
            }
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Video preview subtitle refresh failed");
            RetryLater();
        }
        finally
        {
            _busy = false;
        }
    }

    private void RetryLater()
    {
        _dirty = true;
        _retryNotBeforeMs = Environment.TickCount64 + RetryDelayMs;
    }
}
