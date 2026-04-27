using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Main.MainHelpers;

public class PlaySelectionItem
{
    public double EndSeconds { get; set; } 
    public int Index { get; set; } 
    public bool Loop { get; set; }
    public List<SubtitleLineViewModel> Subtitles { get; set; }

    public PlaySelectionItem(List<SubtitleLineViewModel> subtitles, TimeSpan endTime, bool loop)
    {
        Subtitles = subtitles;
        EndSeconds = endTime.TotalSeconds;
        Index = 0;
        Loop = loop;
    }

    public SubtitleLineViewModel? GetNextSubtitle(double playerPositionInSeconds)
    {
        // find the first subtitle after the current position
        var nextIndex = Subtitles.FindIndex(s => s.EndTime.TotalSeconds >= playerPositionInSeconds);
        if (nextIndex >= 0)
        {
            Index = nextIndex;
            var s = Subtitles[Index];
            EndSeconds = s.EndTime.TotalSeconds;
            return s;
        }
        
        if (Loop)
        {
            Index = 0;
            var s = Subtitles[Index];
            EndSeconds = s.EndTime.TotalSeconds;
            return s;
        }

        return null;
    }

    public bool HasGapOrIsFirst()
    {
        if (Index < 1)
        {
            return true;
        }

        var previousEnd = Subtitles[Index-1].EndTime.TotalMilliseconds;
        var currentStart = Subtitles[Index].StartTime.TotalMilliseconds;
        
        return currentStart - previousEnd > 100;
    }
}
