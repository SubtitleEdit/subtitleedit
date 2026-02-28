using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.SplitBreakLongLines;

public class SplitBreakLongLinesItem
{
    public string Name { get; set; }
    public int Number { get; set; }
    public string Fix { get; set; }
    public SubtitleLineViewModel SubtitleLine { get; set; }

    public SplitBreakLongLinesItem(string name, int number, string fix, SubtitleLineViewModel subtitleLine)
    {
        Name = name;
        Number = number;
        Fix = fix;
        SubtitleLine = subtitleLine;
    }
}
