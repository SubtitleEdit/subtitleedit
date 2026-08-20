using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Shared.PickTeletextColor;

/// <summary>
/// One of the eight teletext colors: the English name is what EBU STL import/export
/// uses in font tags, the display name is localized for the UI.
/// </summary>
public class TeletextColorItem
{
    public string EnglishName { get; }
    public string DisplayName { get; }
    public Color Color { get; }

    public TeletextColorItem(string englishName, string displayName, Color color)
    {
        EnglishName = englishName;
        DisplayName = displayName;
        Color = color;
    }
}
