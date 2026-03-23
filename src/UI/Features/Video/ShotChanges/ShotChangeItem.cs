using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.ShotChanges;

public class ShotChangeItem
{
    public int Index { get; set; }
    public double Seconds { get; set; }
    public string TimeText { get; set; }

    public ShotChangeItem(int index, double seconds)
    {
        Index = index;
        Seconds = seconds;
        TimeText = TimeCode.FromSeconds(seconds).ToDisplayString();
    }

    public override string ToString() => TimeText;
}
