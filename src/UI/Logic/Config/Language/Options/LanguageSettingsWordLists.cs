namespace Nikse.SubtitleEdit.Logic.Config.Language.Options;

public class LanguageSettingsWordLists
{
    public string Title { get; set; }
    public string AddName { get; set; }
    public string AddWord { get; set; }
    public string AddPair { get; set; }
    public string NameAndIgnoreList { get; set; }
    public string UserWords { get; set; }
    public string OcrFixList { get; set; }
    public string UnableToAddItem { get; set; }
    public string UnableToRemoveItem { get; set; }

    public LanguageSettingsWordLists()
    {
        Title = "Word lists";
        AddName = "Add name";
        AddWord = "Add word";
        AddPair = "Add pair";
        NameAndIgnoreList = "Name/ignore list";
        UserWords = "User word list";
        OcrFixList = "OCR fix list";
        UnableToAddItem = "Unable to add item, it probably already exists!";
        UnableToRemoveItem = "Unable to remove item!";
    }
}