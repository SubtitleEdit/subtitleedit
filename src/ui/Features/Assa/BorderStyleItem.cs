using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

public class BorderStyleItem
{
    public string Name { get; set; }
    public BorderStyleType Style { get; set; }

    private static BorderStyleItem[] _borderStyleItems = new BorderStyleItem[]
    {
        new(Se.Language.General.Outline, BorderStyleType.Outline),
        new(Se.Language.General.BoxPerLine, BorderStyleType.BoxPerLine),
        new(Se.Language.General.Box, BorderStyleType.OneBox),
    };

    public BorderStyleItem(string name, BorderStyleType style)
    {
        Name = name;
        Style = style;
    }

    public override string ToString()
    {
        return Name;
    }

    public static BorderStyleItem[] List()
    {
        return _borderStyleItems;
    }
}
