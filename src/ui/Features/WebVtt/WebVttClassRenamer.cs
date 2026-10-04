using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Features.WebVtt;

/// <summary>
/// Rewrites cue class names in WebVTT cue text after styles were renamed in the WebVTT styles
/// dialog. Classes are attached to a start tag with dots - <c>&lt;c.name&gt;</c>,
/// <c>&lt;c.name.other&gt;</c>, <c>&lt;i.name&gt;</c>, <c>&lt;v.name Bob&gt;</c> - so only
/// the dot-separated class list of a start tag is touched, never the annotation or plain text.
/// All renames are applied in one pass, so swapping two names (a to b, b to a) works.
/// </summary>
public static class WebVttClassRenamer
{
    private static readonly Regex StartTagClasses = new Regex(
        @"<(c|i|b|u|v|lang|ruby|rt)((?:\.[^\s.>]+)+)",
        RegexOptions.Compiled);

    /// <summary>
    /// Returns <paramref name="text"/> with every class found in <paramref name="renames"/>
    /// (old name to new name, case-sensitive like CSS classes) replaced.
    /// </summary>
    public static string RenameClasses(string text, IReadOnlyDictionary<string, string> renames)
    {
        if (string.IsNullOrEmpty(text) || renames.Count == 0 || !text.Contains('<'))
        {
            return text;
        }

        return StartTagClasses.Replace(text, match =>
        {
            var classes = match.Groups[2].Value.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var changed = false;
            for (var i = 0; i < classes.Length; i++)
            {
                if (renames.TryGetValue(classes[i], out var newName))
                {
                    classes[i] = newName;
                    changed = true;
                }
            }

            if (!changed)
            {
                return match.Value;
            }

            var sb = new StringBuilder();
            sb.Append('<');
            sb.Append(match.Groups[1].Value);
            foreach (var c in classes)
            {
                sb.Append('.');
                sb.Append(c);
            }

            return sb.ToString();
        });
    }
}
