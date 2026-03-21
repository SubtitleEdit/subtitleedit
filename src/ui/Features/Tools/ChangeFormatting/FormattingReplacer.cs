using Avalonia.Media;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeFormatting;

/// <summary>
/// Replace formatting in subtitle text while preserving tag placements.
/// Example: "How <i>are you</i>?" to "How <b>are you</b>?"
/// </summary>
public static class FormattingReplacer
{
    public static string Replace(string text, ChangeFormattingType from, ChangeFormattingType to, Color color, SubtitleFormat format)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var isAssa = format is AdvancedSubStationAlpha;

        // Replace tags in place, preserving their positions
        return ReplaceFormattingTags(text, from, to, color, isAssa);
    }

    private static string ReplaceFormattingTags(string text, ChangeFormattingType from, ChangeFormattingType to, Color color, bool isAssa)
    {
        if (from == to)
        {
            return text;
        }

        var result = text;

        if (isAssa)
        {
            result = ReplaceAssaTags(result, from, to, color);
        }
        else
        {
            result = ReplaceHtmlTags(result, from, to, color);
        }

        return result;
    }

    private static string ReplaceHtmlTags(string text, ChangeFormattingType from, ChangeFormattingType to, Color color)
    {
        var result = text;

        // Get the tag names for from and to
        var fromOpenTag = GetHtmlOpenTag(from);
        var fromCloseTag = GetHtmlCloseTag(from);
        var toOpenTag = GetHtmlOpenTag(to, color);
        var toCloseTag = GetHtmlCloseTag(to);

        if (from == ChangeFormattingType.Color)
        {
            // Replace font color tags
            result = Regex.Replace(result, @"<font\s+color=[""'][^""']*[""']\s*>", toOpenTag, RegexOptions.IgnoreCase);
            result = result.Replace("</font>", toCloseTag, StringComparison.OrdinalIgnoreCase);
            result = result.Replace("</FONT>", toCloseTag, StringComparison.OrdinalIgnoreCase);
        }
        else if (to == ChangeFormattingType.Color)
        {
            // Replace tags with color font tags
            result = result.Replace(fromOpenTag, toOpenTag, StringComparison.OrdinalIgnoreCase);
            result = result.Replace(fromOpenTag.ToUpperInvariant(), toOpenTag, StringComparison.OrdinalIgnoreCase);
            result = result.Replace(fromCloseTag, toCloseTag, StringComparison.OrdinalIgnoreCase);
            result = result.Replace(fromCloseTag.ToUpperInvariant(), toCloseTag, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // Replace simple tags (i, b, u)
            result = result.Replace(fromOpenTag, toOpenTag, StringComparison.OrdinalIgnoreCase);
            result = result.Replace(fromOpenTag.ToUpperInvariant(), toOpenTag, StringComparison.OrdinalIgnoreCase);
            result = result.Replace(fromCloseTag, toCloseTag, StringComparison.OrdinalIgnoreCase);
            result = result.Replace(fromCloseTag.ToUpperInvariant(), toCloseTag, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private static string ReplaceAssaTags(string text, ChangeFormattingType from, ChangeFormattingType to, Color color)
    {
        var result = text;

        if (from == ChangeFormattingType.Color && to == ChangeFormattingType.Color)
        {
            // Replace color values only
            return result;
        }

        if (from == ChangeFormattingType.Color)
        {
            // Replace color tags with formatting tags
            result = Regex.Replace(result, @"\{\\(1c|c)&H[0-9A-Fa-f]+&\}", GetAssaTag(to, true), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\\(1c|c)&H[0-9A-Fa-f]+&", GetAssaTag(to, false), RegexOptions.IgnoreCase);
        }
        else if (to == ChangeFormattingType.Color)
        {
            // Replace formatting tags with color
            var skColor = new SkiaSharp.SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
            var colorTag = "{\\" + AdvancedSubStationAlpha.GetSsaColorStringForEvent(skColor) + "&}";

            var fromTag = GetAssaTagLetter(from);
            var pattern1 = $@"{{\{{\\{fromTag}1}}}}".Replace("{", @"\{").Replace("}", @"\}");
            var pattern2 = $@"{{\{{\\{fromTag}0}}}}".Replace("{", @"\{").Replace("}", @"\}");
            var pattern3 = $@"\\{fromTag}1";
            var pattern4 = $@"\\{fromTag}0";

            result = Regex.Replace(result, pattern1, Regex.Escape(colorTag), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, pattern2, @"{\\r}", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, pattern3, Regex.Escape(colorTag.TrimStart('{').TrimEnd('}')), RegexOptions.IgnoreCase);
            result = Regex.Replace(result, pattern4, @"\\r", RegexOptions.IgnoreCase);
        }
        else
        {
            // Replace formatting tags with other formatting tags
            var fromTag = GetAssaTagLetter(from);
            var toTag = GetAssaTagLetter(to);

            result = result.Replace($"{{\\{fromTag}1}}", $"{{\\{toTag}1}}", StringComparison.OrdinalIgnoreCase);
            result = result.Replace($"{{\\{fromTag}0}}", $"{{\\{toTag}0}}", StringComparison.OrdinalIgnoreCase);
            result = result.Replace($"\\{fromTag}1", $"\\{toTag}1", StringComparison.OrdinalIgnoreCase);
            result = result.Replace($"\\{fromTag}0", $"\\{toTag}0", StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private static string GetHtmlOpenTag(ChangeFormattingType type, Color? color = null)
    {
        return type switch
        {
            ChangeFormattingType.Italic => "<i>",
            ChangeFormattingType.Bold => "<b>",
            ChangeFormattingType.Underline => "<u>",
            ChangeFormattingType.Color => color.HasValue ? $"<font color=\"{ToHex(color.Value)}\">" : "<font color=\"#FFFFFF\">",
            _ => string.Empty
        };
    }

    private static string GetHtmlCloseTag(ChangeFormattingType type)
    {
        return type switch
        {
            ChangeFormattingType.Italic => "</i>",
            ChangeFormattingType.Bold => "</b>",
            ChangeFormattingType.Underline => "</u>",
            ChangeFormattingType.Color => "</font>",
            _ => string.Empty
        };
    }

    private static string GetAssaTagLetter(ChangeFormattingType type)
    {
        return type switch
        {
            ChangeFormattingType.Italic => "i",
            ChangeFormattingType.Bold => "b",
            ChangeFormattingType.Underline => "u",
            _ => string.Empty
        };
    }

    private static string GetAssaTag(ChangeFormattingType type, bool wrapped)
    {
        var tag = type switch
        {
            ChangeFormattingType.Italic => "\\i1",
            ChangeFormattingType.Bold => "\\b1",
            ChangeFormattingType.Underline => "\\u1",
            _ => string.Empty
        };

        return wrapped ? $"{{{tag}}}" : tag;
    }

    private static string ToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
