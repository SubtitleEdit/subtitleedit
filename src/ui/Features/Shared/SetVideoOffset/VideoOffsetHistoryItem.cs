using System;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;

/// <summary>
/// One previously used video offset, shown in the drop-down next to the offset field.
/// The display text is built once, in the same format the time code box next to it uses.
/// </summary>
public class VideoOffsetHistoryItem
{
    public long TotalMilliseconds { get; }
    public TimeSpan Offset { get; }
    public string DisplayText { get; }

    public VideoOffsetHistoryItem(long totalMilliseconds)
    {
        TotalMilliseconds = totalMilliseconds;
        Offset = TimeSpan.FromMilliseconds(totalMilliseconds);

        var timeCode = new TimeCode(totalMilliseconds);
        DisplayText = Se.Settings.General.UseFrameMode ? timeCode.ToHHMMSSFF() : timeCode.ToString();
    }

    public override string ToString() => DisplayText;
}
