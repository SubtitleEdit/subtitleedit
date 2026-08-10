namespace Nikse.SubtitleEdit.Features.Options.DoNotBreakAfterList;

public class NoBreakAfterListItem
{
    public string Text { get; }
    public bool IsRegex { get; }

    public NoBreakAfterListItem(string text, bool isRegex)
    {
        Text = text;
        IsRegex = isRegex;
    }

    public override string ToString()
    {
        return Text;
    }
}
