namespace Nikse.SubtitleEdit.Features.Options.WordLists;

public class OcrFixItem
{
    public string Find { get; set; }
    public string ReplaceWith { get; set; }

    public OcrFixItem(string find, string replaceWith)
    {
        Find = find;
        ReplaceWith = replaceWith;
    }

    public override string ToString() => $"{Find} -> {ReplaceWith}";
}
