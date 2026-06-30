using Nikse.SubtitleEdit.Features.Options.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

/// <summary>
/// Imports multiple-replace rules from a CSV file written by <see cref="CsvExporter"/>.
/// Expects the columns Category,Find,ReplaceWith,Description,Active,Type (a header row is
/// optional - columns are matched by name when present, otherwise by position). RFC 4180 quoting
/// is honored so patterns with commas, quotes or newlines round-trip.
/// </summary>
public static class CsvImporter
{
    public static CategoryImportExportItem Import(string csv)
    {
        var result = new CategoryImportExportItem
        {
            Categories = new List<CategoryImportExportItem.RuleImportExportCategory>(),
        };

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

        var byCategory = new Dictionary<string, CategoryImportExportItem.RuleImportExportCategory>(StringComparer.Ordinal);
        var order = new List<string>();

        for (var r = hasHeader ? 1 : 0; r < rows.Count; r++)
        {
            var row = rows[r];
            if (row.Count == 0 || row.All(string.IsNullOrEmpty))
            {
                continue;
            }

            string Get(int i) => i >= 0 && i < row.Count ? row[i] : string.Empty;

            var find = Get(findIdx);
            if (string.IsNullOrEmpty(find))
            {
                continue; // a rule must have something to find
            }

            var categoryName = Get(catIdx).Trim();
            if (string.IsNullOrEmpty(categoryName))
            {
                categoryName = "Default";
            }

            if (!byCategory.TryGetValue(categoryName, out var category))
            {
                category = new CategoryImportExportItem.RuleImportExportCategory
                {
                    Name = categoryName,
                    Rules = new List<CategoryImportExportItem.RuleImportExportItem>(),
                };
                byCategory[categoryName] = category;
                order.Add(categoryName);
            }

            category.Rules!.Add(new CategoryImportExportItem.RuleImportExportItem
            {
                Find = find,
                ReplaceWith = Get(replaceIdx),
                Description = Get(descIdx),
                IsActive = ParseBool(Get(activeIdx)),
                Type = ParseType(Get(typeIdx)),
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

    private static string ParseType(string value)
    {
        return Enum.TryParse<MultipleReplaceType>(value.Trim(), true, out var type)
            ? type.ToString()
            : MultipleReplaceType.CaseInsensitive.ToString();
    }

    // RFC 4180 CSV parser: handles quoted fields with embedded commas, doubled quotes ("") and newlines.
    private static List<List<string>> ParseCsv(string text)
    {
        // Strip a leading UTF-8 BOM if present (Excel writes one).
        if (text.Length > 0 && text[0] == '﻿')
        {
            text = text.Substring(1);
        }

        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
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
