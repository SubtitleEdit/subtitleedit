namespace Nikse.SubtitleEdit.Logic.Config;

public class SeEdit
{
    public SeEditMultipleReplace MultipleReplace { get; set; } = new SeEditMultipleReplace();
    public SeEditFind Find { get; set; } = new SeEditFind();
    public string ModifySelectionMode { get; set; }
    public string ModifySelectionRule { get; set; }
    public string ModifySelectionText { get; set; }
    public bool ModifySelectionMatchCase { get; set; }
    public double ModifySelectionNumber { get; set; }

    public SeEdit()
    {
        ModifySelectionMode = "New";
        ModifySelectionRule = "Contains";
        ModifySelectionText = string.Empty;
    }
}