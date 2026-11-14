using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Assa;

public class YCbCrMatrixItem
{
    public string Name { get; set; }
    public YCbCrMatrixType Style { get; set; }

    // Header values are literal ASS syntax, not UI concepts - kept untranslated like Aegisub does.
    // Same values and order as libass (parse_ycbcr_matrix) and Aegisub (valid_header_strings).
    private static readonly YCbCrMatrixItem[] _items = new YCbCrMatrixItem[]
    {
        new(Se.Language.General.NotAvailable, YCbCrMatrixType.NotSet),
        new("None", YCbCrMatrixType.None),
        new("TV.601", YCbCrMatrixType.Tv601),
        new("PC.601", YCbCrMatrixType.Pc601),
        new("TV.709", YCbCrMatrixType.Tv709),
        new("PC.709", YCbCrMatrixType.Pc709),
        new("TV.FCC", YCbCrMatrixType.TvFcc),
        new("PC.FCC", YCbCrMatrixType.PcFcc),
        new("TV.240M", YCbCrMatrixType.Tv240M),
        new("PC.240M", YCbCrMatrixType.Pc240M),
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
