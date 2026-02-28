using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;

public class ApplyDurationLimitItem
{
    public bool Apply { get; set; }
    public string Name { get; set; }
    public int Number { get; set; }
    public string Fix { get; set; }
    public SubtitleLineViewModel SubtitleLine { get; set; }

    public ApplyDurationLimitItem(bool apply, string name, int number, string fix, SubtitleLineViewModel subtitleLine)
    {
        Apply = apply;
        Name = name;
        Number = number;
        Fix = fix;
        SubtitleLine = subtitleLine;
    }
}
