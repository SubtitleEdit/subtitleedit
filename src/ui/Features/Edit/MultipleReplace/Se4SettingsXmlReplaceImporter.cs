using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

// Reads the Multiple Search and Replace groups straight out of SE 4's
// Settings.xml (the <MultipleSearchAndReplaceGroups> section). This is a
// different shape from Se4XmlImporter, which handles the stand-alone *export*
// file (<MultipleSearchAndReplaceList>/<MultipleSearchAndReplaceItem>).
//
// SE 4 has shipped two slightly different element namings over its life, so we
// match on local names loosely: a group is any child whose name contains
// "Group", a rule is "MultipleSearchAndReplaceSetting" or "Rule", and the
// enabled flag may be "Enabled" or "IsActive"/"Active".
public static class Se4SettingsXmlReplaceImporter
{
    public static List<SeEditMultipleReplace.MultipleReplaceCategory> ImportFromXml(string xml)
    {
        var result = new List<SeEditMultipleReplace.MultipleReplaceCategory>();
        if (string.IsNullOrWhiteSpace(xml))
        {
            return result;
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch
        {
            return result;
        }

        var groupsRoot = doc.Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "MultipleSearchAndReplaceGroups");
        if (groupsRoot == null)
        {
            return result;
        }

        foreach (var groupElement in groupsRoot.Elements().Where(e => e.Name.LocalName.Contains("Group")))
        {
            var categoryName = LocalValue(groupElement, "Name") ?? string.Empty;
            var groupActive = ParseBool(LocalValue(groupElement, "Enabled", "IsActive", "Active"));

            var rulesContainer = groupElement.Elements().FirstOrDefault(e => e.Name.LocalName == "Rules") ?? groupElement;

            var rules = new List<MultipleReplaceRule>();
            foreach (var ruleElement in rulesContainer.Elements()
                         .Where(e => e.Name.LocalName == "MultipleSearchAndReplaceSetting" || e.Name.LocalName == "Rule"))
            {
                var find = LocalValue(ruleElement, "FindWhat", "Find") ?? string.Empty;
                if (string.IsNullOrEmpty(find))
                {
                    continue;
                }

                rules.Add(new MultipleReplaceRule
                {
                    Find = find,
                    ReplaceWith = LocalValue(ruleElement, "ReplaceWith") ?? string.Empty,
                    Description = LocalValue(ruleElement, "Description") ?? string.Empty,
                    Active = ParseBool(LocalValue(ruleElement, "Enabled", "IsActive", "Active")),
                    Type = MapSearchType(LocalValue(ruleElement, "SearchType")),
                });
            }

            if (rules.Count == 0)
            {
                continue;
            }

            result.Add(new SeEditMultipleReplace.MultipleReplaceCategory
            {
                Name = string.IsNullOrEmpty(categoryName) ? "Subtitle Edit 4" : categoryName,
                IsActive = groupActive,
                Rules = rules,
            });
        }

        return result;
    }

    // Same data in the shape the Multiple Replace import dialog works with, so the user can
    // point the right-click Import directly at an SE 4 Settings.xml (issue #12225).
    internal static CategoryImportExportItem? ImportFromXmlAsImportExport(string xml)
    {
        var categories = ImportFromXml(xml);
        if (categories.Count == 0)
        {
            return null;
        }

        return new CategoryImportExportItem
        {
            Categories = categories.Select(c => new CategoryImportExportItem.RuleImportExportCategory
            {
                Name = c.Name,
                Rules = c.Rules.Select(r => new CategoryImportExportItem.RuleImportExportItem
                {
                    Find = r.Find,
                    ReplaceWith = r.ReplaceWith,
                    Description = r.Description,
                    IsActive = r.Active,
                    Type = r.Type.ToString(),
                }).ToList(),
            }).ToList(),
        };
    }

    private static string? LocalValue(XElement parent, params string[] names)
    {
        foreach (var name in names)
        {
            var child = parent.Elements().FirstOrDefault(e => e.Name.LocalName == name);
            if (child != null)
            {
                return child.Value;
            }
        }

        return null;
    }

    private static bool ParseBool(string? value)
    {
        return bool.TryParse(value, out var b) && b;
    }

    private static MultipleReplaceType MapSearchType(string? searchType)
    {
        return searchType switch
        {
            "CaseSensitive" => MultipleReplaceType.CaseSensitive,
            "RegularExpression" => MultipleReplaceType.RegularExpression,
            _ => MultipleReplaceType.CaseInsensitive,
        };
    }
}
