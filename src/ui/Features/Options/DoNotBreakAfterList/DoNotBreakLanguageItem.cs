namespace Nikse.SubtitleEdit.Features.Options.DoNotBreakAfterList;

public class DoNotBreakLanguageItem
{
    public string DisplayName { get; }
    public string TwoLetterCode { get; }
    public string FileName { get; }

    public DoNotBreakLanguageItem(string displayName, string twoLetterCode, string fileName)
    {
        DisplayName = displayName;
        TwoLetterCode = twoLetterCode;
        FileName = fileName;
    }

    public override string ToString()
    {
        return DisplayName;
    }
}
