using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Main;

// Picks up edits made to the open subtitle file by another program (#15828). No live file
// watcher: the file's time stamp and size are compared when the main window is activated again,
// which is when the user comes back from the other editor. Unchanged work is reloaded right
// away, unsaved work is only replaced after asking.
public partial class MainViewModel
{
    private SubtitleFileStamp? _subtitleFileStamp;
    private bool _checkingSubtitleFileChangedOutside;

    private sealed record SubtitleFileStamp(string FileName, DateTime LastWriteTimeUtc, long Length)
    {
        public static SubtitleFileStamp? Read(string fileName)
        {
            try
            {
                var fileInfo = new FileInfo(fileName);
                return fileInfo.Exists ? new SubtitleFileStamp(fileName, fileInfo.LastWriteTimeUtc, fileInfo.Length) : null;
            }
            catch
            {
                return null;
            }
        }

        public bool IsSameContentAs(SubtitleFileStamp other) =>
            LastWriteTimeUtc == other.LastWriteTimeUtc && Length == other.Length;
    }

    /// <summary>
    /// The file currently being edited, when it is a real file on disk that SE writes back to -
    /// not a converted import, whose name is only a "Save as" suggestion and may well point at an
    /// unrelated file next to the source.
    /// </summary>
    private string? GetWatchedSubtitleFileName()
    {
        if (_converted || string.IsNullOrEmpty(_subtitleFileName))
        {
            return null;
        }

        return _subtitleFileName;
    }

    /// <summary>
    /// Takes the time stamp of the file as it is now - call after opening or saving it, so SE's
    /// own writes never count as changes made outside.
    /// </summary>
    private void RememberSubtitleFileStamp()
    {
        var fileName = GetWatchedSubtitleFileName();
        _subtitleFileStamp = fileName == null ? null : SubtitleFileStamp.Read(fileName);
    }

    /// <summary>
    /// Run when the main window loses activation: the many import paths that end up with a file
    /// name (and the ones still to come) are covered here instead of each taking a stamp itself.
    /// </summary>
    private void EnsureSubtitleFileStamp()
    {
        var fileName = GetWatchedSubtitleFileName();
        if (fileName == null)
        {
            _subtitleFileStamp = null;
            return;
        }

        if (_subtitleFileStamp == null || !IsSameFile(_subtitleFileStamp.FileName, fileName))
        {
            _subtitleFileStamp = SubtitleFileStamp.Read(fileName);
        }
    }

    private async Task CheckSubtitleFileChangedOutside()
    {
        if (_checkingSubtitleFileChangedOutside || _opening || _loading || Window is not { IsActive: true } || WindowService.IsModalDialogOpen)
        {
            return;
        }

        var fileName = GetWatchedSubtitleFileName();
        if (fileName == null || _subtitleFileStamp is not { } stamp || !IsSameFile(stamp.FileName, fileName))
        {
            return;
        }

        // A missing file has nothing to reload, and an empty one is most likely another editor
        // half way through writing it - keep the old stamp so the next activation looks again.
        var current = SubtitleFileStamp.Read(fileName);
        if (current == null || current.Length == 0 || current.IsSameContentAs(stamp))
        {
            return;
        }

        // Ask once per change: answering "No" keeps the edits and does not ask again until the
        // file is changed outside once more.
        _subtitleFileStamp = current;

        _checkingSubtitleFileChangedOutside = true;
        try
        {
            if (HasChanges())
            {
                var answer = await MessageBox.Show(Window, Se.Language.General.Warning,
                    string.Format(Se.Language.General.SubtitleFileChangedOutsideX, fileName),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            await ReloadSubtitleChangedOutside(fileName);
        }
        finally
        {
            _checkingSubtitleFileChangedOutside = false;
            _shortcutManager.ClearKeys();
        }
    }

    /// <summary>
    /// Reopens the file the way File > Reopen does, keeping the video, the selected line, the
    /// encoding, the original next to it, the video offset and SMPTE timing.
    /// </summary>
    private async Task ReloadSubtitleChangedOutside(string fileName)
    {
        var selectedLine = SelectedSubtitleIndex ?? 0;
        var state = new RecentFile
        {
            SubtitleFileName = fileName,
            SubtitleFileNameOriginal = ShowColumnOriginalText ? _subtitleFileNameOriginal ?? string.Empty : string.Empty,
            VideoFileName = _videoFileName ?? string.Empty,
            SelectedLine = selectedLine,
            Encoding = SelectedEncoding.DisplayName,
            VideoOffsetInMs = Se.Settings.General.CurrentVideoOffsetInMs,
            VideoIsSmpte = IsSmpteTimingEnabled,
            AudioTrack = _audioTrack?.Id ?? -1,
        };

        // A successful open takes a new stamp; none means the new content could not be read and
        // the open left the current work alone (it already told the user why).
        _subtitleFileStamp = null;
        await SubtitleOpen(fileName, selectedSubtitleIndex: selectedLine, textEncoding: SelectedEncoding, skipLoadVideo: true);
        if (_subtitleFileStamp == null || !IsSameFile(_subtitleFileName, fileName))
        {
            return;
        }

        await RestoreRememberedOriginal(selectedLine, state.SubtitleFileNameOriginal, fileName);
        SetRecentFileProperties(state);
        ShowStatus(string.Format(Se.Language.General.SubtitleReloadedChangedOutsideX, fileName));
    }
}
