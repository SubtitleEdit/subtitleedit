namespace Nikse.SubtitleEdit.Features.Options.Language;

public class LanguageItem
{
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public override string ToString()
    {
        return Name;
    }
}
