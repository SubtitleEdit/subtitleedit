namespace Nikse.SubtitleEdit.Features.Options.Settings;

public class FileTypeAssociationViewModel
{
    public string Extension { get; set; } = string.Empty;
    public bool IsAssociated { get; set; } = false;
    public string IconPath { get; set; } = string.Empty; // Optional: use if you have images per extension
}
