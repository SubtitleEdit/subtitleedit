using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Globalization;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Helpers for the ASSA style alignment tags "{\an1}".."{\an9}" used in subtitle text.
/// </summary>
public static class AlignmentTagHelper
{
    /// <summary>
    /// True if the text contains the given alignment, either as "{\anX}" or inline as "\anX".
    /// </summary>
    public static bool HasAlignment(string text, string alignment)
    {
        return text.Contains("\\" + alignment, StringComparison.Ordinal);
    }

    public static string RemoveAlignmentTags(string text)
    {
        return text
            .Replace("{\\an1}", string.Empty)
            .Replace("{\\an2}", string.Empty)
            .Replace("{\\an3}", string.Empty)
            .Replace("{\\an4}", string.Empty)
            .Replace("{\\an5}", string.Empty)
            .Replace("{\\an6}", string.Empty)
            .Replace("{\\an7}", string.Empty)
            .Replace("{\\an8}", string.Empty)
            .Replace("{\\an9}", string.Empty)
            .Replace("\\an1", string.Empty)
            .Replace("\\an2", string.Empty)
            .Replace("\\an3", string.Empty)
            .Replace("\\an4", string.Empty)
            .Replace("\\an5", string.Empty)
            .Replace("\\an6", string.Empty)
            .Replace("\\an7", string.Empty)
            .Replace("\\an8", string.Empty)
            .Replace("\\an9", string.Empty);
    }

    /// <summary>
    /// Removes any existing alignment tags and applies the given alignment.
    /// "an2" (default bottom-center) is only written when writeAn2Tag is true.
    /// </summary>
    public static string SetAlignment(string text, string alignment, bool writeAn2Tag)
    {
        var result = RemoveAlignmentTags(text);

        if (string.IsNullOrEmpty(result))
        {
            return result;
        }

        if (alignment == "an2" && !writeAn2Tag)
        {
            return result;
        }

        if (result.StartsWith("{\\", StringComparison.Ordinal))
        {
            return result.Insert(2, alignment + "\\");
        }

        return $"{{\\{alignment}}}{result}";
    }

    /// <summary>
    /// Moves the alignment tag to the top/middle/bottom band a teletext row (1..23) falls in,
    /// keeping its left/centre/right column. The bands are the ones the EBU STL reader uses
    /// for the vertical position, so a saved file reads back with the same tag. Without this a
    /// line moved to row 1 kept the reader's bottom tag, and everything that goes by the tag -
    /// open subtitling, a subtitle not read from an STL file - left it at the bottom.
    /// </summary>
    public static string SetTeletextRowBand(string text, int teletextRow, bool writeAn2Tag)
    {
        var column = 2;
        if (HasAlignment(text, "an1") || HasAlignment(text, "an4") || HasAlignment(text, "an7"))
        {
            column = 1;
        }
        else if (HasAlignment(text, "an3") || HasAlignment(text, "an6") || HasAlignment(text, "an9"))
        {
            column = 3;
        }

        var band = Ebu.GetVerticalAlignmentBand(teletextRow, TeletextRows);
        return SetAlignment(text, "an" + (band * 3 + column).ToString(CultureInfo.InvariantCulture), writeAn2Tag);
    }

    private const int TeletextRows = 23;
}
