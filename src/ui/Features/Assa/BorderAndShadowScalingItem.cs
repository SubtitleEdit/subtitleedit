using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

public class BorderAndShadowScalingItem
{
    public string Name { get; set; }
    public BorderAndShadowScalingType Style { get; set; }

    private static BorderAndShadowScalingItem[] _wrapStyleItems = new BorderAndShadowScalingItem[]
    {
        new(Se.Language.General.Yes, BorderAndShadowScalingType.Yes),
        new(Se.Language.General.No, BorderAndShadowScalingType.No),
        new(Se.Language.General.NotAvailable, BorderAndShadowScalingType.NotSet),
    };

    public BorderAndShadowScalingItem(string name, BorderAndShadowScalingType style)
    {
        Name = name;
        Style = style;
    }

    public override string ToString()
    {
        return Name;
    }

    public static BorderAndShadowScalingItem[] List()
    {
        return _wrapStyleItems;
    }
}
