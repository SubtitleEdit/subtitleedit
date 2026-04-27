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
                    try
                    {
                        var regex = new Regex(rule.FindWhat);
                        newText = RegexUtils.ReplaceNewLineSafe(regex, newText, rule.ReplaceWith);
                    }
                    catch
                    {
                        // Bad regex — skip this rule for this paragraph
                    }
                }
                else // Normal — case-insensitive whole-string replace
                {
                    newText = ReplaceCaseInsensitive(newText, rule.FindWhat, rule.ReplaceWith);
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

    private static string ReplaceCaseInsensitive(string source, string find, string replaceWith)
    {
        if (string.IsNullOrEmpty(find))
        {
            return source;
        }
        return Regex.Replace(source, Regex.Escape(find), replaceWith.Replace("$", "$$"), RegexOptions.IgnoreCase);
    }
}
