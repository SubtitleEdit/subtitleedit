using System;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;

public class SyncPoint
{
    public int LeftIndex { get; set; }
    public int RightIndex { get; set; }
    public TimeSpan LeftStartTime { get; set; }
    public TimeSpan RightStartTime { get; set; }
    public string Text { get; set; }
    
    public override string ToString() => Text;

    public SyncPoint(
        SubtitleLineViewModel left,
        int leftIndex,
        SubtitleLineViewModel right,
        int rightIndex)
    {
        LeftIndex = leftIndex;
        RightIndex = rightIndex;
        LeftStartTime = left.StartTime;
        RightStartTime = right.StartTime;
        Text = new TimeCode(left.StartTime).ToShortDisplayString() + " -> "  +
               new TimeCode(right.StartTime).ToShortDisplayString();
    }
}