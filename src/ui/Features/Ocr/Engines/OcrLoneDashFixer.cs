using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Ocr.Engines;

/// <summary>
/// Google Lens segments the dialog dash of a subtitle line as its own text line (the dash is
/// far enough from the first word to be detected separately), so a two-line dialog comes back
/// as e.g. "-", "YOU CAN'T DO THIS.", "-", "DO WHAT?" - sometimes with both dashes grouped
/// before the text lines or a dash trailing last. Re-attach each lone dash to the text line
/// it belongs to. (#12988)
/// </summary>
public static class OcrLoneDashFixer
{
    public static string FixLoneDashes(string text)
    {
        if (!text.Contains('-'))
        {
            return text;
        }

        var lines = FixLoneDashes(text.SplitToLines());
        return string.Join(Environment.NewLine, lines);
    }

    public static List<string> FixLoneDashes(List<string> lines)
    {
        var dashCount = lines.Count(IsLoneDash);
        if (dashCount == 0)
        {
            return lines;
        }

        var textLines = lines.Where(p => !IsLoneDash(p)).ToList();
        if (textLines.Count == 0)
        {
            return lines;
        }

        // When every text line missing a dialog dash can be paired with exactly one lone dash,
        // the dashes are dialog dashes that were split off - prefix them back, keeping the
        // text lines in their original order. This also handles dashes that arrive grouped
        // before the text lines or after them.
        var linesMissingDash = textLines.Count(p => !p.TrimStart().StartsWith('-'));
        if (dashCount == linesMissingDash)
        {
            return textLines
                .Select(p => p.TrimStart().StartsWith('-') ? p : "- " + p.TrimStart())
                .ToList();
        }

        // Fallback: join each lone dash with the following text line.
        var result = new List<string>(lines.Count);
        for (var i = 0; i < lines.Count; i++)
        {
            if (IsLoneDash(lines[i]) && i + 1 < lines.Count && !IsLoneDash(lines[i + 1]))
            {
                result.Add("- " + lines[i + 1].TrimStart());
                i++;
            }
            else
            {
                result.Add(lines[i]);
            }
        }

        return result;
    }

    private static bool IsLoneDash(string line)
    {
        return line.Trim() == "-";
    }
}
