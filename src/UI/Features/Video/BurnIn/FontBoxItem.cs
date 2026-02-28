namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public class FontBoxItem
{
    public FontBoxType BoxType { get; set; }
    public string Name { get; set; }

    public FontBoxItem(FontBoxType boxType, string name)
    {
        BoxType = boxType;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}
