using System;

namespace Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;

public interface IAdjustCallback
{
    void Adjust(TimeSpan adjustment, bool adjustAll, bool adjustSelectedLines, bool adjustSelectedLinesAndForward);
}