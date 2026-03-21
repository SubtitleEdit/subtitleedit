namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageMultipleReplace
{

    public string Title { get; set; }
    public string EditRule { get; set; }
    public string NewRule { get; set; }
    public string EditCategory { get; set; }
    public string NewCategory { get; set; }
    public string CategoryName { get; set; }
    public string ExportReplaceRules { get; set; }
    public string AppliedRules { get; set; }
    public string FindRule { get; set; }
    public string XLinesAffected { get; set; }

    public LanguageMultipleReplace()
    {
        Title = "Multiple replace";
        EditRule = "Edit rule";
        EditCategory = "Edit category";
        NewRule = "New rule";
        NewCategory = "New category";
        CategoryName = "Category name";
        ExportReplaceRules = "Export rules";
        AppliedRules = "Applied rules";
        FindRule = "Find rule";
        XLinesAffected = "{0:#,##0} lines affected";
    }
}