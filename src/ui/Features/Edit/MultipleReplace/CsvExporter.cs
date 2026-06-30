using Nikse.SubtitleEdit.Features.Options.Settings;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Edit.MultipleReplace;

/// <summary>
/// Exports multiple-replace rules to a portable CSV file.
/// Columns: Category,Find,ReplaceWith,Description,Active,Type
/// Values are RFC 4180 quoted so patterns containing commas, quotes or newlines round-trip.
/// </summary>
public static class CsvExporter
{
    public const string Header = "Category,Find,ReplaceWith,Description,Active,Type";

    public static string Export(CategoryImportExportItem item)
    {
        var sb = new StringBuilder();
        sb.Append(Header).Append("\r\n");

        if (item.Categories != null)
        {
            foreach (var category in item.Categories)
            {
                if (category.Rules == null)
                {
                    continue;
                }

                foreach (var rule in category.Rules)
                {
                    sb.Append(Escape(category.Name)).Append(',');
                    sb.Append(Escape(rule.Find)).Append(',');
                    sb.Append(Escape(rule.ReplaceWith)).Append(',');
                    sb.Append(Escape(rule.Description)).Append(',');
                    sb.Append(rule.IsActive ? "true" : "false").Append(',');
                    sb.Append(Escape(rule.Type)).Append("\r\n");
                }
            }
        }

        return sb.ToString();
    }

    private static string Escape(string? value)
    {
        value ??= string.Empty;
        if (value.IndexOfAny(['"', ',', '\n', '\r']) >= 0)
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
