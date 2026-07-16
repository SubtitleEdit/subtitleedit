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
    /// Reads the active rules from an XML / JSON (.template) / CSV multiple-replace file into the
    /// flat rule list the apply loop uses. Format is chosen by extension, falling back to sniffing
    /// the first non-whitespace character ('&lt;' = XML, '{' = JSON, else CSV).
    /// </summary>
    public static List<Rule> LoadRules(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        string content;
        try
        {
            content = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Failed to read multiple-replace file at {path}: {ex.Message}", ex);
        }

        var firstChar = content.AsSpan().TrimStart().Length > 0 ? content.AsSpan().TrimStart()[0] : '\0';
        var isXml = ext == ".xml" || (ext != ".csv" && ext != ".json" && ext != ".template" && firstChar == '<');
        var isJson = ext == ".json" || ext == ".template" || (ext != ".xml" && ext != ".csv" && firstChar == '{');

        if (isXml)
        {
            return LoadXmlRules(content, path);
        }

        if (isJson)
        {
            return LoadCategoryItemRules(ParseJson(content, path));
        }

        return LoadCategoryItemRules(CsvRules.Parse(content));
    }

    private static List<Rule> LoadXmlRules(string content, string path)
    {
        GroupsRoot? root;
        try
        {
            using var reader = new StringReader(content);
            var serializer = new XmlSerializer(typeof(GroupsRoot));
            root = (GroupsRoot?)serializer.Deserialize(reader);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Failed to parse multiple-replace XML at {path}: {ex.Message}", ex);
        }

        var rules = new List<Rule>();
        if (root is null)
        {
            return rules;
        }

        foreach (var group in root.Groups.Where(g => g.IsActive))
        {
            foreach (var rule in group.Rules.Where(r => r.Active))
            {
                if (!string.IsNullOrEmpty(rule.FindWhat))
                {
                    rules.Add(rule);
                }
            }
        }

        return rules;
    }

    private static CategoryImportExport ParseJson(string content, string path)
    {
        try
        {
            var item = System.Text.Json.JsonSerializer.Deserialize<CategoryImportExport>(content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return item ?? new CategoryImportExport();
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Failed to parse multiple-replace JSON at {path}: {ex.Message}", ex);
        }
    }

    // Flattens the SE5 category/rule shape (shared by the .template JSON and the CSV) into active
    // rules, mapping the UI's MultipleReplaceType names to the XML SearchType names the apply loop
    // expects: CaseInsensitive -> Normal, others by name.
    private static List<Rule> LoadCategoryItemRules(CategoryImportExport item)
    {
        var rules = new List<Rule>();
        if (item.Categories == null)
        {
            return rules;
        }

        foreach (var category in item.Categories)
        {
            if (category.Rules == null)
            {
                continue;
            }

            foreach (var rule in category.Rules)
            {
                if (!rule.IsActive || string.IsNullOrEmpty(rule.Find))
                {
                    continue;
                }

                rules.Add(new Rule
                {
                    Active = true,
                    FindWhat = rule.Find,
                    ReplaceWith = rule.ReplaceWith ?? string.Empty,
                    Description = rule.Description,
                    SearchType = MapSearchType(rule.Type),
                });
            }
        }

        return rules;
    }

    private static string MapSearchType(string? type)
    {
        var t = (type ?? string.Empty).Trim();
        if (t.Equals("CaseSensitive", StringComparison.OrdinalIgnoreCase))
        {
            return SearchTypeCaseSensitive;
        }

        if (t.Equals("RegularExpression", StringComparison.OrdinalIgnoreCase))
        {
            return SearchTypeRegularExpression;
        }

        // "CaseInsensitive" (SE5 default), "Normal", or anything unknown -> case-insensitive literal.
        return SearchTypeNormal;
    }

    // CSV reader for the SE5 "Multiple replace > export" CSV
    // (columns: Category,Find,ReplaceWith,Description,Active,Type). Ported from the UI's
    // CsvImporter so a CSV exported from the GUI produces the same rules here.
    private static class CsvRules
    {
        public static CategoryImportExport Parse(string csv)
        {
            var result = new CategoryImportExport { Categories = new List<CategoryImportExport.Category>() };
            var rows = ParseCsv(csv);
            if (rows.Count == 0)
            {
                return result;
            }

            var header = rows[0];
            var hasHeader = header.Any(h => string.Equals(h.Trim(), "Find", StringComparison.OrdinalIgnoreCase));

            int Col(string name, int fallback)
            {
                for (var i = 0; i < header.Count; i++)
                {
                    if (string.Equals(header[i].Trim(), name, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                return fallback;
            }

            var catIdx = hasHeader ? Col("Category", 0) : 0;
            var findIdx = hasHeader ? Col("Find", 1) : 1;
            var replaceIdx = hasHeader ? Col("ReplaceWith", 2) : 2;
            var descIdx = hasHeader ? Col("Description", 3) : 3;
            var activeIdx = hasHeader ? Col("Active", 4) : 4;
            var typeIdx = hasHeader ? Col("Type", 5) : 5;

            var byCategory = new Dictionary<string, CategoryImportExport.Category>(StringComparer.Ordinal);
            var order = new List<string>();

            for (var r = hasHeader ? 1 : 0; r < rows.Count; r++)
            {
                var rowValues = rows[r];
                if (rowValues.Count == 0 || rowValues.All(string.IsNullOrEmpty))
                {
                    continue;
                }

                string Get(int i) => i >= 0 && i < rowValues.Count ? rowValues[i] : string.Empty;

                var find = Get(findIdx);
                if (string.IsNullOrEmpty(find))
                {
                    continue;
                }

                var categoryName = Get(catIdx).Trim();
                if (string.IsNullOrEmpty(categoryName))
                {
                    categoryName = "Default";
                }

                if (!byCategory.TryGetValue(categoryName, out var category))
                {
                    category = new CategoryImportExport.Category { Name = categoryName, Rules = new List<CategoryImportExport.RuleItem>() };
                    byCategory[categoryName] = category;
                    order.Add(categoryName);
                }

                category.Rules!.Add(new CategoryImportExport.RuleItem
                {
                    Find = find,
                    ReplaceWith = Get(replaceIdx),
                    Description = Get(descIdx),
                    IsActive = ParseBool(Get(activeIdx)),
                    Type = Get(typeIdx).Trim(),
                });
            }

            result.Categories = order.Select(n => byCategory[n]).ToList();
            return result;
        }

        private static bool ParseBool(string value)
        {
            value = value.Trim();
            return value.Equals("true", StringComparison.OrdinalIgnoreCase)
                   || value.Equals("1", StringComparison.Ordinal)
                   || value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        // RFC 4180: quoted fields with embedded commas, doubled quotes ("") and newlines.
        private static List<List<string>> ParseCsv(string text)
        {
            if (text.Length > 0 && text[0] == '﻿')
            {
                text = text.Substring(1);
            }

            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new System.Text.StringBuilder();
            var inQuotes = false;

            void EndField() => row.Add(field.ToString());
            void EndRow()
            {
                EndField();
                rows.Add(row);
                row = new List<string>();
            }

            var i = 0;
            while (i < text.Length)
            {
                var c = text[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            field.Append('"');
                            i += 2;
                            continue;
                        }

                        inQuotes = false;
                        i++;
                        continue;
                    }

                    field.Append(c);
                    i++;
                    continue;
                }

                switch (c)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        EndField();
                        field.Clear();
                        break;
                    case '\r':
                        break;
                    case '\n':
                        EndRow();
                        field.Clear();
                        break;
                    default:
                        field.Append(c);
                        break;
                }

                i++;
            }

            if (field.Length > 0 || row.Count > 0)
            {
                EndRow();
            }

            return rows;
        }
    }

    // Mirrors the SE UI's CategoryImportExportItem JSON shape (camelCase-serialized), read case-
    // insensitively so both .template JSON and CSV flow through one flattener.
    private sealed class CategoryImportExport
    {
        public List<Category>? Categories { get; set; }

        public sealed class Category
        {
            public string Name { get; set; } = string.Empty;
            public List<RuleItem>? Rules { get; set; }
        }

        public sealed class RuleItem
        {
            public string Find { get; set; } = string.Empty;
            public string ReplaceWith { get; set; } = string.Empty;
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public string Type { get; set; } = string.Empty;
        }
    }

    /// <summary>
    /// Loads + applies all active rules from <paramref name="path"/> to <paramref name="subtitle"/>.
    /// The file may be the legacy SE4 <c>MultipleSearchAndReplaceGroups</c> XML, or the SE5
    /// GUI's exported <c>.template</c> JSON or <c>.csv</c> (Tools &gt; Multiple replace &gt;
    /// export). Format is chosen by extension, then by content sniffing. Returns the number of
    /// paragraphs whose text was modified.
    /// </summary>
    public static int Apply(Subtitle subtitle, string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Multiple-replace file not found: {path}", path);
        }

        var rules = LoadRules(path);
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
