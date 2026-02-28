using System.Globalization;

namespace Nikse.SubtitleEdit.Features.Tools.FixCommonErrors;

public class LanguageDisplayItem
{
    public CultureInfo Code { get; }
    public string Name { get; }

    public LanguageDisplayItem(CultureInfo code, string name)
    {
        Code = code;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}
