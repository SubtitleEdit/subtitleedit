using System;
using System.Collections.Generic;
using System.Linq;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

/// <summary>
/// The name-casing engine behind "Fix names": which names from the dictionary occur in the
/// subtitle with the wrong casing, and what each line looks like once they are fixed.
/// Kept free of UI state so batch convert runs exactly what the dialog previews - batch has
/// no place to show the name list, but it must not silently do nothing either.
/// </summary>
internal static class FixNamesLogic
{
    private const string PrefixChars = "([ --'>\r\n¿¡\"”“„";
    private const string SuffixChars = " ,.!?:;…')]<-\"\r\n";

    /// <summary>
    /// Names the dialog leaves unchecked by default: real words often capitalized mid-sentence,
    /// where "fixing" the casing is usually wrong. Batch skips them for the same reason.
    /// </summary>
    private static readonly string[] CommonWords = ["US", "Lane", "Bill", "Rose"];

    /// <summary>
    /// Finds dictionary names that occur in the subtitle with different casing. IsChecked mirrors
    /// how the dialog pre-selects each hit, so an unattended run applies the same subset.
    /// </summary>
    internal static List<(string Name, bool IsChecked)> FindNames(Subtitle subtitle, IEnumerable<string> names, string extraNames, string language)
    {
        var text = HtmlUtil.RemoveHtmlTags(subtitle.GetAllTexts());

        var allNames = names.ToList();
        foreach (var s in (extraNames ?? string.Empty).Split(','))
        {
            var extra = s.Trim();
            if (extra.Length > 1 && !allNames.Contains(extra))
            {
                allNames.Add(extra);
            }
        }

        const string english = "en";
        const string dont = "don't";

        var usedNames = new HashSet<string>();
        var result = new List<(string Name, bool IsChecked)>();

        foreach (var name in allNames)
        {
            // filter out invalid names
            if (name.Length <= 1 || name == name.ToLowerInvariant())
            {
                continue;
            }

            var startIndex = text.IndexOf(name, StringComparison.OrdinalIgnoreCase);
            while (startIndex >= 0)
            {
                if (IsWordBoundary(text, startIndex, name) && !text.AsSpan().Slice(startIndex, name.Length).Equals(name, StringComparison.Ordinal)) // do not add names where casing already is correct
                {
                    if (!usedNames.Contains(name))
                    {
                        var skip = false;
                        var isChecked = true;
                        if (language.StartsWith(english, StringComparison.OrdinalIgnoreCase))
                        {
                            skip = text.AsSpan()[startIndex..].StartsWith(dont, StringComparison.OrdinalIgnoreCase);
                            isChecked = !CommonWords.Contains(name);
                        }

                        if (!skip)
                        {
                            usedNames.Add(name);
                            result.Add((name, isChecked));
                            break; // break while
                        }
                    }
                }

                startIndex = text.IndexOf(name, startIndex + name.Length, StringComparison.OrdinalIgnoreCase);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the line with every given name re-cased, or the unchanged line if nothing applies.
    /// </summary>
    internal static string ApplyNames(string paragraphText, IReadOnlyList<string> activeNames)
    {
        var text = paragraphText;

        // reusable array
        var processingNames = new string[1];

        foreach (var name in activeNames)
        {
            // no extra processing if paragraph doesn't contain name
            if (!text.Contains(name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);

            // has letter and not already uppercase
            if (textNoTags != textNoTags.ToUpperInvariant())
            {
                var st = new StrippableText(text);
                processingNames[0] = name;
                st.FixCasing(processingNames, true, false, false, string.Empty);
                text = st.MergedString;
            }
        }

        return text;
    }

    private static bool IsWordBoundary(string text, int startIndex, string name)
    {
        var afterNameIndex = startIndex + name.Length;
        return (startIndex == 0 || PrefixChars.Contains(text[startIndex - 1]))
               && (afterNameIndex == text.Length || SuffixChars.Contains(text[afterNameIndex]));
    }
}
