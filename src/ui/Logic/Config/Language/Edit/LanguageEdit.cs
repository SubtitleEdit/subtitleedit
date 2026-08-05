namespace Nikse.SubtitleEdit.Logic.Config.Language.Edit;

public class LanguageEdit
{
    public LanguageModifySelection ModifySelection { get; set; } = new();
    public LanguageMultipleReplace MultipleReplace { get; set; } = new();
    public LanguageEditFind Find { get; set; } = new();
    public LanguageRegularExpressionContextMenu RegularExpressionContextMenu { get; set; } = new();
    public string ShowHistory { get; set; }
    public string RestoreSelected { get; set; }
    public string NoSubtitleSelected { get; set; }
    public string PleaseSelectExactlyOneSubtitle { get; set; }

    public LanguageEdit()
    {
        ShowHistory = "History for undo";
        RestoreSelected = "Restore selected";
        NoSubtitleSelected = "No subtitle selected";
        PleaseSelectExactlyOneSubtitle = "Please select exactly one subtitle.";
    }
}