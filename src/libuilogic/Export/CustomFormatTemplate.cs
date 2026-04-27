namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// Plain POCO version of Subtitle Edit's CustomFormatItem (no MVVM observability).
/// Drives <see cref="CustomTextFormatter"/> from headless callers like seconv.
/// </summary>
public sealed class CustomFormatTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string FormatHeader { get; set; } = string.Empty;
    public string FormatParagraph { get; set; } = string.Empty;
    public string FormatFooter { get; set; } = string.Empty;
    public string FormatTimeCode { get; set; } = string.Empty;
    public string? FormatNewLine { get; set; }
}
