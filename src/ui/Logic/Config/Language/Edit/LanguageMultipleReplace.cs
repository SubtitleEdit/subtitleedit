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
    public string RegularExpressionTooSlowX { get; set; }
    public string MoveToCategory { get; set; }
    public string AddRules { get; set; }
    public string AddRulesDotDotDot { get; set; }
    public string AddRulesToX { get; set; }
    public string XRules { get; set; }
    public string DiscardXRules { get; set; }
    public string EnterAddsRule { get; set; }
    public string Categories { get; set; }
    public string NewestFirst { get; set; }
    public string FilterRules { get; set; }
    public string RulesInX { get; set; }
    public string EnterAddsRuleToCategory { get; set; }

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
        RegularExpressionTooSlowX = "Regular expression gave up after {0} seconds - the rule is skipped, try a simpler pattern";
        MoveToCategory = "Move to category";
        AddRules = "Add rules";
        AddRulesDotDotDot = "Add rules...";
        AddRulesToX = "Add rules to '{0}'";
        XRules = "{0:#,##0} rules";
        DiscardXRules = "Discard the {0} rules entered?";
        EnterAddsRule = "Enter adds the rule to the list, OK adds the list to the category (Ctrl+Enter)";
        Categories = "Categories";
        NewestFirst = "Newest first";
        FilterRules = "Filter rules...";
        RulesInX = "Rules in '{0}'";
        EnterAddsRuleToCategory = "Enter adds the rule to the end of the category";
    }
}