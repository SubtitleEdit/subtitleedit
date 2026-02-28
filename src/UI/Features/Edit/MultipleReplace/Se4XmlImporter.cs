using Nikse.SubtitleEdit.Features.Options.Settings;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;
public static class Se4XmlImporter
{
    internal static CategoryImportExportItem? ImportFromXml(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var multipleSearchAndReplaceList = doc.Root?.Element("MultipleSearchAndReplaceList");

            if (multipleSearchAndReplaceList == null)
            {
                return null;
            }

            var categories = new List<CategoryImportExportItem.RuleImportExportCategory>();

            foreach (var groupElement in multipleSearchAndReplaceList.Elements("Group"))
            {
                var categoryName = groupElement.Element("Name")?.Value ?? string.Empty;
                var enabled = bool.TryParse(groupElement.Element("Enabled")?.Value, out var isEnabled) && isEnabled;

                var rules = new List<CategoryImportExportItem.RuleImportExportItem>();

                foreach (var itemElement in groupElement.Elements("MultipleSearchAndReplaceItem"))
                {
                    var itemEnabled = bool.TryParse(itemElement.Element("Enabled")?.Value, out var itemIsEnabled) && itemIsEnabled;
                    var findWhat = itemElement.Element("FindWhat")?.Value ?? string.Empty;
                    var replaceWith = itemElement.Element("ReplaceWith")?.Value ?? string.Empty;
                    var searchType = itemElement.Element("SearchType")?.Value ?? "Normal";
                    var description = itemElement.Element("Description")?.Value ?? string.Empty;

                    // Map SE4 search type to our type system
                    var ruleType = MapSearchTypeToRuleType(searchType);

                    var rule = new CategoryImportExportItem.RuleImportExportItem
                    {
                        Find = findWhat,
                        ReplaceWith = replaceWith,
                        Description = description,
                        IsActive = itemEnabled,
                        Type = ruleType
                    };

                    rules.Add(rule);
                }

                var category = new CategoryImportExportItem.RuleImportExportCategory
                {
                    Name = categoryName,
                    Rules = rules
                };

                categories.Add(category);
            }

            return new CategoryImportExportItem
            {
                Categories = categories
            };
        }
        catch
        {
            return null;
        }
    }

    private static string MapSearchTypeToRuleType(string searchType)
    {
        return searchType switch
        {
            "CaseSensitive" => "CaseSensitive",
            "RegularExpression" => "RegularExpression",
            "Normal" or _ => "CaseInsensitive"
        };
    }
}