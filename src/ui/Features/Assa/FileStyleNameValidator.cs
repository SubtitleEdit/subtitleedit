using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa;

/// <summary>
/// Checks the file styles' names before the ASSA/SSA styles dialog writes them to the header.
/// The rename tracker leaves lines alone while a name is blank or clashes (a transient state
/// while typing), so accepting such a name on OK/Apply wrote a nameless or duplicate
/// "Style:" line, and the lines of the renamed style fell back to the first style.
/// </summary>
public static class FileStyleNameValidator
{
    /// <summary>
    /// Returns the first style with an empty name or a name used by an earlier style
    /// (case-insensitive), with the message to show - or null when all names are fine.
    /// </summary>
    public static (StyleDisplay Style, string Message)? FindInvalidName(IEnumerable<StyleDisplay> styles)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var style in styles)
        {
            var name = style.Name?.Trim() ?? string.Empty;
            if (name.Length == 0)
            {
                return (style, Se.Language.Assa.StyleNameCannotBeEmpty);
            }

            if (!seen.Add(name))
            {
                return (style, string.Format(Se.Language.Assa.StyleNameXIsUsedMoreThanOnce, name));
            }
        }

        return null;
    }
}
