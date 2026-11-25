using Nikse.SubtitleEdit.Features.Main;
using System;

namespace Nikse.SubtitleEdit.Features.Sync.PointSync;

/// <summary>
/// A point sync view model that knows which lines are part of a sync point, so the "#" column
/// can mark them (issue #17053).
/// </summary>
public interface ISyncedLineMarker
{
    bool IsSynced(SubtitleLineViewModel line);

    /// <summary>Raised when the set of synced lines changes, so visible cells re-mark themselves.</summary>
    event Action? SyncedLinesChanged;
}
