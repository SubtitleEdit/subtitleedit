using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic.Media;

public class FileHelperSubtitleSavePickerResult
{
    public string FileName { get; set; } = string.Empty;
    public SubtitleFormat SubtitleFormat { get; set; } = new SubRip();
}