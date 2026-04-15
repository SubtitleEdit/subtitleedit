using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformThemes;

public class WaveformThemeDisplay
{
    public string Name { get; set; } = string.Empty;
    public Color TextColor { get; set; }
    public Color WaveformColor { get; set; }
    public Color BackgroundColor { get; set; }
    public Color SelectedColor { get; set; }
    public Color CursorColor { get; set; }
    public Color ShotChangeColor { get; set; }
    public Color ParagraphBackgroundColor { get; set; }
    public Color ParagraphSelectedBackgroundColor { get; set; }
    public Color ParagraphLeftColor { get; set; }
    public Color ParagraphRightColor { get; set; }
    public Color FancyHighColor { get; set; }

    public override string ToString() => Name;
}
