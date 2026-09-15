using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Video.BurnIn;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// The style choices made in the "Second subtitle file" dialog, kept on MainViewModel so
/// reopening the dialog to adjust an already-loaded secondary subtitle starts from what was
/// last chosen instead of silently resetting to defaults every time (#14842).
/// </summary>
public class SecondarySubtitleStyle
{
    public Color Color { get; set; } = Colors.White;
    public int FontSize { get; set; }
    public bool FontBold { get; set; }
    public FontBoxType FontBoxType { get; set; } = FontBoxType.None;
    public string AlignmentCode { get; set; } = "8";
}
