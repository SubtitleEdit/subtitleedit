namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// A user rule that marks a line as an error when its text matches a regular expression,
/// e.g. review markers like "[CHECK]" left by a translation pipeline (#10136).
/// </summary>
public class SeCustomErrorRule
{
    public bool Enabled { get; set; }
    public string Name { get; set; }
    public string Pattern { get; set; }
    public bool MatchCase { get; set; }
    public string Color { get; set; }

    public static string DefaultColor => Avalonia.Media.Color.FromArgb(90, 255, 165, 0).FromColorToHex();

    public SeCustomErrorRule()
    {
        Enabled = true;
        Name = string.Empty;
        Pattern = string.Empty;
        Color = DefaultColor;
    }
}
