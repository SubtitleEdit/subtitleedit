using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.MergeShortLines;

public class MergeShortLinesItem
{
    public string Name { get; set; }
    public int Number { get; set; }
    public string Fix { get; set; }
    public SubtitleLineViewModel SubtitleLine { get; set; }

    public MergeShortLinesItem(string name, int number, string fix, SubtitleLineViewModel subtitleLine)
    {
        Name = name;
        Number = number;
        Fix = fix;
        SubtitleLine = subtitleLine;
    }
}
