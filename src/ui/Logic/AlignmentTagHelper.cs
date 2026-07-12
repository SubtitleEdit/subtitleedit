using System;

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
}
