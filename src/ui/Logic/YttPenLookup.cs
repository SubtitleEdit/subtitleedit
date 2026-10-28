using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using System;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// The pens of the YouTube timed text subtitle being edited, for the grid and the text box -
/// they only see a line's text, and <c>&lt;s p="3"&gt;</c> means nothing without the header.
/// Null when the current format is not YouTube timed text.
/// </summary>
public static class YttPenLookup
{
    private static volatile YttHead? _head;

    public static void SetHeader(string? header)
    {
        _head = YttHead.IsYttHeader(header) ? YttHead.Parse(header) : new YttHead();
    }

    public static void Clear()
    {
        _head = null;
    }

    public static bool IsActive => _head != null;

    /// <summary>The pen of an s tag's attributes (the text between "s" and "&gt;").</summary>
    public static YttPen? GetPen(ReadOnlySpan<char> attributes)
    {
        var head = _head;
        if (head == null)
        {
            return null;
        }

        foreach (var attribute in YttText.ParseAttributes(attributes.ToString()))
        {
            if (attribute.Key == "p")
            {
                return head.GetPen(attribute.Value);
            }
        }

        return null;
    }

    public static YttPen? GetPen(int id) => _head?.GetPen(id);

    /// <summary>The pen's text color, null for the default white.</summary>
    public static Color? GetForeColor(YttPen? pen)
    {
        if (pen?.ForeColor == null)
        {
            return null;
        }

        var color = SubtitleSyntaxTokenizer.TryParseColor(pen.ForeColor);
        if (color == null || (color.Value.R >= 0xFE && color.Value.G >= 0xFE && color.Value.B >= 0xFE))
        {
            return null;
        }

        return color;
    }
}
