namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public class FixNameHitItem
{
    public bool IsEnabled { get; set; }
    public string Name { get; set; }
    public int LineIndex { get; set; }
    public int LineIndexDisplay { get; set; }
    public string Before { get; set; }
    public string After { get; set; }

    public FixNameHitItem(string name, int lineIndex, string before, string after, bool isEnabled)
    {
        Name = name;
        LineIndex = lineIndex;
        LineIndexDisplay = lineIndex + 1;
        Before = before;
        After = after;
        IsEnabled = isEnabled;
    }

    public override string ToString()
    {
        return Name;
    }
}
