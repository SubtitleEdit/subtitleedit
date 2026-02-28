namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class FormatViewModel
{
    public string Name { get; set; }
    public bool IsFavorite { get; set; }

    public FormatViewModel()
    {
        Name = string.Empty;
    }
}
