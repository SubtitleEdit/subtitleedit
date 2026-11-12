using Avalonia.Media;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// A bookmark uses one of three colors (<see cref="Features.Main.SubtitleLineViewModel.BookmarkColor"/>),
/// set in Settings &gt; Appearance as Bookmark color, Bookmark color 2 and Bookmark color 3.
/// </summary>
public static class BookmarkColors
{
    public const int Count = 3;

    public static string GetHex(int index)
    {
        var appearance = Se.Settings.Appearance;
        return index switch
        {
            1 => appearance.BookmarkColor2,
            2 => appearance.BookmarkColor3,
            _ => appearance.BookmarkColor,
        };
    }

    public static Color Get(int index) => GetHex(index).FromHexToColor();
}
