namespace Nikse.SubtitleEdit.Features.Ocr.NOcr;

public class NOcrDrawModeItem
{
    public string Name { get; set; }
    public NOcrDrawModeItemType Type { get; set; }


    public NOcrDrawModeItem(string name, NOcrDrawModeItemType type  )
    {
        Name = name;
        Type = type;
    }

    public override string ToString()
    {
        return Name;
    }

    public static NOcrDrawModeItem ForegroundItem = new("Foreground", NOcrDrawModeItemType.Foreground);
    public static NOcrDrawModeItem BackgroundItem = new("Background", NOcrDrawModeItemType.Background);

    public static NOcrDrawModeItem[] Items = new NOcrDrawModeItem[]
    {
        ForegroundItem,
        BackgroundItem
    };
}
