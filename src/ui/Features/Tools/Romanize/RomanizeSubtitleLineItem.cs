using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Tools.Romanize;

public partial class RomanizeSubtitleLineItem
{
    public int? LineNumber { get; set; }
    public string? TextOriginal { get; set; }
    public string? TextRomanized { get; set; }
}
