namespace Nikse.SubtitleEdit.Features.Tools.Romanize;

public partial class RomanizeSubtitleLineItem
{
    public bool Merged { get; set; }
    public int? LineNumber { get; set; }
    public string? TextOutput { get; set; }
    public string? TextOriginal { get; set; }
    public string? TextRomanized { get; set; }
    public RomanizedLinePositions RomanizedLinePosition { get; set; }
}
