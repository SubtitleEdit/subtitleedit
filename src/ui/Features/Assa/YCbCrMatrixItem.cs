using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

public class YCbCrMatrixItem
{
    public string Name { get; set; }
    public YCbCrMatrixType Style { get; set; }

    // Header values are literal ASS syntax, not UI concepts - kept untranslated like Aegisub does.
    // SMPTE240M and FCC are parsed by libass too, but are museum values not offered here.
    private static YCbCrMatrixItem[] _items = new YCbCrMatrixItem[]
    {
        new(Se.Language.General.NotAvailable, YCbCrMatrixType.NotSet),
        new("None", YCbCrMatrixType.None),
        new("PC.601", YCbCrMatrixType.Pc601),
        new("PC.709", YCbCrMatrixType.Pc709),
        new("TV.601", YCbCrMatrixType.Tv601),
        new("TV.709", YCbCrMatrixType.Tv709),
    };

    public YCbCrMatrixItem(string name, YCbCrMatrixType style)
    {
        Name = name;
        Style = style;
    }

    public override string ToString()
    {
        return Name;
    }

    public static YCbCrMatrixItem[] List()
    {
        return _items;
    }
}
