using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Video.ShotChanges;

public class TimeItem
{
    public double Seconds { get; set; }
    public string TimeText { get; set; }
    public int Number { get; set; }

    public TimeItem(double seconds, int number)
    {
        Seconds = seconds;
        TimeText = TimeCode.FromSeconds(seconds).ToDisplayString();
        Number = number;
    }

    public override string ToString() => TimeText;
}
