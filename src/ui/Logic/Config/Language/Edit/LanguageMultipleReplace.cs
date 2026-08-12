namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageMultipleReplace
{
    public string EditRule { get; set; }
    public string NewRule { get; set; }
    public string EditCategory { get; set; }
    public string NewCategory { get; set; }
    public string CategoryName { get; set; }
    public string ExportReplaceRules { get; set; }
    public string ImportReplaceRules { get; set; }
    public string NoCategoriesSelected { get; set; }
    public string AppliedRules { get; set; }
    public string FindRule { get; set; }
    public string XLinesAffected { get; set; }
    public string DeleteCategoryConfirm { get; set; }
    public string DeleteRuleConfirm { get; set; }
    public string FindWhat { get; set; }
    public string DescriptionOptional { get; set; }
    public string InvalidRegularExpressionX { get; set; }
    public string RegularExpressionTooSlowX { get; set; }

    public LanguageMultipleReplace()
    {
        EditRule = "Edit rule";
        EditCategory = "Edit category";
        NewRule = "New rule";
        NewCategory = "New category";
        CategoryName = "Category name";
        ExportReplaceRules = "Export rules";
        ImportReplaceRules = "Import rules";
        NoCategoriesSelected = "No rule categories selected";
        AppliedRules = "Applied rules";
        FindRule = "Find rule";
        XLinesAffected = "{0:#,##0} lines affected";
        DeleteCategoryConfirm = "Delete category '{0}'?";
        DeleteRuleConfirm = "Delete rule '{0}'?";
        FindWhat = "Find what";
        DescriptionOptional = "Description (optional)";
        InvalidRegularExpressionX = "Invalid regular expression: {0}";
        RegularExpressionTooSlowX = "Regular expression gave up after {0} seconds - the rule is skipped, try a simpler pattern";
    }
}