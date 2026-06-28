using Nikse.SubtitleEdit.Core.Common;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace SeConv.Core;

/// <summary>
/// Loads a Subtitle Edit Multiple-Search-and-Replace XML file and applies the rules
/// to a Subtitle. The XML is the same shape SE's UI saves: a list of groups, each
/// containing a list of rules with FindWhat/ReplaceWith/SearchType.
/// </summary>
// XmlSerializer requires the type tree to be public.
public static class MultipleReplaceLoader
{
    public const string SearchTypeNormal = "Normal";
    public const string SearchTypeCaseSensitive = "CaseSensitive";
    public const string SearchTypeRegularExpression = "RegularExpression";

    [XmlRoot("MultipleSearchAndReplaceGroups")]
    public sealed class GroupsRoot
    {
        [XmlElement("Group")]
        public List<Group> Groups { get; set; } = [];
    }

    public sealed class Group
    {
        [XmlElement("Name")]
        public string Name { get; set; } = string.Empty;
        [XmlElement("IsActive")]
        public bool IsActive { get; set; } = true;
        [XmlArray("Rules")]
        [XmlArrayItem("Rule")]
        public List<Rule> Rules { get; set; } = [];
    }

    public sealed class Rule
    {
        [XmlElement("Active")]
        public bool Active { get; set; } = true;
        [XmlElement("FindWhat")]
        public string FindWhat { get; set; } = string.Empty;
        [XmlElement("ReplaceWith")]
        public string ReplaceWith { get; set; } = string.Empty;
        [XmlElement("SearchType")]
        public string SearchType { get; set; } = SearchTypeNormal;
        [XmlElement("Description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Loads + applies all active rules from <paramref name="xmlPath"/> to <paramref name="subtitle"/>.
    /// Returns the number of paragraphs whose text was modified.
    /// </summary>
    public static int Apply(Subtitle subtitle, string xmlPath)
    {
        if (!File.Exists(xmlPath))
        {
            throw new FileNotFoundException($"Multiple-replace XML not found: {xmlPath}", xmlPath);
        }

        GroupsRoot? root;
        try
        {
            using var fs = File.OpenRead(xmlPath);
            var serializer = new XmlSerializer(typeof(GroupsRoot));
            root = (GroupsRoot?)serializer.Deserialize(fs);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Failed to parse multiple-replace XML at {xmlPath}: {ex.Message}", ex);
        }

        if (root is null || root.Groups.Count == 0)
        {
            return 0;
        }

        var rules = new List<Rule>();
        foreach (var group in root.Groups.Where(g => g.IsActive))
        {
            foreach (var rule in group.Rules.Where(r => r.Active))
            {
                if (string.IsNullOrEmpty(rule.FindWhat))
                {
                    continue;
                }
                rules.Add(rule);
            }
        }

        if (rules.Count == 0)
        {
            return 0;
        }

        // Pre-compile rule patterns once (not per-paragraph):
        //  - RegularExpression rules use the user pattern with RegexOptions.Multiline so ^/$
        //    anchors match at every line boundary, matching the UI (MultipleReplaceViewModel
        //    compiles with Compiled | Multiline).
        //  - Normal (case-insensitive literal) rules compile an escaped, IgnoreCase pattern
        //    so the per-paragraph loop no longer re-escapes and re-parses on every line.
        // A rule with an invalid/empty pattern is left out here so it's skipped entirely.
        var compiledRegex = new Dictionary<Rule, Regex>();
        foreach (var rule in rules)
        {
            try
            {
                if (string.Equals(rule.SearchType, SearchTypeRegularExpression, StringComparison.OrdinalIgnoreCase))
                {
                    compiledRegex[rule] = new Regex(rule.FindWhat, RegexOptions.Compiled | RegexOptions.Multiline);
                }
                else if (!string.Equals(rule.SearchType, SearchTypeCaseSensitive, StringComparison.OrdinalIgnoreCase)
                         && !string.IsNullOrEmpty(rule.FindWhat))
                {
                    compiledRegex[rule] = new Regex(Regex.Escape(rule.FindWhat), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
            }
            catch
            {
                // Bad regex — leave it out so it's skipped during replacement.
            }
        }

        var modified = 0;
        foreach (var paragraph in subtitle.Paragraphs)
        {
            var newText = paragraph.Text ?? string.Empty;
            foreach (var rule in rules)
            {
                if (string.Equals(rule.SearchType, SearchTypeCaseSensitive, StringComparison.OrdinalIgnoreCase))
                {
                    newText = newText.Replace(rule.FindWhat, rule.ReplaceWith);
                }
                else if (string.Equals(rule.SearchType, SearchTypeRegularExpression, StringComparison.OrdinalIgnoreCase))
                {
                    if (compiledRegex.TryGetValue(rule, out var regex))
                    {
                        newText = RegexUtils.ReplaceNewLineSafe(regex, newText, rule.ReplaceWith);
                    }
                }
                else // Normal — case-insensitive literal replace (empty FindWhat = no-op, not compiled)
                {
                    if (compiledRegex.TryGetValue(rule, out var regex))
                    {
                        // Escape '$' so the replacement is treated literally (no $1/$& expansion).
                        newText = regex.Replace(newText, rule.ReplaceWith.Replace("$", "$$"));
                    }
                }
            }
            if (newText != paragraph.Text)
            {
                paragraph.Text = newText;
                modified++;
            }
        }

        return modified;
    }
}
