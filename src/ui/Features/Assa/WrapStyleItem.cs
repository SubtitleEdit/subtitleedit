using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

public class WrapStyleItem
{
    public string Name { get; set; }
    public WrapStyleType Style { get; set; }

    private static WrapStyleItem[] _wrapStyleItems = new WrapStyleItem[]
    {
        new(Se.Language.Assa.SmartWrappingTopWide, WrapStyleType.SmartWrappingTopWide),
        new(Se.Language.Assa.EndOfLineWrapping, WrapStyleType.EndOfLineWrapping),
        new(Se.Language.Assa.NoWrapping, WrapStyleType.NoWrapping),
        new(Se.Language.Assa.SmartWrappingBottomWide, WrapStyleType.SmartWrappingBottomWide),
    };

    public WrapStyleItem(string name, WrapStyleType style)
    {
        Name = name;
        Style = style;
    }

    public override string ToString()
    {
        return Name;
    }

    public static WrapStyleItem[] List()
    {
        return _wrapStyleItems;
    }
}
